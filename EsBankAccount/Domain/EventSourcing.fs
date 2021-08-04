namespace EsBankAccount.Domain

/// Given the current state and what happened, evolve to a new state.
type Evolve<'State, 'Event> = 'State -> 'Event -> 'State
/// Given what has been requested and the current state, decide what should happen.
type Decide<'Command, 'State, 'Outcome> = 'Command -> 'State -> 'Outcome
/// Given the current state and the history, build the new state.
type Build<'State, 'Event> = 'State -> 'Event list -> 'State
/// Given the history, rebuild the current state.
type Rebuild<'State, 'Event> = 'Event list -> 'State

[<RequireQualifiedAccess>]
module EventSourcing =

    (*
    this function seems not interesting
    but it allows signature checking
    and easily find deciders
    *)
    let createDsl<'State, 'Command, 'Event, 'Outcome>
        /// Initial (empty) state we will start with.
        initialState
        (evolve: Evolve<'State, 'Event>)
        (decide: Decide<'Command, 'State, 'Outcome>)
        =

        let build = List.fold evolve
        let rebuild = build initialState
        let handle command = rebuild >> decide command

        (build, rebuild, handle)
