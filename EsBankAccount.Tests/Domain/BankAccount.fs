module EsBankAccount.Tests.Domain.BankAccount

open System
open Xunit

open EsBankAccount.Domain.BankAccount

let spec = DeciderSpecResult (State.Initial, decide, build)

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
            { Balance = 150m }
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
            { Balance = -25m }
    }

[<Fact>]
let ``when withdrawing, the threshold limit should not be exceeded`` () =
    let thresholdLimit = -500m
    spec {
        GivenState
            { Balance = -400m }
        When
            ( Withdraw (100m, DateTime.MinValue, Some thresholdLimit) )
        Then
            [ Withdrawn { Amount = 100m; Date = DateTime.MinValue } ]
        ThenState
            { Balance = thresholdLimit }
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
            { Balance = 135m }
    }

[<Fact>]
let ``close the account and withdraw the remaining amount`` () =
    spec {
        GivenState
            { Balance = 100m }
        When
            ( Close DateTime.MinValue )
        Then
            [ Withdrawn { Amount = 100m; Date = DateTime.MinValue }
              Closed DateTime.MinValue ]
    }

[<Fact>]
let ``close the account but do not withdraw if nothing left`` () =
    spec {
        GivenState
            { Balance = 0m }
        When
            ( Close DateTime.MinValue )
        Then
            [ Closed DateTime.MinValue ]
    }

[<Fact>]
let ``negative balance cannot be closed`` () =
    spec {
        GivenState
            { Balance = -50m }
        When
            ( Close DateTime.MinValue )
        ThenError
            ( BalanceIsNegative -50m )
    }
