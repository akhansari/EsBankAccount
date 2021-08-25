namespace EsBankAccount.App

open System

open EsBankAccount.Domain

type AccountId = string

type AccountModel =
    { Id: AccountId
      State: string }

type TransactionModel =
    { AccountId: AccountId
      Date: DateTime
      Amount: decimal
      Balance: decimal }

module BankAccountProjector =

    type Agent = MailboxProcessor<AccountId * BankAccount.Event * BankAccount.State>

    let create addTransaction =
        Agent.Start (fun inbox ->
            let rec loop () = async {
                let! accountId, event, state = inbox.Receive ()

                match event with
                | BankAccount.Deposited transaction ->
                    do! addTransaction
                            { AccountId = accountId
                              Date = transaction.Date
                              Amount = transaction.Amount
                              Balance = state.Balance }
                | BankAccount.Withdrawn transaction ->
                    do! addTransaction
                            { AccountId = accountId
                              Date = transaction.Date
                              Amount = -transaction.Amount
                              Balance = state.Balance }
                | BankAccount.Closed _ ->
                    ()

                do! loop ()
            }
            loop ()
        )
