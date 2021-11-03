[<RequireQualifiedAccess>]
module EsBankAccount.View.Main

open System
open Elmish
open Bolero
open Bolero.Html

open EsBankAccount.Infra
open EsBankAccount.Startup


type HomeModel =
    { Accounts: ReadModelDb.AccountsModel
      AccountId: string option }
module HomeModel =
    let empty =
        { Accounts = List.empty
          AccountId = None }

type AccountModel =
    { AccountId: string
      Transactions: ReadModelDb.TransactionModel list
      TransactionAmount: decimal option }
module AccountModel =
    let empty id =
        { AccountId = id
          Transactions = List.empty
          TransactionAmount = None }

type Model =
    { State: State
      Events: (FsCodec.StreamName * string) list }
and State =
    | HomeView of HomeModel
    | AccountView of AccountModel
module Model =
    let initial =
        { State = HomeView HomeModel.empty
          Events = List.empty }

type Message =
    // home
    | SetAccounts of ReadModelDb.AccountsModel
    | SetAccountId of string
    | OpenAccount
    // account
    | GetAccountInfo
    | GotAccountInfo of ReadModelDb.TransactionModel list
    | SwitchAccount
    | SetTransactionAmount of decimal
    | Deposit
    | Withdraw
    // common
    | AddEvent of FsCodec.StreamName * string

let updateHome model stateModel = { model with State = HomeView stateModel }
let updateAccount model stateModel = { model with State = AccountView stateModel }
let doNothing model = model, Cmd.none

let update message model : Model * Cmd<Message> =
    match message, model.State with

    | SetAccounts accounts, HomeView homeModel ->
        { homeModel with Accounts = accounts } |> updateHome model,
        Cmd.none
    | SetAccountId accountId, HomeView homeModel ->
        { homeModel with AccountId = Some accountId } |> updateHome model,
        Cmd.none
    | OpenAccount, HomeView homeModel ->
        match homeModel.AccountId with
        | Some accountId when String.IsNullOrWhiteSpace accountId |> not ->
            { model with State = AccountModel.empty accountId |> AccountView },
            Cmd.ofMsg GetAccountInfo
        | _ ->
            doNothing model

    | GetAccountInfo, AccountView accountModel ->
        model,
        Cmd.OfAsync.perform ReadModelClient.transactionsOf accountModel.AccountId GotAccountInfo
    | GotAccountInfo transactions, AccountView accountModel ->
        { accountModel with Transactions = transactions } |> updateAccount model,
        Cmd.none

    | SwitchAccount, AccountView _ ->
        { Model.initial with Events = model.Events },
        Cmd.OfAsync.perform ReadModelClient.getAccounts () SetAccounts

    | SetTransactionAmount amount, AccountView accountModel ->
        { accountModel with TransactionAmount = Some amount } |> updateAccount model,
        Cmd.none
    | Deposit, AccountView accountModel ->
        model,
        match accountModel.TransactionAmount with
        | Some amount when amount > 0m ->
            Cmd.OfAsync.perform
                ((<||) BankAccountClient.deposit)
                (accountModel.AccountId, amount)
                (fun () -> GetAccountInfo)
        | _ ->
            Cmd.none
    | Withdraw, AccountView accountModel ->
        model,
        match accountModel.TransactionAmount with
        | Some amount when amount > 0m ->
            Cmd.OfAsync.perform
                ((<||) BankAccountClient.withdraw)
                (accountModel.AccountId, amount)
                (fun () -> GetAccountInfo)
        | _ ->
            Cmd.none

    | AddEvent (key, event), _ ->
        { model with Events = (key, event) :: model.Events },
        Cmd.none

    | _ ->
        $"{message.GetType().Name}, {model.State.GetType().Name}"
        |> NotSupportedException |> raise


let initialView (model: HomeModel) dispatch =
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
    let accounts =
        btable [] [
            thead [] [ tr [] [
                th [] [ text "Account" ]
                th [] [ text "State" ]
            ] ]
            tbody [] [
            forEach model.Accounts <| fun (accountId, accountState) -> tr [] [
                td [] [ text accountId ]
                td [] [
                    cond accountState <| function
                    | ReadModelDb.Opened ->
                        button
                            [ on.click (fun _ ->
                                SetAccountId accountId |> dispatch
                                OpenAccount |> dispatch)
                              css "button is-small is-rounded" ]
                            [ text "open" ]
                    | ReadModelDb.Closed ->
                         button
                            [ attr.disabled 0
                              css "button is-small is-rounded" ]
                            [ text "closed" ] ]
            ] ]
        ]
    concat [
        div [ css "is-size-4 block" ] [ text "Account" ]
        bfieldIcoBtn "user" inputNode buttonNode
        cond model.Accounts.IsEmpty <| function
        | true  -> empty
        | false -> accounts
    ]

let accountView (model: AccountModel) dispatch =
    concat [

    div [ css "is-flex is-align-items-center block" ] [
        div [ css "is-size-4" ] [ text model.AccountId ]
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
                [ bind.input.decimal
                    (defaultArg model.TransactionAmount 0m)
                    (SetTransactionAmount >> dispatch)
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
            | HomeView homeModel -> initialView homeModel dispatch
            | AccountView accountModel -> accountView accountModel dispatch
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
