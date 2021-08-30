module EsBankAccount.Tests.App.CommandHandlerTests

open Swensen.Unquote
open Xunit

open EsBankAccount.App

type Event = FakeEvent
type Command = FakeCommand
type State = { Foo: int }
let initialState = { Foo = 0 }
let createFakeStore (col: Event seq) =
    let store = ResizeArray col
    let read = List.ofSeq store |> async.Return
    let append es = store.AddRange es |> async.Return
    read, append

[<Fact>]
let ``handle should return one event`` () =
    async {
        let read, append = createFakeStore []
        let evolve _ _ = { Foo = 1 }
        let decide _ _ = [ FakeEvent ]
        let! res =
            CommandHandler.handle
                read append evolve decide initialState FakeCommand
        res =! [ FakeEvent, { Foo = 1 } ]
    }

[<Fact>]
let ``handle should return multiple events`` () =
    async {
        let read, append = createFakeStore [ FakeEvent ]
        let counter = ref 0
        let evolve _ _ = incr counter; { Foo = !counter }
        let decide _ _ = [ FakeEvent; FakeEvent ]
        let! res =
            CommandHandler.handle
                read append evolve decide initialState FakeCommand
        res =! [ FakeEvent, { Foo = 2 }; FakeEvent, { Foo = 3 } ]
    }

[<Fact>]
let ``tryHandle should return one ok event`` () =
    async {
        let read, append = createFakeStore []
        let evolve _ _ = { Foo = 1 }
        let decide _ _ = Ok [ FakeEvent ]
        let! res =
            CommandHandler.tryHandle
                read append evolve decide initialState FakeCommand
        res =! Ok [ FakeEvent, { Foo = 1 } ]
    }

[<Fact>]
let ``tryHandle should return multiple ok events`` () =
    async {
        let read, append = createFakeStore [ FakeEvent ]
        let counter = ref 0
        let evolve _ _ = incr counter; { Foo = !counter }
        let decide _ _ = Ok [ FakeEvent; FakeEvent ]
        let! res =
            CommandHandler.tryHandle
                read append evolve decide initialState FakeCommand
        res =! Ok [ FakeEvent, { Foo = 2 }; FakeEvent, { Foo = 3 } ]
    }

[<Fact>]
let ``tryHandle should return error`` () =
    async {
        let read, append = createFakeStore []
        let evolve s _ = s
        let decide _ _ = Error ()
        let! res =
            CommandHandler.tryHandle
                read append evolve decide initialState FakeCommand
        res =! Error ()
    }
