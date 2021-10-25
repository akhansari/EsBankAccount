module EsBankAccount.Domain.Decider

let rec evolveZip evolve state events =
    match events with
    | [] -> []
    | head :: tail ->
        let newState = evolve state head
        (head, newState) :: evolveZip evolve newState tail
