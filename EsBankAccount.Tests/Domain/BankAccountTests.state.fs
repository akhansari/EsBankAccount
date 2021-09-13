module EsBankAccount.Tests.Domain.BankAccountStateTests

open System
open Xunit

open EsBankAccount.Domain.BankAccount

let spec = DeciderStateSpecResult (State.initial, evolve, decide)

[<Fact>]
let ``make a deposit and calculate the balance`` () =
    spec {
        Given
            { State.initial with Balance = 100m }
        When
            ( Deposit (50m, DateTime.MinValue) )
        Then
            { State.initial with Balance = 150m }
    }

[<Fact>]
let ``make a withdrawal and calculate the balance`` () =
    spec {
        Given
            { State.initial with Balance = -10m }
        When
            ( Withdraw (15m, DateTime.MinValue, None) )
        Then
            { State.initial with Balance = -25m }
    }

[<Fact>]
let ``calculate the balance of withdrawals and deposits`` () =
    spec {
        GivenEvents
            [ Deposited { Amount =  50m; Date = DateTime.MinValue }
              Withdrawn { Amount =  10m; Date = DateTime.MinValue }
              Withdrawn { Amount =   5m; Date = DateTime.MinValue }
              Deposited { Amount = 100m; Date = DateTime.MinValue } ]
        Then
            { State.initial with Balance = 135m }
    }

[<Fact>]
let ``close the account and update the state`` () =
    spec {
        When
            ( Close DateTime.MinValue )
        Then
            { State.initial with IsClosed = true }
    }
