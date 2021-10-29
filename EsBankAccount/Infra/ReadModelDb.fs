[<RequireQualifiedAccess>]
/// A fake relational database
module EsBankAccount.Infra.ReadModelDb

open System
open System.Collections.Generic

type AcountStateModel = Open | Closed

type TransactionModel =
    { AccountId: string
      Date: DateTime
      Amount: decimal
      Balance: decimal }

[<NoComparison>]
type Db =
    { Accounts: IDictionary<string, AcountStateModel>
      Transactions: ResizeArray<TransactionModel> }
    interface IDisposable with
        member _.Dispose () = ()

let private connection =
    lazy
    { Accounts = Dictionary ()
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
