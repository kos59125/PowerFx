[<RequireQualifiedAccess>]
module PowerFxApp.Client.UI.InputFormula

open Bolero

type ViewModel =
    {
        Formula: string
    }

let getFormula model = model.Formula

type Message =
    | SetText of string
    | ClearText
    | ButtonClicked

let init () =
    {
        Formula = ""
    }

let update message model =
    match message with
    | SetText(text) -> { model with Formula = text }
    | ClearText -> { model with Formula = "" }
    | ButtonClicked -> model

let clearText = ClearText |> update

type ViewTemplate = Template<const(__SOURCE_DIRECTORY__ + "/InputFormula.html")>

let view model dispatch =
    ViewTemplate()
        .Formula(model.Formula, (SetText >> dispatch))
        .ButtonClick(fun _ -> ButtonClicked |> dispatch)
        .Elt()

type UIComponent() =
    inherit ElmishComponent<ViewModel, Message>()
    override _.View model dispatch = view model dispatch
