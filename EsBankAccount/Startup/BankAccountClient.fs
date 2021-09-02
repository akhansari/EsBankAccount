/// A very basic client
module EsBankAccount.Startup.BankAccountClient

open System

open EsBankAccount.Domain
open EsBankAccount.App
open EsBankAccount.Infra

let handleCommand conn accountId command =
    async {
        let key : EventStore.StreamKey =
            { Name = BankAccount.DeciderName
              Id = accountId }
        return!
            CommandHandler.tryHandle
                (EventStore.read   conn key)
                (EventStore.append conn key)
                BankAccount.evolve
                BankAccount.decide
                BankAccount.State.initial
                command
    }

let handleOutcome publishEvent accountId = function
    | Ok data ->
        for (event, state) in data do
            publishEvent (accountId, event, state)
    | Error error ->
        printfn $"Error: {error}"

// connections are usually disposable
// but since this is an isolated demo, ok to be static
let connections = ()
let private esConn = EventStore.createConnection ()
let private rmConn = Database.createConnection ()
let private projector =
    BankAccountProjector.project
        { AddTransaction = Database.addTransaction rmConn }
    |> MessageQueue.createEventHandler

let handle accountId command =
    async {
        let! outcome = handleCommand esConn accountId command
        return handleOutcome projector.Post accountId outcome
    }

let deposit accountId amount =
    (amount, DateTime.Now)
    |> BankAccount.Deposit
    |> handle accountId

let withdraw accountId amount =
    (amount, DateTime.Now, None)
    |> BankAccount.Withdraw
    |> handle accountId

let transactionsOf =
    Database.transactionsOf rmConn

let listenToEvents callback =
    esConn.Publisher.Subscribe callback
