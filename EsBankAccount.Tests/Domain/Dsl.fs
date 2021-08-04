namespace EsBankAccount.Tests.Domain

open Swensen.Unquote
open EsBankAccount.Domain

type DeciderSpecState<'State, 'Outcome> =
    { State: 'State
      Outcome: 'Outcome }

type DeciderSpec<'State, 'Command, 'Event>
    (initialState: 'State,
    decide: Decide<'Command, 'State, 'Event list>,
    build: Build<'State, 'Event>)
    =

    member _.Yield _ : DeciderSpecState<'State, 'Event list> =
        { State = initialState
          Outcome = [] }

    [<CustomOperation "Given">]
    member _.Given (spec, events) =
        { spec with State = build spec.State events }

    [<CustomOperation "GivenState">]
    member _.GivenState (spec, state) =
        { spec with State = state }

    [<CustomOperation "When">]
    member _.When (spec, command) =
        let events = decide command spec.State
        { spec with
            State = build spec.State events
            Outcome = events }

    [<CustomOperation "Then">]
    member _.ThenEvents (spec, expected) =
        let events = spec.Outcome
        test <@ events = expected @>
        spec

    [<CustomOperation "ThenState">]
    member _.ThenState (spec, expected) =
        let state = spec.State
        test <@ state = expected @>
        spec

type DeciderSpecResult<'State, 'Command, 'Event, 'Error>
    (initialState: 'State,
    decide: Decide<'Command, 'State, Result<'Event list, 'Error>>,
    build: Build<'State, 'Event>)
    =

    member _.Yield _ : DeciderSpecState<'State, Result<'Event list, 'Error>> =
        { State = initialState
          Outcome = Ok [] }

    [<CustomOperation "Given">]
    member _.Given (spec, events) =
        { spec with State = build spec.State events }

    [<CustomOperation "GivenState">]
    member _.GivenState (spec, state) =
        { spec with State = state }

    [<CustomOperation "When">]
    member _.When (spec, command) =
        let result = decide command spec.State
        { spec with
            State =
                match result with
                | Ok events -> build spec.State events
                | Error _ -> spec.State
            Outcome = result }

    [<CustomOperation "Then">]
    member _.ThenOk (spec, expected) =
        let result = spec.Outcome
        test <@ result = Ok expected @>
        spec

    [<CustomOperation "ThenError">]
    member _.ThenError (spec, expected) =
        let result = spec.Outcome
        test <@ result = Error expected @>
        spec

    [<CustomOperation "ThenState">]
    member _.ThenState (spec, expected) =
        let state = spec.State
        test <@ state = expected @>
        spec
