module EsBankAccount.Domain.Decider

let rec evolveAndZip evolve state events =
    match events with
    | [] -> []
    | head :: tail ->
        let newState = evolve state head
        (head, newState) :: evolveAndZip evolve newState tail
