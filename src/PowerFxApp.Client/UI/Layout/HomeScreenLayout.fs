module PowerFxApp.Client.UI.Layout.HomeScreenLayout

open Bolero

type private ViewTemplate = Template<const(__SOURCE_DIRECTORY__ + "/HomeScreenLayout.html")>

let template (node:Node) =
   ViewTemplate()
      .PowerFxTable(node)
      .Elt()
