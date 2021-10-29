[<RequireQualifiedAccess>]
module EsBankAccount.View.Main

open System
open Elmish
open Bolero
open Bolero.Html

open EsBankAccount.App
open EsBankAccount.Infra
open EsBankAccount.Startup


type State =
    | Initial
    | AccountOpened of string

type Model =
    { State: State
      AccountId: string option
      Transactions: ReadModelDb.TransactionModel list
      TransactionAmount: decimal
      Events: (FsCodec.StreamName * string) list }

[<RequireQualifiedAccess>]
module Model =
    let initial =
        { State = Initial
          AccountId = None
          Transactions = List.empty
          TransactionAmount = 1m
          Events = List.empty }

type Message =
    | AddEvent of FsCodec.StreamName * string

    | SetAccountId of string
    | OpenAccount
    | GetAccountInfo
    | GotAccountInfo of ReadModelDb.TransactionModel list
    | SwitchAccount

    | SetTransactionAmount of decimal
    | Deposit
    | Withdraw

let update message model : Model * Cmd<Message> =
    match message with

    | AddEvent (key, event) ->
        { model with Events = (key, event) :: model.Events },
        Cmd.none

    | SetAccountId accountId ->
        { model with AccountId = Some accountId },
        Cmd.none
    | OpenAccount ->
        match model.AccountId with
        | Some accountId when String.IsNullOrWhiteSpace accountId |> not ->
            { model with State = AccountOpened accountId },
            Cmd.ofMsg GetAccountInfo
        | _ ->
            { model with State = Initial },
            Cmd.none
    | GetAccountInfo ->
        model,
        match model.State with
        | AccountOpened accountId ->
            Cmd.OfAsync.perform ReadModelClient.transactionsOf accountId GotAccountInfo
        | _ ->
            Cmd.none
    | GotAccountInfo transactions ->
        { model with Transactions = List.rev transactions },
        Cmd.none
    | SwitchAccount ->
        { Model.initial with Events = model.Events },
        Cmd.none

    | SetTransactionAmount amount ->
        { model with TransactionAmount = amount },
        Cmd.none
    | Deposit ->
        model,
        match model.State with
        | AccountOpened accountId when model.TransactionAmount > 0m ->
            Cmd.OfAsync.perform
                ((<||) BankAccountClient.deposit)
                (accountId, model.TransactionAmount)
                (fun () -> GetAccountInfo)
        | _ ->
            Cmd.none
    | Withdraw ->
        model,
        match model.State with
        | AccountOpened accountId when model.TransactionAmount > 0m ->
            Cmd.OfAsync.perform
                ((<||) BankAccountClient.withdraw)
                (accountId, model.TransactionAmount)
                (fun () -> GetAccountInfo)
        | _ ->
            Cmd.none

let initialView model dispatch =
    let inputNode =
        input [
            bind.input.string
                (defaultArg model.AccountId String.Empty)
                (SetAccountId >> dispatch)
            attr.placeholder "Account name"
            attr.``type`` "text"
            css "input" ]
    let buttonNode =
        button
            [ on.click (fun _ -> dispatch OpenAccount)
              css "button is-info" ]
            [ text "Open" ]
    concat [
        div [ css "is-size-4 block" ] [ text "Account" ]
        bfieldIcoBtn "user" inputNode buttonNode
    ]

let accountView model dispatch =
    concat [

    div [ css "is-flex is-align-items-center block" ] [
        div [ css "is-size-4" ] [ text model.AccountId.Value ]
        button
            [ on.click (fun _ -> dispatch SwitchAccount)
              css "button is-small is-rounded ml-3" ]
            [ text "switch" ]
        button
            [ on.click ignore
              css "button is-small is-rounded ml-3" ]
            [ text "close" ]
    ]

    div [ css "field is-grouped" ] [
        div [ css "control" ] [
            input
                [ bind.input.decimal model.TransactionAmount (SetTransactionAmount >> dispatch)
                  attr.``type`` "number"
                  attr.min 1
                  attr.placeholder "Amount"
                  css "input" ]
            |> bfieldIco "euro-sign"
        ]
        div [ css "control" ] [
            button
                [ on.click (fun _ -> dispatch Deposit)
                  css "button is-info" ]
                [ text "Deposit" ]
        ]
        div [ css "control" ] [
            button
                [ on.click (fun _ -> dispatch Withdraw)
                  css "button is-info"]
                [ text "Withdraw" ]
        ]
    ]

    btable [] [
        thead [] [ tr [] [
            th [] [ text "Date" ]
            th [] [ text "Amount" ]
            th [] [ text "Balance" ]
        ] ]
        tbody [] [
        forEach model.Transactions <| fun transact -> tr [] [
            td [] [ transact.Date.ToString "yyyy-MM-dd HH:mm" |> text ]
            td [] [ transact.Amount |> string |> text ]
            td [] [ transact.Balance |> string |> text ]
        ] ]
    ]

    ]

let eventsView model =
    forEach model.Events <| fun (key, event) ->
        p [] [
            text $"{key} :"
            pre [] [ text event ]
        ]

let view model dispatch =
    bcolumns [] [
        bcolumn [] [
            cond model.State <| function
            | Initial -> initialView model dispatch
            | AccountOpened _ -> accountView model dispatch
        ]
        bcolumn [] [
            div [ css "is-size-4 block" ] [ text "Event Store" ]
            eventsView model
        ]
    ]

let listenToEvents _ =
    fun dispatch ->
        AddEvent >> dispatch
        |> BankAccountClient.listenToEvents
    |> Cmd.ofSub

type Component () =
    inherit ProgramComponent<Model, Message>()
    override _.Program =
        Program.mkProgram (fun _ -> Model.initial, Cmd.none) update view
        |> Program.withSubscription listenToEvents
