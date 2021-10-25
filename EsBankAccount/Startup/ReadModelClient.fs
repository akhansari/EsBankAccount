module EsBankAccount.Startup.ReadModelClient

open EsBankAccount.Infra

let transactionsOf accountId =
    async {
        use conn = Database.createReadConnection ()
        return! Database.transactionsOf conn accountId
    }
