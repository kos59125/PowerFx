[<RequireQualifiedAccess>]
module PowerFxApp.Client.UI.InputFile

open Microsoft.AspNetCore.Components.Forms
open Bolero

type ViewModel = { File: IBrowserFile option }

let getFile model = model.File

type Message = FileChanged of IBrowserFile

let init () = { File = None }

let update message model =
    match message with
    | FileChanged (file) -> { File = Some(file) }

type ViewTemplate = Template<const(__SOURCE_DIRECTORY__ + "/InputFile.html")>

let view model dispatch =
    ViewTemplate()
        .InputFile(
            Html.comp<InputFile>
                [
                    Html.attr.classes [
                        "file-input"
                    ]
                    Html.attr.callback "OnChange" (fun (e: InputFileChangeEventArgs) -> FileChanged(e.File) |> dispatch)
                ]
                []
        )
        .Elt()

type UIComponent() =
    inherit ElmishComponent<ViewModel, Message>()
    override _.View model dispatch = view model dispatch
