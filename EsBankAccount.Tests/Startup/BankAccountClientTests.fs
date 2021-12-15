module EsBankAccount.Tests.Startup.BankAccountClientTests

open System
open Swensen.Unquote
open Xunit

open EsBankAccount.Startup
open EsBankAccount.Startup.BankAccountClient

// Startup tests are usually of the type integration

let fakeAccountId () = string Guid.Empty

[<Fact>]
let ``Should deposit and then withdraw`` () =
    async {
        let accountId = fakeAccountId ()
        let! _depositResult = deposit accountId 10m
        let! _withdrawalResult = withdraw accountId 5m
        let! account = ReadModelClient.readAccount accountId
        account.Transactions.Length =! 2
    }
