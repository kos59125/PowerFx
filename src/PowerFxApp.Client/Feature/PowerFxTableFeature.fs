[<RequireQualifiedAccess>]
module PowerFxApp.Client.Feature.PowerFxTable

open System
open System.IO
open Microsoft.AspNetCore.Components.Forms
open Microsoft.PowerFx
open Microsoft.PowerFx.Core.Public.Values
open FSharp.Data
open Bolero
open Elmish
open PowerFxApp.Client.UI

type ViewModel = { Table: InputFileTable.ViewModel }

type Message =
    | ReceiveTableMessage of InputFileTable.Message
    | DataLoaded of string list * Map<string, obj> list
    | DataLoadError of exn

let init () = { Table = InputFileTable.init () }

let parseValue (field:string) =
    match Double.TryParse(field) with
    | true, value -> box value
    | false, _ ->
        match Boolean.TryParse(field) with
        | true, value -> box value
        | false, _ -> box field

let rowToMap (headers:string list) (row:CsvRow) =
    Seq.map parseValue row.Columns
    |> Seq.map2 (fun columnName value -> columnName, value) headers
    |> Map.ofSeq

let readCsvAsync (file:IBrowserFile) = task {
    use buffer = new MemoryStream()
    do! file.OpenReadStream(1_000_000_000).CopyToAsync(buffer)
    do buffer.Seek(0L, SeekOrigin.Begin) |> ignore
    use csv = CsvFile.Load(buffer, hasHeaders = true)
    let headers = Option.get csv.Headers |> List.ofArray
    let values = csv.Rows |> Seq.map (rowToMap headers) |> List.ofSeq
    return headers, values
}

let toPowerFxRecordList model =
    let header, recordList = InputFileTable.getTable model.Table
    recordList |> List.map (fun record ->
        header |> List.map (fun columnName ->
            let value = Map.find columnName record
            let formulaValue : FormulaValue =
                match value with
                | :? float as x -> FormulaValue.New(x)
                | :? bool as x -> FormulaValue.New(x)
                | :? string as x -> FormulaValue.New(x)
                | _ -> FormulaValue.New(string value)
            NamedValue(columnName, formulaValue)
        )
        |> RecordValue.RecordFromFields
    )

let evaluate (columnName:string) (formula:string) model =
    let engine = RecalcEngine()
    let values =
        toPowerFxRecordList model
        |> List.map (fun record ->
            let result = engine.Eval(formula, record)
            result.ToObject()
        )
    columnName, values

let update message model =
    match message with
    | ReceiveTableMessage (InputFileTable.FileChanged(file)) ->
        let cmd = Cmd.OfTask.either readCsvAsync file DataLoaded DataLoadError
        model, cmd, None
    | ReceiveTableMessage (InputFileTable.ButtonClicked) ->
        let formula = InputFileTable.getFormula model.Table
        let column = evaluate formula formula model
        let table, uiCmd = InputFileTable.appendColumn column model.Table
        let model = { model with Table = table }
        let cmd = Cmd.map ReceiveTableMessage uiCmd
        model, cmd, None
    | ReceiveTableMessage (msg) ->
        let table, uiCmd = InputFileTable.update msg model.Table
        let model = { model with Table = table }
        let cmd = Cmd.map ReceiveTableMessage uiCmd
        model, cmd, None
    | DataLoaded(headers, recordList) ->
        let table, uiCmd = InputFileTable.setTable (headers, recordList) model.Table
        let model = { model with Table = table }
        let cmd = Cmd.map ReceiveTableMessage uiCmd
        model, cmd, None
    | DataLoadError(ex) ->
        let commonMessage = ShowError($"Failed to load a CSV file. Message=%s{ex.Message}; StackTrace=%s{ex.StackTrace}")
        model, Cmd.none, Some(commonMessage)

let view model dispatch =
    Html.ecomp<InputFileTable.UIComponent, _, _> [] model.Table (ReceiveTableMessage >> dispatch)

type FeatureComponent() =
    inherit ElmishComponent<ViewModel, Message>()
    override _.View model dispatch = view model dispatch
