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
        do! deposit accountId 10m |> Async.Ignore
        do! withdraw accountId 5m |> Async.Ignore
        let! account = ReadModelClient.getAccount accountId
        account.Transactions.Length =! 2
    }
