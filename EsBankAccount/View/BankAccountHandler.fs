module EsBankAccount.View.BankAccountHandler

open EsBankAccount.Domain
open EsBankAccount.App
open EsBankAccount.Infra

let private conn = EventStore.createConnection ()

let projector = Publisher ()
let notifier  = Publisher ()

let handle id command =
    async {
        let key : EventStore.StreamKey =
            { Name = BankAccount.DeciderName
              Id = id }

        let handle command =
            CommandHandler.tryHandle
                (EventStore.read conn key)
                (EventStore.append conn key)
                BankAccount.evolve
                BankAccount.decide
                BankAccount.State.initial
                command

        match! handle command with
        | Ok    data  -> projector.Post data
        | Error error -> notifier.Post  error
    }
