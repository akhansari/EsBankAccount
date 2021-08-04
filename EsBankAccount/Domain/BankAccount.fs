module EsBankAccount.Domain.BankAccount

open System

type Money = decimal

type Transaction =
    { Amount: Money
      Date: DateTime }

type Event =
    | Deposited of Transaction
    | Withdrawn of Transaction
    | Closed    of DateTime

type State =
    { Balance: decimal }
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
    | Deposit  of amount: Money * date: DateTime
    | Withdraw of amount: Money * date: DateTime 
    | Close    of date: DateTime

let decide command state =
    match command with
    | Deposit (amount, date) ->
        [ Deposited { Amount = amount; Date = date } ]
    | Withdraw (amount, date) ->
        [ Withdrawn { Amount = amount; Date = date } ]
    | Close date ->
        [ if state.Balance > 0m then
            Withdrawn { Amount = state.Balance; Date = date }
          Closed date ]

let build, rebuild, handle =
    EventSourcing.createDsl State.Initial evolve decide
