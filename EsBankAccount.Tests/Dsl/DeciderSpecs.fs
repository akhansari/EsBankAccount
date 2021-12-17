namespace EsBankAccount.Tests.Domain

open Diffract

type DeciderSpecState<'State, 'Outcome> =
    { State: 'State
      Outcome: 'Outcome }

type DeciderSpecList<'State, 'Command, 'Event when 'Event: equality>
    (initialState: 'State
    , evolve: 'State -> 'Event -> 'State
    , decide: 'Command -> 'State -> 'Event list)
    =

    member _.Yield _ : DeciderSpecState<'State, 'Event list> =
        { State = initialState
          Outcome = [] }

    [<CustomOperation "Given">]
    member _.Given (spec, events) =
        { spec with State = List.fold evolve spec.State events }

    [<CustomOperation "When">]
    member _.When (spec, command) =
        let events = decide command spec.State
        { spec with
            State = List.fold evolve spec.State events
            Outcome = events }

    [<CustomOperation "Then">]
    member _.Then (spec, expected: 'Event list) =
        if not (expected = spec.Outcome) then
            Diffract.Assert (expected, spec.Outcome)
        spec

type DeciderSpecResult<'State, 'Command, 'Event, 'Error
        when 'Event: equality and 'Error: equality>
    (initialState: 'State
    , evolve: 'State -> 'Event -> 'State
    , decide: 'Command -> 'State -> Result<'Event list, 'Error>)
    =

    member _.Yield _ : DeciderSpecState<'State, Result<'Event list, 'Error>> =
        { State = initialState
          Outcome = Ok [] }

    [<CustomOperation "Given">]
    member _.Given (spec, events) =
        { spec with State = List.fold evolve spec.State events }

    [<CustomOperation "When">]
    member _.When (spec, command) =
        let result = decide command spec.State
        { spec with
            State =
                match result with
                | Ok events -> List.fold evolve spec.State events
                | Error _   -> spec.State
            Outcome = result }

    [<CustomOperation "Then">]
    member _.ThenOk (spec, expected: 'Event list) =
        if not (Ok expected = spec.Outcome) then
            Diffract.Assert (Ok expected, spec.Outcome)
        spec

    [<CustomOperation "ThenError">]
    member _.ThenError (spec, expected: 'Error) =
        if not (Error expected = spec.Outcome) then
            Diffract.Assert (Error expected, spec.Outcome)
        spec
