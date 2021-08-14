[<RequireQualifiedAccess>]
module EsBankAccount.View.Main

open System
open Elmish
open Bolero
open Bolero.Html

type TransactionModel =
    { Date: DateTime
      Amount: decimal
      Balance: decimal }

type Model =
    { Foo: int option }
[<RequireQualifiedAccess>]
module Model =
    let initial =
        { Foo = None }

type Message =
    | Hello

let update message model =
    match message with
    | Hello -> model, Cmd.none

let view model dispatch =
    columns [
        column [
            text "Dashboard"
        ]
        column [
            text "Event Store"
        ]
    ]

let project initial =
    let sub dispatch =
        fun data ->
            for event, state in data do
                dispatch Hello
        |> BankAccountHandler.projector.Subscribe
    Cmd.ofSub sub

type Component () =
    inherit ProgramComponent<Model, Message>()
    override _.Program =
        Program.mkProgram
            (fun _ -> Model.initial, Cmd.none)
            update
            view
       |> Program.withSubscription project
