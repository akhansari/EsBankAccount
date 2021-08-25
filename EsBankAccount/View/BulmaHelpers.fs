[<AutoOpen>]
module EsBankAccount.View.BulmaHelpers

open Bolero.Html

let css = attr.``class``
let bcolumns attrs = div [ yield! attrs; css "columns" ]
let bcolumn attrs = div [ yield! attrs; css "column container" ]
let btable attrs = table [ yield! attrs; css "table" ]
let btableContainer attrs children =
    div [ yield! attrs; css "table-container" ] [ btable [] children ]

let bfieldIco icon inputNode =
    div [ css "field has-addons" ] [
        div [ css "control has-icons-left" ] [
            inputNode
            span [ css "icon is-small is-left" ] [
                i [ css $"fas fa-{icon}" ] []
            ]
        ]
    ]

let bfieldIcoBtn icon inputNode buttonNode =
    div [ css "field has-addons" ] [
        div [ css "control has-icons-left" ] [
            inputNode
            span [ css "icon is-small is-left" ] [
                i [ css $"fas fa-{icon}" ] []
            ]
        ]
        div [ css "control" ] [
            buttonNode
        ]
    ]
