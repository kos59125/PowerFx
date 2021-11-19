[<RequireQualifiedAccess>]
module PowerFxApp.Client.UI.Table

open Bolero

type ViewModel =
    {
        Header: string list
        RecordList: Map<string, obj> list
    }

let getHeader model = model.Header
let getRecordList model = model.RecordList

type Message =
    | Clear
    | SetHeader of string list
    | AppendRecord of Map<string, obj>
    | AppendColumn of string * obj list

let init () =
    {
        Header = []
        RecordList = []
    }

let update message model =
    match message with
    | Clear -> { model with Header = []; RecordList = [] }
    | SetHeader (columnNameList) -> { model with Header = columnNameList }
    | AppendRecord (record) -> { model with RecordList = model.RecordList @ [ record ] }
    | AppendColumn (columnName, valueList) ->
        let header =
            match List.contains columnName model.Header with
            | true -> model.Header
            | false -> model.Header @ [ columnName ]

        let recordList =
            List.map2 (fun record value -> Map.add columnName value record) model.RecordList valueList

        { model with
            Header = header
            RecordList = recordList
        }

let clear = Clear |> update
let setHeader = SetHeader >> update
let appendRecord = AppendRecord >> update
let appendColumn = AppendColumn >> update

type ViewTemplate = Template<const(__SOURCE_DIRECTORY__ + "/Table.html")>

let private viewHeaderCell (columnName: string) =
    ViewTemplate.TableHeaderCell().HeaderText(columnName).Elt()

let private viewBodyCell (value: obj option) =
    match value with
    | None -> Html.empty
    | Some (value) ->
        match value with
        | :? float as x ->
            ViewTemplate
                .TableBodyCell()
                .CellClass("has-text-right")
                .CellText(string x)
                .Elt()
        | :? bool as x ->
            ViewTemplate
                .TableBodyCell()
                .CellClass("has-text-center")
                .CellText(string x)
                .Elt()
        | :? string as x -> ViewTemplate.TableBodyCell().CellClass("has-text-left").CellText(x).Elt()
        | _ ->
            ViewTemplate
                .TableBodyCell()
                .CellClass("has-text-left")
                .CellText(string value)
                .Elt()

let view model dispatch =
    ViewTemplate()
        .TableHeader(ViewTemplate.TableRow().Cells(Html.forEach model.Header viewHeaderCell).Elt())
        .TableBody(
            Html.forEach model.RecordList
            <| fun record ->
                ViewTemplate
                    .TableRow()
                    .Cells(Html.forEach model.Header (fun columnName -> Map.tryFind columnName record |> viewBodyCell))
                    .Elt()
        )
        .Elt()

type UIComponent() =
    inherit ElmishComponent<ViewModel, Message>()
    override _.View model dispatch = view model dispatch
