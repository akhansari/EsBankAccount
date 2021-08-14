[<RequireQualifiedAccess>]
module EsBankAccount.App.CommandHandler

let private getCurrentState
    (read: Async<'Event list>)
    (evolve: 'State -> 'Event -> 'State)
    initialState
    =
    async {
        let! history = read
        return List.fold evolve initialState history
    }

let rec private evolveAndZip evolve state events =
    match events with
    | [] -> []
    | head :: tail ->
        let newState = evolve state head
        (head, newState) :: evolveAndZip evolve newState tail

let private handleEvents
    (append: 'Event list -> Async<unit>)
    (evolve: 'State -> 'Event -> 'State)
    currentState events
    =
    async {
        do! append events
        return evolveAndZip evolve currentState events
    }

let private genericHandle
    read
    evolve
    (decide: 'Command -> 'State -> 'Outcome)
    handleOutcome
    initialState command
    =
    async {
        let! currentState = getCurrentState read evolve initialState
        let outcome = decide command currentState
        return! handleOutcome currentState outcome
    }

let handle
    read append
    evolve decide
    initialState command
    =
    let handleOutcome = handleEvents append evolve
    genericHandle read evolve decide handleOutcome initialState command

let tryHandle
    read append
    evolve decide
    initialState command
    =
    let handleOutcome currentState outcome =
        async {
            match outcome with
            | Ok events ->
                let! data = handleEvents append evolve currentState events
                return Ok data
            | Error error ->
                return Error error
        }
    genericHandle read evolve decide handleOutcome initialState command
