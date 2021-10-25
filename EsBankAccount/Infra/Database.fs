[<RequireQualifiedAccess>]
/// A fake relational database
module EsBankAccount.Infra.Database

open System
open EsBankAccount.App

type Db =
    { Accounts: ResizeArray<AccountModel>
      Transactions: ResizeArray<TransactionModel> }
    interface IDisposable with
        member _.Dispose () = ()

let private connection =
    lazy
    { Accounts = ResizeArray ()
      Transactions = ResizeArray () }

let createReadConnection () =
    connection.Value
let createWriteConnection () =
    connection.Value

let addAccount conn account =
    conn.Accounts.Add account
    async.Return ()

let addTransaction conn transaction =
    conn.Transactions.Add transaction
    async.Return ()

let transactionsOf conn accountId =
    conn.Transactions
    |> Seq.filter (fun t -> t.AccountId = accountId)
    |> Seq.toList
    |> async.Return
