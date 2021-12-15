[<RequireQualifiedAccess>]
/// A fake database
module EsBankAccount.Infra.ReadModelDb

open System
open System.Collections.Concurrent

type TransactionModel =
    { Date: DateTime
      Amount: decimal
      Balance: decimal }

type AccountModel =
    { IsClosed: bool
      Transactions: TransactionModel list }

type AccountsModel = (string * bool) list

[<NoComparison>]
type Db =
    { Accounts: ConcurrentDictionary<string, AccountModel> }
    interface IDisposable with
        member _.Dispose () = ()

let private connection =
    lazy
    { Accounts = ConcurrentDictionary () }

let createReadConnection () =
    connection.Value
let createWriteConnection () =
    connection.Value

let addTransaction conn accountId transaction =
    async {
        conn.Accounts.AddOrUpdate (
            accountId,
            (fun _ ->
                { IsClosed = false
                  Transactions = [ transaction ] }),
            (fun _ account ->
                { account with Transactions = transaction :: account.Transactions })
        )
        |> ignore
    }

let closeAccount conn accountId =
    async {
        match conn.Accounts.TryGetValue accountId with
        | true, model ->
            conn.Accounts.TryUpdate (accountId, { model with IsClosed = true }, model)
            |> ignore
        | _ ->
            ()
    }

let getAccount conn accountId =
    async {
        return conn.Accounts.[accountId]
    }

let getAccounts conn =
    async {
        return
            conn.Accounts
            |> Seq.map (fun (KeyValue (accountId, account)) -> accountId, account.IsClosed)
            |> Seq.toList
    }
