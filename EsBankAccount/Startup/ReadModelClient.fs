module EsBankAccount.Startup.ReadModelClient

open EsBankAccount.App
open EsBankAccount.Infra

let mapTransaction dto : ReadModelDb.TransactionModel =
    { Date = dto.Date
      Amount = dto.Amount
      Balance = dto.Balance }

let getAccount accountId =
    async {
        use conn = ReadModelDb.createReadConnection ()
        return! ReadModelDb.getAccount conn accountId
    }

let getAccounts () =
    async {
        use conn = ReadModelDb.createReadConnection ()
        return! ReadModelDb.getAccounts conn
    }
