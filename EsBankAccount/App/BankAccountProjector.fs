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

    type Dependencies =
        { AddTransaction: TransactionModel -> Async<unit> }

    let project deps accountId event (state: BankAccount.State) =
        async {
            match event with
            | BankAccount.Deposited transaction ->
                do!
                    { AccountId = accountId
                      Date = transaction.Date
                      Amount = transaction.Amount
                      Balance = state.Balance }
                    |> deps.AddTransaction
            | BankAccount.Withdrawn transaction ->
                do!
                    { AccountId = accountId
                      Date = transaction.Date
                      Amount = -transaction.Amount
                      Balance = state.Balance }
                    |> deps.AddTransaction
            | BankAccount.Closed _ ->
                ()
        }
