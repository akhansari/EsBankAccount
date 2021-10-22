/// A very basic client
module EsBankAccount.Startup.BankAccountClient

open System
open System.Text

open EsBankAccount.Domain
open EsBankAccount.App
open EsBankAccount.Infra

module private Client =
    open System.Text.Json
    open System.Text.Json.Serialization
    open Equinox
    open FsCodec
    open Serilog

    let logger = LoggerConfiguration().CreateLogger ()
    let store = MemoryStore.VolatileStore ()
    let codec =
        let jso = JsonSerializerOptions ()
        JsonFSharpConverter JsonUnionEncoding.FSharpLuLike |> jso.Converters.Add
        jso.WriteIndented <- true
        SystemTextJson.Codec.Create jso
    let cat = MemoryStore.MemoryStoreCategory (store, codec, Seq.fold BankAccount.evolve, BankAccount.State.initial)
    let streamName accountId = StreamName.create BankAccount.DeciderName accountId
    let resolve accountId = Decider (logger, cat.Resolve (streamName accountId), maxAttempts = 1)

    let readModelConn = Database.createConnection ()
    let project =
        BankAccountProjector.project
            { AddTransaction = Database.addTransaction readModelConn }

    let decide accountId state command =
        async {
            match BankAccount.decide command state with
            | Ok events ->
                let data = Decider.evolveAndZip BankAccount.evolve state events
                for (event, state) in data do
                    do! project accountId event state
                return (), events
            | Error _ ->
                return (), List.empty
        }

let deposit accountId amount =
    fun state ->
        (amount, DateTime.Now)
        |> BankAccount.Deposit
        |> Client.decide accountId state
    |> (Client.resolve accountId).TransactAsync

let withdraw accountId amount =
    fun state ->
        (amount, DateTime.Now, None)
        |> BankAccount.Withdraw
        |> Client.decide accountId state
    |> (Client.resolve accountId).TransactAsync

let transactionsOf =
    Database.transactionsOf Client.readModelConn

let listenToEvents callback =
    Client.store.Committed.Add <| fun (streamName, events) ->
        let sb = StringBuilder ()
        for event in events do
            sb.AppendLine(event.EventType).AppendLine(string event.Data) |> ignore
        callback (string streamName, string sb)
