module EsBankAccount.Startup.ReadModelClient

open EsBankAccount.App
open EsBankAccount.Infra

let mapTransaction dto : ReadModelDb.TransactionModel =
    { Date = dto.Date
      Amount = dto.Amount
      Balance = dto.Balance }

let readAccount accountId =
    async {
        use conn = ReadModelDb.createReadConnection ()
        return! ReadModelDb.getAccount conn accountId
    }

let readAccounts () =
    async {
        use conn = ReadModelDb.createReadConnection ()
        return! ReadModelDb.getAccounts conn
    }
