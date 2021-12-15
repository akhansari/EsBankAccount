namespace EsBankAccount.App

open System

open EsBankAccount.Domain

type AccountId = string

type AccountDto =
    { Id: AccountId
      State: string }

type TransactionDto =
    { Date: DateTime
      Amount: decimal
      Balance: decimal }

module BankAccountProjector =

    [<NoComparison; NoEquality>]
    type Dependencies =
        { AddTransaction: TransactionDto -> Async<unit>
          CloseAccount: Async<unit> }

    let project deps event (state: BankAccount.State) =
        async {
            match event with
            | BankAccount.Deposited transaction ->
                do! { Date = transaction.Date
                      Amount = transaction.Amount
                      Balance = state.Balance }
                    |> deps.AddTransaction
            | BankAccount.Withdrawn transaction ->
                do! { Date = transaction.Date
                      Amount = -transaction.Amount
                      Balance = state.Balance }
                    |> deps.AddTransaction
            | BankAccount.Closed _ ->
                do! deps.CloseAccount
        }

    let handleEvents deps
        (eventAndStates: (BankAccount.Event * BankAccount.State) list) =
        async {
            for (event, state) in eventAndStates do
                do! project deps event state
        }
