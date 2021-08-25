[<RequireQualifiedAccess>]
/// A fake relational database
module EsBankAccount.Infra.Database

open EsBankAccount.App

type Db =
    { Accounts: ResizeArray<AccountModel>
      Transactions: ResizeArray<TransactionModel> }

let createConnection () =
    { Accounts = ResizeArray ()
      Transactions = ResizeArray () }

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
