module EsBankAccount.Domain.BankAccount

open System

let [<Literal>] DeciderName = "BankAccount"

type Amount = decimal
type Money = Amount

type Transaction =
    { Amount: Money
      Date: DateTime }

type Event =
    | Deposited of Transaction
    | Withdrawn of Transaction
    | Closed    of DateTime

type State =
    { Balance: Amount
      IsClosed: bool }

[<RequireQualifiedAccess>]
module State =
    let initial =
        { Balance = 0m
          IsClosed = false }

let evolve state event =
    match event with
    | Deposited transaction ->
        { state with Balance = state.Balance + transaction.Amount }
    | Withdrawn transaction ->
        { state with Balance = state.Balance - transaction.Amount }
    | Closed _ ->
        { state with IsClosed = true }


type Command =
    | Deposit  of Money * DateTime
    | Withdraw of Money * DateTime * thresholdLimit: Amount option
    | Close    of DateTime

type Error =
    | BalanceIsNegative of Amount
    | ThresholdExceeded of upcomingBalance: Amount * thresholdLimit: Amount
    | AlreadyClosed

module private Check =

    let ifClosed state =
        if state.IsClosed
        then Error AlreadyClosed
        else Ok ()

    let ifNegativeBalance state =
        if state.Balance < 0m
        then BalanceIsNegative state.Balance |> Error
        else Ok ()

    let thresholdLimit thresholdLimit amount state =
        match thresholdLimit with
        | Some thresholdLimit when state.Balance - amount < thresholdLimit ->
            ThresholdExceeded (state.Balance - amount, thresholdLimit) |> Error
        | _ ->
            Ok ()

let private deposit amount date =
    [ Deposited { Amount = amount; Date = date } ]

let private withdraw amount date =
    [ Withdrawn { Amount = amount; Date = date } ]

let private close date state =
    [ if state.Balance > 0m then
        Withdrawn { Amount = state.Balance; Date = date }
      Closed date ]

let decide command state =
    result {
        do! Check.ifClosed state
        match command with
        | Deposit (amount, date) ->
            return deposit amount date
        | Withdraw (amount, date, thresholdLimit) ->
            do! Check.thresholdLimit thresholdLimit amount state
            return withdraw amount date
        | Close date ->
            do! Check.ifNegativeBalance state
            return close date state
    }

let isTerminal state =
    state.IsClosed
