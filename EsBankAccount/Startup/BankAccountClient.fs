module EsBankAccount.Startup.BankAccountClient

open System

open EsBankAccount.Domain
open EsBankAccount.App
open EsBankAccount.Infra

let private esConn = EventStore.createConnection ()
let private rmConn = Database.createConnection ()

let private projector =
    BankAccountProjector.create
        (Database.addTransaction rmConn)

let handle id command =
    async {
        let key : EventStore.StreamKey =
            { Name = BankAccount.DeciderName
              Id = id }

        let tryHandle =
            CommandHandler.tryHandle
                (EventStore.read   esConn key)
                (EventStore.append esConn key)
                BankAccount.evolve
                BankAccount.decide
                BankAccount.State.initial
                command

        match! tryHandle with
        | Ok data ->
            for (event, state) in data do
                projector.Post (id, event, state)
        | Error error ->
            printfn $"Error: {error}"
    }

let listenToEvents callback =
    esConn.Publisher.Subscribe callback

let deposit accountId amount =
    handle accountId (BankAccount.Deposit (amount, DateTime.Now))

let withdraw accountId amount =
    handle accountId (BankAccount.Withdraw (amount, DateTime.Now, None))

let transactionsOf =
    Database.transactionsOf rmConn
