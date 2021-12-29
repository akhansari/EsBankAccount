namespace EsBankAccount.Tests.Domain

open Diffract

type DeciderStateSpecList<'State, 'Command, 'Event when 'State: equality>
    (initialState: 'State
    , evolve: 'State -> 'Event -> 'State
    , decide: 'Command -> 'State -> 'Event list)
    =

    member _.Yield _ =
        initialState

    [<CustomOperation "Given">]
    member _.Given (_, newState) =
        newState

    [<CustomOperation "When">]
    member _.When (state, command) =
        decide command state
        |> List.fold evolve state

    [<CustomOperation "Then">]
    member _.Then (state, expected: 'State) =
        if not (expected = state) then
            Diffract.Assert (expected, state)
        state

    [<CustomOperation "Then">]
    member _.Then (state, check: 'State -> bool) =
        Diffract.Assert (true, check state)
        state

    [<CustomOperation "Then">]
    member _.Then (state, check: 'State -> unit) =
        check state
        state

type DeciderStateSpecResult<'State, 'Command, 'Event, 'Error when 'State: equality>
    (initialState: 'State
    , evolve: 'State -> 'Event -> 'State
    , decide: 'Command -> 'State -> Result<'Event list, 'Error>)
    =

    member _.Yield _ =
        initialState

    [<CustomOperation "Given">]
    member _.Given (_, newState: 'State) =
        newState

    [<CustomOperation "Given">]
    member _.Given (state, events) =
        List.fold evolve state events

    [<CustomOperation "When">]
    member _.When (state, command) =
        match decide command state with
        | Ok events -> List.fold evolve state events
        | Error _   -> state

    [<CustomOperation "Then">]
    member _.Then (state, expected: 'State) =
        if not (expected = state) then
            Diffract.Assert (expected, state)
        state

    [<CustomOperation "Then">]
    member _.Then (state, check: 'State -> bool) =
        Diffract.Assert (true, check state)
        state

    [<CustomOperation "Then">]
    member _.Then (state, check: 'State -> unit) =
        check state
        state
