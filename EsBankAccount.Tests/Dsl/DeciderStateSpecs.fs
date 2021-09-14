namespace EsBankAccount.Tests.Domain

open Swensen.Unquote

type DeciderStateSpecList<'State, 'Command, 'Event>
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
    member _.Then (state, expected) =
        test <@ state = expected @>
        state

type DeciderStateSpecResult<'State, 'Command, 'Event, 'Error>
    (initialState: 'State
    , evolve: 'State -> 'Event -> 'State
    , decide: 'Command -> 'State -> Result<'Event list, 'Error>)
    =

    member _.Yield _ =
        initialState

    [<CustomOperation "Given">]
    member _.Given (_, newState) =
        newState

    [<CustomOperation "GivenEvents">]
    member _.GivenEvents (state, events) =
        List.fold evolve state events

    [<CustomOperation "When">]
    member _.When (state, command) =
        match decide command state with
        | Ok events -> List.fold evolve state events
        | Error _   -> state

    [<CustomOperation "Then">]
    member _.Then (state, expected) =
        test <@ state = expected @>
        state
