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
    Assert.True false

[<Fact>]
let ``make a withdrawal`` () =
    Assert.True false

[<Fact>]
let ``close the account`` () =
    Assert.True false

[<Fact>]
let ``close the account and withdraw the remaining amount`` () =
    Assert.True false

(*
    Now switch the DSL from DeciderSpecList to DeciderSpecResult (also for state tests)
    Then refactor the decide function to return a Result<Event list, Error>
*)

[<Fact>]
let ``when withdrawing, the threshold limit should not be exceeded`` () =
    Assert.True false

[<Fact>]
let ``negative balance cannot be closed`` () =
    Assert.True false

[<Fact>]
let ``cannot deposit or withdraw if the account is already closed`` () =
    Assert.True false

[<Fact>]
let ``cannot close an already closed account`` () =
    Assert.True false
