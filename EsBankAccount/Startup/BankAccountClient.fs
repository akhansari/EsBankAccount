/// A very basic client
module EsBankAccount.Startup.BankAccountClient

open System
open System.Text

open EsBankAccount.Domain
open EsBankAccount.App
open EsBankAccount.Infra


module private EquinoxClient =
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


module private Client =

    let decide state command =
        async {
            match BankAccount.decide command state with
            | Ok events ->
                let data = Decider.evolveZip BankAccount.evolve state events
                return data, events
            | Error _ ->
                return [], [] // todo: error handling
        }

    let handle accountId command =
        async {
            // handle command
            let! eventAndStates =
                fun state -> decide state command
                |> (EquinoxClient.resolve accountId).TransactAsync
            // handle events
            use conn = Database.createWriteConnection ()
            do! // todo: idempotency & retry strategy
                BankAccountProjector.handleEvents
                    { AddTransaction = Database.addTransaction conn }
                    accountId eventAndStates
        }


let deposit accountId amount =
    BankAccount.Deposit (amount, DateTime.Now)
    |> Client.handle accountId

let withdraw accountId amount =
    BankAccount.Withdraw (amount, DateTime.Now, None)
    |> Client.handle accountId

// todo: closing

let listenToEvents callback =
    EquinoxClient.store.Committed.Add <| fun (streamName, events) ->
        let sb = StringBuilder ()
        for event in events do
            sb.AppendLine(event.EventType).AppendLine(string event.Data) |> ignore
        callback (string streamName, string sb)
