namespace EsBankAccount.App

open System

open EsBankAccount.Domain

type AccountId = string

type AccountDto =
    { Id: AccountId
      State: string }

type TransactionDto =
    { AccountId: AccountId
      Date: DateTime
      Amount: decimal
      Balance: decimal }

module BankAccountProjector =

    [<NoComparison; NoEquality>]
    type Dependencies =
        { AddTransaction: TransactionDto -> Async<unit> }

    let project deps accountId event (state: BankAccount.State) =
        async {
            match event with
            | BankAccount.Deposited transaction ->
                do! { AccountId = accountId
                      Date = transaction.Date
                      Amount = transaction.Amount
                      Balance = state.Balance }
                    |> deps.AddTransaction
            | BankAccount.Withdrawn transaction ->
                do! { AccountId = accountId
                      Date = transaction.Date
                      Amount = -transaction.Amount
                      Balance = state.Balance }
                    |> deps.AddTransaction
            | BankAccount.Closed _ ->
                ()
        }

    let handleEvents deps accountId
        (eventAndStates: (BankAccount.Event * BankAccount.State) list) =
        async {
            for (event, state) in eventAndStates do
                do! project deps accountId event state
        }
