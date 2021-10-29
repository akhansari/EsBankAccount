[<RequireQualifiedAccess>]
/// A fake database
module EsBankAccount.Infra.ReadModelDb

open System
open System.Collections.Concurrent

type AcountStateModel = Opened | Closed

type TransactionModel =
    { Date: DateTime
      Amount: decimal
      Balance: decimal }

[<NoComparison>]
type AccountModel =
    { State: AcountStateModel
      Transactions: ResizeArray<TransactionModel> }

type AccountsModel = (string * AcountStateModel) list

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
    conn.Accounts.AddOrUpdate (
        accountId,
        (fun _ ->
            { State = Opened
              Transactions = ResizeArray [| transaction |] }),
        (fun _ account ->
            account.Transactions.Add transaction
            account)
    ) |> ignore
    async.Return ()

let transactionsOf conn accountId =
    conn.Accounts.[accountId].Transactions
    |> Seq.toList
    |> async.Return

let getAccounts conn =
    conn.Accounts
    |> Seq.map (fun (KeyValue (accountId, account)) -> accountId, account.State)
    |> Seq.toList
    |> async.Return
