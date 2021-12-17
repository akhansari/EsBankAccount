module EsBankAccount.Tests.Domain.BankAccountTests

open System
open Xunit

open EsBankAccount.Domain.BankAccount

let spec = DeciderSpecResult (State.initial, evolve, decide)

[<Fact>]
let ``make a deposit`` () =
    spec {
        When
            ( Deposit (10m, DateTime.MinValue) )
        Then
            [ Deposited { Amount = 10m; Date = DateTime.MinValue } ]
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
let ``when withdrawing, the threshold limit should not be exceeded`` () =
    let thresholdLimit = -500m
    spec {
        Given
            [ Withdrawn { Amount = 400m; Date = DateTime.MinValue } ]
        When
            ( Withdraw (100m, DateTime.MinValue, Some thresholdLimit) )
        Then
            [ Withdrawn { Amount = 100m; Date = DateTime.MinValue } ]
        When
            ( Withdraw (1m, DateTime.MinValue, Some thresholdLimit) )
        Then
            ( ThresholdExceeded (-501m, thresholdLimit) |> WithdrawalError )
    }

[<Fact>]
let ``close the account`` () =
    spec {
        When
            ( Close DateTime.MinValue )
        Then
            [ Closed {| ClosedOn = DateTime.MinValue |} ]
    }

[<Fact>]
let ``close the account and withdraw the remaining amount`` () =
    spec {
        Given
            [ Deposited { Amount = 100m; Date = DateTime.MinValue } ]
        When
            ( Close DateTime.MinValue )
        Then // assert scenario
            ( function Ok events -> Assert.NotEmpty events | _ -> () )
        Then // true or false scenario
            ( function Ok [ Withdrawn _; Closed _ ] -> true | _ -> false )
        Then // equality then structural diff scenario
            [ Withdrawn { Amount = 100m; Date = DateTime.MinValue }
              Closed {| ClosedOn = DateTime.MinValue |} ]
    }

[<Fact>]
let ``negative balance cannot be closed`` () =
    spec {
        Given
            [ Withdrawn { Amount = 50m; Date = DateTime.MinValue } ]
        When
            ( Close DateTime.MinValue )
        Then
            ( BalanceIsNegative -50m |> ClosingError )
    }

[<Fact>]
let ``cannot deposit or withdraw if the account is already closed`` () =
    spec {
        Given
            [ Closed {| ClosedOn = DateTime.MinValue |} ]
        When
            ( Deposit (10m, DateTime.MinValue) )
        Then
            ( AlreadyClosed )
        When
            ( Withdraw (10m, DateTime.MinValue, None) )
        Then
            ( AlreadyClosed )
    }
