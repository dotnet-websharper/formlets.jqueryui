namespace IntelliFactory.WebSharper.Formlet.JQueryUI

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Formlet
open IntelliFactory.Formlet.Base
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

    [<Inline "windows.alert($x)">]
    let Alert x = ()
    
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
                
    /// Implementation for IObservable representing
    /// the state of controls.
    type internal State<'T> =
        internal 
            {
                Initial : Result<'T>
                Event   : Event<Result<'T>>
            }
        interface IObservable<Result<'T>> with
            
            [<JavaScript>]
            member this.Subscribe(o) =
                o.OnNext(this.Initial)
                let disp =
                    this.Event.Publish.Subscribe(fun v ->
                        o.OnNext(v)
                    )
                disp

        [<JavaScript>]
        member this.Trigger(v) =
            this.Event.Trigger v

        [<JavaScript>]
        [<Name "NewFail">]
        static member New<'U>() : State<'U> =
            {
                Initial = Result.Failure []
                Event = Event<_>()
            }
        
        [<JavaScript>]
        [<Name "NewSuccess">]
        static member New<'U>(v) : State<'U> =
            {
                Initial = Result.Success v
                Event = Event<_>()
            }
        
        [<JavaScript>]
        static member FromResult(res: Result<'U>) : State<'U> =
            {
                Initial = res
                Event = Event<_>()
            } 


