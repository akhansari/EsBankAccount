module EsBankAccount.Tests.Startup.BankAccountClientTests

open System
open Swensen.Unquote
open Xunit

open EsBankAccount.Startup
open EsBankAccount.Startup.BankAccountClient

let fakeAccountId () = string Guid.Empty

[<Fact>]
let ``Should deposit and then withdraw`` () =
    async {
        let accountId = fakeAccountId ()
        do! deposit accountId 10m
        do! withdraw accountId 5m
        let! transac = ReadModelClient.transactionsOf accountId
        transac.Length =! 2
    }
