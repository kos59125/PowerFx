module PowerFxApp.Client.Application

open Bolero
open Elmish
open PowerFxApp.Client.Feature
open PowerFxApp.Client.Screen

type ViewModel =
    {
        Page: Page
        Screen: HomeScreen.ViewModel
    }

type Message =
    | SetPage of Page
    | ReceiveScreenMessage of HomeScreen.Message
    | ReceiveCommonMessage of CommonMessage option

let init () =
    {
        Page = Home
        Screen = HomeScreen.init ()
    }

let update message model =
    match message with
    | SetPage (page) -> { model with Page = page }, Cmd.none
    | ReceiveScreenMessage (msg) ->
        let screen, screenCmd, commonMessage = HomeScreen.update msg model.Screen
        let model = { model with Screen = screen }
        let cmd = Cmd.batch [
            Cmd.map ReceiveScreenMessage screenCmd
            Cmd.ofMsg <| ReceiveCommonMessage commonMessage
        ]
        model, cmd
    | ReceiveCommonMessage(None) ->
        model, Cmd.none
    | ReceiveCommonMessage(Some(msg)) ->
        match msg with
        | ShowError(errorMessage) -> eprintfn "%s" errorMessage
        model, Cmd.none

let router =
    Router.infer SetPage (fun model -> model.Page)

let view model dispatch =
    Html.ecomp<HomeScreen.ScreenComponent, _, _> [] model.Screen (ReceiveScreenMessage >> dispatch)

type ApplicationComponent() =
    inherit ProgramComponent<ViewModel, Message>()

    override _.Program =
        Program.mkProgram (fun _ -> init (), Cmd.none) update view
        |> Program.withRouter router
