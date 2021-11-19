module PowerFxApp.Client.UI.InputFileTable

open Microsoft.AspNetCore.Components.Forms
open Bolero
open Elmish

type ViewModel =
    {
        InputFile: InputFile.ViewModel
        InputFormula: InputFormula.ViewModel
        Table: Table.ViewModel
    }

let getTable model =
    Table.getHeader model.Table, Table.getRecordList model.Table
let getFormula model =
    InputFormula.getFormula model.InputFormula

type Message =
    | ReceiveInputFileMessage of InputFile.Message
    | ReceiveInputFormulaMessage of InputFormula.Message
    | ReceiveTableMessage of Table.Message
    | FileChanged of IBrowserFile
    | ButtonClicked
    | SetTable of string list * Map<string, obj> list
    | AppendColumn of string * obj list

let init () =
    {
        InputFile = InputFile.init ()
        InputFormula = InputFormula.init ()
        Table =
            Table.init ()
            |> Table.setHeader [
                "x"
                "y"
               ]
            |> Table.appendRecord (
                Map.ofList [
                    "x", box 1.0
                    "y", box 11.0
                ]
            )
            |> Table.appendRecord (
                Map.ofList [
                    "x", box 2.0
                    "y", box 22.0
                ]
            )
    }

let update message model =
    match message with
    | ReceiveInputFileMessage (msg) ->
        let model =
            { model with InputFile = InputFile.update msg model.InputFile }

        let cmd =
            match msg with
            | InputFile.FileChanged (file) -> Cmd.ofMsg <| FileChanged(file)

        model, cmd
    | ReceiveInputFormulaMessage (msg) ->
        let model =
            { model with InputFormula = InputFormula.update msg model.InputFormula }

        let cmd =
            match msg with
            | InputFormula.ButtonClicked -> Cmd.ofMsg ButtonClicked
            | _ -> Cmd.none

        model, cmd
    | ReceiveTableMessage (msg) ->
        let model =
            { model with Table = Table.update msg model.Table }

        model, Cmd.none
    | FileChanged (_) -> model, Cmd.none
    | ButtonClicked -> model, Cmd.none
    | SetTable(header, recordList) ->
        let table = Table.clear model.Table |> Table.setHeader header |> List.fold (fun table record -> Table.appendRecord record table) <| recordList
        let model = { model with Table = table }
        model, Cmd.none
    | AppendColumn(columnName, values) ->
        let table = Table.appendColumn (columnName, values) model.Table
        let model = { model with Table = table }
        model, Cmd.none
    
let setTable = SetTable >> update
let appendColumn = AppendColumn >> update

type ViewTemplate = Template<const(__SOURCE_DIRECTORY__ + "/InputFileTable.html")>

let view model dispatch =
    ViewTemplate()
        .InputFile(Html.ecomp<InputFile.UIComponent, _, _> [] model.InputFile (ReceiveInputFileMessage >> dispatch))
        .InputFormula(Html.ecomp<InputFormula.UIComponent, _, _> [] model.InputFormula (ReceiveInputFormulaMessage >> dispatch))
        .Table(Html.ecomp<Table.UIComponent, _, _> [] model.Table (ReceiveTableMessage >> dispatch))
        .Elt()

type UIComponent() =
    inherit ElmishComponent<ViewModel, Message>()
    override _.View model dispatch = view model dispatch
