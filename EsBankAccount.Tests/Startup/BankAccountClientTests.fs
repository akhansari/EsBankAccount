module EsBankAccount.Tests.Startup.BankAccountClientTests

open System
open Swensen.Unquote
open Xunit

open EsBankAccount.Domain
open EsBankAccount.Infra
open EsBankAccount.Startup

let fakeAccountId = string Guid.Empty

[<Fact>]
let ``Command handler should return Ok`` () =
    async {
        let esConn = EventStore.createConnection ()
        let! res =
            BankAccountClient.handleCommand esConn fakeAccountId
                (BankAccount.Deposit (10m, DateTime.MinValue))
        match res with Ok [ (BankAccount.Deposited _ , _) ] -> true | _ -> false
        =! true
    }
