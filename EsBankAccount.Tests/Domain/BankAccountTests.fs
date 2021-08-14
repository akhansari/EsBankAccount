module EsBankAccount.Tests.Domain.BankAccountTests

open System
open Xunit

open EsBankAccount.Domain.BankAccount

let spec = DeciderSpecResult (State.initial, decide, evolve)

[<Fact>]
let ``make a deposit`` () =
    spec {
        When
            ( Deposit (10m, DateTime.MinValue) )
        Then
            [ Deposited { Amount = 10m; Date = DateTime.MinValue } ]
    }

[<Fact>]
let ``make a deposit and calculate the balance`` () =
    spec {
        Given
            [ Deposited { Amount = 100m; Date = DateTime.MinValue } ]
        When
            ( Deposit (50m, DateTime.MinValue) )
        Then
            [ Deposited { Amount = 50m; Date = DateTime.MinValue } ]
        ThenState
            { State.initial with Balance = 150m }
    }

[<Fact>]
let ``make a withdrawal`` () =
    spec {
        When
            ( Withdraw (10m, DateTime.MinValue, None) )
        Then
            [ Withdrawn { Amount = 10m; Date = DateTime.MinValue } ]
    }

[<Fact>]
let ``make a withdrawal and calculate the balance`` () =
    spec {
        Given
            [ Withdrawn { Amount = 10m; Date = DateTime.MinValue } ]
        When
            ( Withdraw (15m, DateTime.MinValue, None) )
        Then
            [ Withdrawn { Amount = 15m; Date = DateTime.MinValue } ]
        ThenState
            { State.initial with Balance = -25m }
    }

[<Fact>]
let ``when withdrawing, the threshold limit should not be exceeded`` () =
    let thresholdLimit = -500m
    spec {
        GivenState
            { State.initial with Balance = -400m }
        When
            ( Withdraw (100m, DateTime.MinValue, Some thresholdLimit) )
        Then
            [ Withdrawn { Amount = 100m; Date = DateTime.MinValue } ]
        ThenState
            { State.initial with Balance = thresholdLimit }
        When
            ( Withdraw (1m, DateTime.MinValue, Some thresholdLimit) )
        ThenError
            ( ThresholdExceeded (-501m, thresholdLimit) )
    }

[<Fact>]
let ``calculate the balance of withdrawals and deposits`` () =
    spec {
        Given
            [ Deposited { Amount =  50m; Date = DateTime.MinValue }
              Withdrawn { Amount =  10m; Date = DateTime.MinValue }
              Withdrawn { Amount =   5m; Date = DateTime.MinValue }
              Deposited { Amount = 100m; Date = DateTime.MinValue } ]
        ThenState
            { State.initial with Balance = 135m }
    }

[<Fact>]
let ``close the account`` () =
    spec {
        When
            ( Close DateTime.MinValue )
        Then
            [ Closed DateTime.MinValue ]
        ThenState
            { State.initial with IsClosed = true }
    }

[<Fact>]
let ``close the account and withdraw the remaining amount`` () =
    spec {
        GivenState
            { State.initial with Balance = 100m }
        When
            ( Close DateTime.MinValue )
        Then
            [ Withdrawn { Amount = 100m; Date = DateTime.MinValue }
              Closed DateTime.MinValue ]
        ThenState
            { State.initial with IsClosed = true }
    }

[<Fact>]
let ``negative balance cannot be closed`` () =
    spec {
        GivenState
            { State.initial with Balance = -50m }
        When
            ( Close DateTime.MinValue )
        ThenError
            ( BalanceIsNegative -50m )
    }

[<Fact>]
let ``cannot deposit or withdraw if the account is already closed`` () =
    spec {
        GivenState
            { State.initial with IsClosed = true }
        When
            ( Deposit (10m, DateTime.MinValue) )
        ThenError
            AlreadyClosed
        When
            ( Withdraw (10m, DateTime.MinValue, None) )
        ThenError
            AlreadyClosed
    }
