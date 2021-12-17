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
      Info: ReadModelDb.AccountModel
      TransactionAmount: decimal option }
module AccountModel =
    let empty id =
        { AccountId = id
          Info = { IsClosed = false; Transactions = List.empty }
          TransactionAmount = None }

type Model =
    { State: State
      Events: (FsCodec.StreamName * string) list
      NotifyModel: Notify.Model }
and State =
    | HomeView of HomeModel
    | AccountView of AccountModel
module Model =
    let initial =
        { State = HomeView HomeModel.empty
          Events = List.empty
          NotifyModel = Notify.Model.initial }

type Message =
    // home
    | SetAccounts of ReadModelDb.AccountsModel
    | SetAccountId of string
    | ViewAccount
    // account
    | GetAccountInfo
    | GotAccountInfo of ReadModelDb.AccountModel
    | SwitchAccount
    | SetTransactionAmount of decimal
    | Deposit
    | Withdraw
    | CloseAccount
    // common
    | AddEvent of FsCodec.StreamName * string
    | WrapNotify of Notify.Message

let updateHome model stateModel = { model with State = HomeView stateModel }
let updateAccount model stateModel = { model with State = AccountView stateModel }
let doNothing model = model, Cmd.none

let private tryGetAccountInfo = function
    | Ok () -> GetAccountInfo
    | Error errorMsg -> Notify.Open (Notify.Warning, errorMsg) |> WrapNotify

let update message model : Model * Cmd<Message> =
    match message, model.State with

    | SetAccounts accounts, HomeView homeModel ->
        { homeModel with Accounts = accounts } |> updateHome model,
        Cmd.none
    | SetAccountId accountId, HomeView homeModel ->
        { homeModel with AccountId = Some accountId } |> updateHome model,
        Cmd.none
    | ViewAccount, HomeView homeModel ->
        match homeModel.AccountId with
        | Some accountId when String.IsNullOrWhiteSpace accountId |> not ->
            { model with State = AccountModel.empty accountId |> AccountView },
            Cmd.ofMsg GetAccountInfo
        | _ ->
            doNothing model

    | GetAccountInfo, AccountView accountModel ->
        model,
        Cmd.OfAsync.perform ReadModelClient.readAccount accountModel.AccountId GotAccountInfo
    | GotAccountInfo info, AccountView accountModel ->
        { accountModel with Info = info } |> updateAccount model,
        Cmd.none

    | SwitchAccount, AccountView _ ->
        { Model.initial with Events = model.Events },
        Cmd.OfAsync.perform ReadModelClient.readAccounts () SetAccounts

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
                tryGetAccountInfo
        | _ ->
            Cmd.none
    | Withdraw, AccountView accountModel ->
        model,
        match accountModel.TransactionAmount with
        | Some amount when amount > 0m ->
            Cmd.OfAsync.perform
                ((<||) BankAccountClient.withdraw)
                (accountModel.AccountId, amount)
                tryGetAccountInfo
        | _ ->
            Cmd.none
    | CloseAccount, AccountView accountModel ->
        model,
        Cmd.OfAsync.perform BankAccountClient.close accountModel.AccountId tryGetAccountInfo

    | AddEvent (key, event), _ ->
        { model with Events = (key, event) :: model.Events },
        Cmd.none

    | WrapNotify msg, _ ->
        let mdl, cmd = Notify.update msg model.NotifyModel
        { model with NotifyModel = mdl }, Cmd.map WrapNotify cmd

    | _ ->
        $"{message.GetType().Name}, {model.State.GetType().Name}"
        |> NotSupportedException |> raise


let homeView (model: HomeModel) dispatch =
    let accounts =
        btable [] [
            thead [] [ tr [] [
                th [] [ text "Account" ]
                th [] [ text "State" ]
            ] ]
            tbody [] [
            forEach model.Accounts <| fun (accountId, isClosed) -> tr [] [
                td
                    [ attr.classes [ if isClosed then "account-closed" ] ]
                    [ text accountId ]
                td [] [
                    button
                        [ on.click (fun _ ->
                              SetAccountId accountId |> dispatch
                              ViewAccount |> dispatch)
                          css "button is-small is-rounded" ]
                        [ text "view" ]
                ]
            ] ]
        ]
    concat [
        div [ css "block" ] [
            p [ css "is-size-4 mb-1" ] [ text "Account" ]
            binputIconButton "user"
                (input [
                    bind.input.string (defaultArg model.AccountId String.Empty) (SetAccountId >> dispatch)
                    attr.placeholder "Account name"
                    attr.``type`` "text"
                    css "input" ])
                (button
                    [ on.click (fun _ -> dispatch ViewAccount)
                      css "button is-info" ]
                    [ text "Open" ])
        ]
        div [ css "block" ] [
            p [ css "is-size-4" ] [ text "Accounts" ]
            cond model.Accounts.IsEmpty <| function
            | true  -> empty
            | false -> accounts
        ]
    ]

let accountView (model: AccountModel) dispatch =
    concat [

    div [ css "is-flex is-align-items-center block" ] [
        div
            [ attr.classes
                [ "is-size-4"
                  if model.Info.IsClosed then "account-closed"
                  ] ]
            [ text model.AccountId ]
        button
            [ on.click (fun _ -> dispatch SwitchAccount)
              css "button is-small is-rounded ml-3" ]
            [ text "switch" ]
        button
            [ on.click (fun _ -> dispatch CloseAccount)
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
            |> binputIcon "euro-sign"
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
        forEach model.Info.Transactions <| fun transact -> tr [] [
            td [] [ transact.Date.ToString "yyyy-MM-dd HH:mm" |> text ]
            td [] [ transact.Amount |> string |> text ]
            td [] [ transact.Balance |> string |> text ]
        ] ]
    ]

    ]

let eventsView model =
    forEach model.Events <| fun (key, event) ->
        p [] [
            text $"{key}"
            pre [] [ text event ]
        ]

let view model dispatch =
    concat [

    bcolumns [] [
        bcolumn [] [
            cond model.State <| function
            | HomeView homeModel -> homeView homeModel dispatch
            | AccountView accountModel -> accountView accountModel dispatch
        ]
        bcolumn [] [
            div [ css "is-size-4 block" ] [ text "Event Store" ]
            eventsView model
        ]
    ]

    Notify.view model.NotifyModel (WrapNotify >> dispatch)

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
