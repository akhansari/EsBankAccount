module EsBankAccount.Tests.Domain.BankAccountTests

open System
open Xunit

open EsBankAccount.Domain.BankAccount

(*
    Start to do type-driven development by adding the minimum types for Event, State and Command.
    Then add the initial state, also evolve and decide functions.
    Now you can start to practice test-driven development.
*)

//let spec = DeciderSpecList<State, Command, Event> (State.initial, evolve, decide)

[<Fact>]
let ``make a deposit`` () =
    (*
    spec {
        When
            ...
        Then
            ...
    }
    *)
    ()

[<Fact>]
let ``make a withdrawal`` () =
    ()

[<Fact>]
let ``close the account`` () =
    ()

[<Fact>]
let ``close the account and withdraw the remaining amount`` () =
    ()

(*
    Now switch the DSL from DeciderSpecList to DeciderSpecResult (also for state tests)
    Then refactor the decide function to return a Result<Event list, Error>
*)

[<Fact>]
let ``when withdrawing, the threshold limit should not be exceeded`` () =
    ()

[<Fact>]
let ``negative balance cannot be closed`` () =
    ()

[<Fact>]
let ``cannot deposit or withdraw if the account is already closed`` () =
    ()

[<Fact>]
let ``cannot close an already closed account`` () =
    ()
