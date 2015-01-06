namespace IntelliFactory.WebSharper.Formlets.JQueryUI

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.JavaScript
open IntelliFactory.WebSharper.Html.Client
open IntelliFactory.WebSharper.Formlets
open IntelliFactory.Formlets.Base
open System


module CssConstants =

    [<JavaScript>]
    let ValidIconClass = "validIcon ui-icon ui-icon-check"

    [<JavaScript>]
    let ErrorIconClass = "errorIcon icon ui-icon ui-icon-alert"
    
    [<JavaScript>]
    let AddIconClass = "addIcon ui-icon ui-icon-circle-plus"

    [<JavaScript>]
    let RemIconClass = "removeIcon ui-icon ui-icon-circle-minus"

    [<JavaScript>]
    let LegendClass = "ui-widget-content ui-corner-all"

    [<JavaScript>]
    let FormContainerClass = "formlet-jqueryui"

    [<JavaScript>]
    let DropContainerClass = "dropContainer"

    [<JavaScript>]
    let DragContainerClass = "dragContainer"

    [<JavaScript>]
    let DraggableClass = "draggableItem"

    [<JavaScript>]
    let DroppableClass = "droppableItem"


module internal Utils =

    [<JavaScript>]
    let BaseFormlet = FormletProvider(UtilsProvider ())

    [<JavaScript>]
    let RX = Formlets.Data.UtilsProvider().Reactive

    [<JavaScript>]
    let RMap f s= RX.Select s f

    [<JavaScript>]
    let RChoose f s=  RX.Choose s f

    [<JavaScript>]
    let MkFormlet f =
        Formlet.BuildFormlet <| fun () ->
            let b, r, s = f ()
            let panel =
                Div [b]
            panel, r, s

    [<JavaScript>]
    let FormAndElement formlet =
        let formlet = Formlet.WithLayoutOrDefault formlet
        let form = Formlet.BuildForm formlet
        match (formlet :> IFormlet<_,_>).Layout.Apply(form.Body) with
        | Some (body, _) -> form, body.Element
        | None           -> form, Div []



