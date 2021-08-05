module EsBankAccount.Domain.BankAccount

open System

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
    static member Initial =
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
        else Ok state

    let ifNegativeBalance state =
        if state.Balance < 0m
        then BalanceIsNegative state.Balance |> Error
        else Ok state

    let thresholdLimit thresholdLimit amount state =
        match thresholdLimit with
        | Some thresholdLimit when state.Balance - amount < thresholdLimit ->
            ThresholdExceeded (state.Balance - amount, thresholdLimit) |> Error
        | _ ->
            Ok state

let private deposit amount date =
    [ Deposited { Amount = amount; Date = date } ]

let private withdraw amount date =
    [ Withdrawn { Amount = amount; Date = date } ]

let private close date state =
    [ if state.Balance > 0m then
        Withdrawn { Amount = state.Balance; Date = date }
      Closed date ]

let private (<!>) r f = Result.map f r
let private (>>=) r f = Result.bind f r

let decide command state =
    match command with
    | Deposit (amount, date) ->
        state
        |>  Check.ifClosed
        <!> fun _ -> deposit amount date
    | Withdraw (amount, date, thresholdLimit) ->
        state
        |>  Check.ifClosed
        >>= Check.thresholdLimit thresholdLimit amount
        <!> fun _ -> withdraw amount date
    | Close date ->
        state
        |>  Check.ifNegativeBalance
        <!> close date

let build, rebuild, handle =
    Decider.createDsl State.Initial evolve decide
