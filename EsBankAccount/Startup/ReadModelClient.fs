module EsBankAccount.Startup.ReadModelClient

open EsBankAccount.App
open EsBankAccount.Infra

let addTransaction conn dto =
    ReadModelDb.addTransaction conn
        dto.AccountId
        { Date = dto.Date
          Amount = dto.Amount
          Balance = dto.Balance }

let transactionsOf accountId =
    async {
        use conn = ReadModelDb.createReadConnection ()
        return! ReadModelDb.transactionsOf conn accountId
    }

let getAccounts () =
    async {
        use conn = ReadModelDb.createReadConnection ()
        return! ReadModelDb.getAccounts conn
    }
