namespace EsBankAccount.Domain

module Result =
    let zip x1 x2 =
        match x1,x2 with
        | Ok x1res, Ok x2res -> Ok (x1res, x2res)
        | Error e, _ -> Error e
        | _, Error e -> Error e

type ResultBuilder () =
    member _.MergeSources (x1, x2) = Result.zip x1 x2
    member _.Bind (x, binder) = Result.bind binder x
    member _.BindReturn (x, mapping) = Result.map mapping x
    member _.Return value = Ok value

[<AutoOpen>]
module Expressions =
    let result = ResultBuilder ()
