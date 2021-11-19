[<RequireQualifiedAccess>]
module PowerFxApp.Client.Screen.HomeScreen

open Bolero
open Elmish
open PowerFxApp.Client.Feature
open PowerFxApp.Client.UI.Layout

type ViewModel = { Layout: Layout1.Model<PowerFxTable.ViewModel> }
type Message = ReceiveFeatureMessage of PowerFxTable.Message

let init () = { Layout = PowerFxTable.init () |> Layout1.init }

let update message model =
    match message with
    | ReceiveFeatureMessage (msg) ->
      let table, featureCmd, commonMessage = Layout1.getComponent model.Layout |> PowerFxTable.update msg
      let model = { model with Layout = Layout1.setComponent table model.Layout }
      let cmd = Cmd.map ReceiveFeatureMessage featureCmd
      model, cmd, commonMessage

let view model dispatch =
   let viewComponent = Html.ecomp<PowerFxTable.FeatureComponent, _, _> []
   Layout1.layout HomeScreenLayout.template viewComponent ReceiveFeatureMessage model.Layout dispatch

type ScreenComponent() =
    inherit ElmishComponent<ViewModel, Message>()
    override _.View model dispatch = view model dispatch
