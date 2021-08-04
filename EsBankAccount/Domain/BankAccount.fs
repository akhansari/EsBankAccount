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
    { Balance: Amount }
    static member Initial =
        { Balance = 0m }

let evolve state event =
    match event with
    | Deposited transaction ->
        { state with Balance = state.Balance + transaction.Amount }
    | Withdrawn transaction ->
        { state with Balance = state.Balance - transaction.Amount }
    | Closed _ ->
        state


type Command =
    | Deposit  of Money * DateTime
    | Withdraw of Money * DateTime * thresholdLimit: Amount option
    | Close    of DateTime

type Error =
    | BalanceIsNegative  of Amount
    | ThresholdExceeded of upcomingBalance: Amount * thresholdLimit: Amount

module private Closing =

    let checkBalance state =
        if state.Balance < 0m
        then BalanceIsNegative state.Balance |> Error
        else Ok state

    let close date state =
        [ if state.Balance > 0m then
            Withdrawn { Amount = state.Balance; Date = date }
          Closed date ]

module private Withdrawal =
    
    let checkThresholdLimit thresholdLimit amount state =
        match thresholdLimit with
        | Some thresholdLimit when state.Balance - amount < thresholdLimit ->
            ThresholdExceeded (state.Balance - amount, thresholdLimit) |> Error
        | _ ->
            Ok state

    let withdraw amount date =
        [ Withdrawn { Amount = amount; Date = date } ]

let decide command state =
    match command with
    | Deposit (amount, date) ->
        Ok [ Deposited { Amount = amount; Date = date } ]
    | Withdraw (amount, date, thresholdLimit) ->
        state
        |> Withdrawal.checkThresholdLimit thresholdLimit amount
        |> Result.map (fun _ -> Withdrawal.withdraw amount date)
    | Close date ->
        state
        |> Closing.checkBalance
        |> Result.map (Closing.close date)

let build, rebuild, handle =
    Decider.createDsl State.Initial evolve decide
