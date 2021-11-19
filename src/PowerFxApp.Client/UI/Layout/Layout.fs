namespace PowerFxApp.Client.UI.Layout

open Microsoft.AspNetCore.Components
open Bolero
open Elmish

module Layout1 =

    type Model<'C> = { Component: 'C }

    type Message<'C, 'M> =
        | SetComponent of 'C
        | ReceiveComponentMessage of 'M

    let init c = { Component = c }

    let getComponent model = model.Component

    let update message model =
        match message with
        | SetComponent (c) -> { model with Component = c }
        | ReceiveComponentMessage (_) -> model

    let setComponent<'C, 'M> = Message<'C, 'M>.SetComponent >> update

    let view<'C, 'M> (template: Node -> Node) (viewComponent: 'C -> Dispatch<'M> -> Node) (model: Model<'C>) (dispatch: Dispatch<Message<'C, 'M>>) =
        let node =
            viewComponent model.Component (ReceiveComponentMessage >> dispatch)

        template node

    type LayoutComponent<'C, 'M>() =
        inherit ElmishComponent<Model<'C>, Message<'C, 'M>>()

        [<Parameter>]
        member val Template: Node -> Node = id with get, set

        [<Parameter>]
        member val ViewComponent: 'C -> Dispatch<'M> -> Node = fun _ _ -> Html.empty with get, set

        override this.View model dispatch =
            view<'C, 'M> this.Template this.ViewComponent model dispatch

    let layout<'L, 'C, 'M, 'A when 'L :> LayoutComponent<'C, 'M>>
        (template: Node -> Node)
        (viewComponent: 'C -> Dispatch<'M> -> Node)
        (escalateMessage: 'M -> 'A)
        (model: Model<'C>)
        (dispatch: Dispatch<'A>)
        =
        Html.ecomp<'L, _, _>
            [
                Attr("Template", template)
                Attr("ViewComponent", viewComponent)
            ]
            model
            (function
            | ReceiveComponentMessage (msg) -> escalateMessage msg |> dispatch
            | _ -> ())
