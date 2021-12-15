[<RequireQualifiedAccess>]
module EsBankAccount.View.Notify

open System
open Elmish
open Bolero
open Bolero.Html

type State = Opened | Closed

type Level = Info | Warning | Danger
module Level =
    let toCss = function
    | Info -> "info"
    | Warning -> "warning"
    | Danger -> "danger"

type Model =
    { State: State
      Level: Level
      Content: string }
module Model =
    let initial =
        { State = Closed
          Level = Info
          Content = String.Empty }

type Message =
    | Open of Level * string
    | Close

let closer =
    async {
        do! TimeSpan.FromSeconds 4.0 |> Async.Sleep
        return Close
    }

let update message (model: Model) =
    match message with
    | Open (level, content) ->
        { State = Opened
          Level = level
          Content = content },
        Cmd.OfAsync.result closer
    | Close ->
        { model with State = Closed }, Cmd.none

type Component () =
    inherit ElmishComponent<Model, Message>()
    override _.View model dispatch =
        div [ attr.style "position: fixed; top: 0; right: 0; max-width: 320px; z-index: 100; margin: 10px;"
              attr.classes
                [ $"notification is-{Level.toCss model.Level}"
                  if model.State = Closed then "is-hidden" ] ]
            [ button
                [ on.click (fun _ -> dispatch Close)
                  attr.``class`` "delete" ]
                []
              text model.Content ]

let view model dispatch =
    ecomp<Component,_,_> List.empty model dispatch
