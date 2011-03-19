﻿namespace IntelliFactory.WebSharper.Formlet.JQueryUI

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Html.Events
open IntelliFactory.Formlet.Base
open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.JQuery
open Utils

module Controls =

    module CC = IntelliFactory.WebSharper.Formlet.JQueryUI.CssConstants

    [<JavaScript>]
    let private Reactive = IntelliFactory.Reactive.Reactive.Default

    /// Constructs a button formlet. The integer value of the formlet
    /// indicates the number of clicks. The value 0 is triggered the first time
    /// the button is clicked.
    [<JavaScript>]
    let CustomButton (conf : JQueryUI.ButtonConfiguration) =
        MkFormlet <| fun () ->
            let state = State<int>.New()
            let count = ref 0
            let button = 
                let genEl () = Button[Text conf.Label]
                JQueryUI.Button.New(genEl, conf)
            // Make sure to try to render the button
            try
                (button :> IPagelet).Render()
            with
            | _ -> 
                ()

            button.OnClick(fun _ ->
                state.Trigger (Success count.Value)
                incr count
            )
            let reset () =
                count := 0
                state.Trigger (Failure [])
            button , reset, state

    /// Constructs a button formlet with a label. The integer value of the formlet
    /// indicates the number of clicks. The value 0 is triggered the first time
    /// the button is clicked.
    [<JavaScript>]
    let Button (label : string) =
        JQueryUI.ButtonConfiguration(Label = label)
        |> CustomButton

    [<JavaScript>]
    let Dialog (genEl : unit -> Element) : Formlet<unit> =
        MkFormlet <| fun _ ->
            let state = new Event<_>()
            let dialog =
                JQueryUI.Dialog.New(genEl ())
            dialog.OnClose (fun _ ->
                state.Trigger (Result.Success ())
            )
            dialog.Enable ()
            dialog.Open ()

            let reset () =
                dialog.Close ()
                state.Trigger (Result.Failure [])

            Div [dialog], reset , state.Publish

    /// Constructs an Accordion formlet, displaying the given list of formlets on separate tabs.
    [<JavaScript>]
    let AccordionList (fs: list<string * Formlet<'T>>) : Formlet<list<'T>> =
        MkFormlet <| fun () ->
            let fs = 
                fs 
                |> List.map (fun (l,f) -> 
                    let form, elem = Utils.FormAndElement f
                    l, form, elem
                )
            let state =
                let state = 
                    fs
                    |> List.map (fun (_,f,_) -> f.State)
                    |> Reactive.Sequence
                Reactive.Select state (fun rs ->
                    Result.Sequence rs

                )
            let acc =
                fs
                |> List.map (fun (label, _, elem) -> label , elem)
                |> JQueryUI.Accordion.New
                
            let reset () =
                acc.Activate 0
                fs 
                |> List.iter (fun (_,f,_) -> 
                    f.Notify null
                )
            acc, reset , state



    [<Inline "jQuery($acc).accordion('option','active')">]
    let private ActiveAccordionIndex acc = 0
    
    /// Constructs an Accordion formlet, displaying the given list of formlets on separate tabs.
    [<JavaScript>]
    let AccordionChoose (fs: list<string * Formlet<'T>>) : Formlet<'T> =
        MkFormlet <| fun () ->
            // Keep track of last results
            let res = Array.zeroCreate fs.Length
            let fs = 
                fs 
                |> List.mapi (fun ix (l,f) -> 
                    let form, elem = Utils.FormAndElement f
                    l, (Reactive.Heat form.State), form.Notify, elem
                )
                |> Array.ofList

            let rec state = Event<_>()

            and accordion =
                fs
                |> Array.map (fun (label, _, _, elem) -> label , elem)
                |> List.ofArray
                |> JQueryUI.Accordion.New
                |>! OnAfterRender (fun acc ->
                    update 0
                    acc.Activate 0
                )
            and update index =
                let (_, s, _, _) = fs.[index]
                state.Trigger s
                            
            and reset () =
                fs |> Array.iter (fun (_,_,n,_) -> n null)
                accordion.Activate 0
                update 0

            // Update on tab select
            // OnSelectA accordion update
            accordion.OnChange (fun ev _->
                (accordion :> IPagelet).Body
                |> ActiveAccordionIndex
                |> update
            )

            // State
            let state = 
                Reactive.Switch state.Publish

            accordion, reset , state


    [<JavaScript>]
    let Autocomplete def (source: seq<string>) : Formlet<string> =
        MkFormlet <| fun () ->
            let state = State<string>.New(def)
            let input = 
                let i = Input [Text def]
                let upd () = state.Trigger (Success i.Value)
                i |>! Events.OnKeyUp (fun el _ ->
                    upd ()
                )
                |>! Events.OnMouseOut (fun  el _ ->
                    upd ()
                )
                |>! Events.OnChange (fun _ ->
                    upd ()
                )
                |>! Events.OnMouseLeave (fun el _ ->
                    upd ()
                )
            let ac = 
                JQueryUI.Autocomplete.New(
                    input , 
                    JQueryUI.AutocompleteConfiguration(
                        Source = Array.ofSeq source
                    )
                )
            let reset () =
                input.Value <- def
                state.Trigger (Success def)
            ac, reset, state

    [<JavaScript>]
    let private DatepickerInput showCalendar (def: option<EcmaScript.Date>) : Formlet<EcmaScript.Date> =
        MkFormlet <| fun () ->
            let date = 
                if showCalendar then
                    JQueryUI.Datepicker.New()
                else
                    JQueryUI.Datepicker.New(Input [])
            let defDate =
                match def with
                | Some d -> d
                | None   -> new EcmaScript.Date ()
            let state = State<EcmaScript.Date   >.New()
            date.OnSelect (fun date ->
                state.Trigger (Success date)
            )

            let reset () =
                date.SetDate (unbox defDate)
                match def with
                | Some d -> 
                    state.Trigger (Success d)
                | None   -> 
                    state.Trigger (Failure [])
            date 
            |> OnAfterRender (fun _ ->
                reset ()
            )            
            date, reset, state

    [<JavaScript>]
    let Datepicker (def: option<EcmaScript.Date>) : Formlet<EcmaScript.Date> =
        DatepickerInput true def

    [<JavaScript>]
    let InputDatepicker (def: option<EcmaScript.Date>) : Formlet<EcmaScript.Date> =
        DatepickerInput false def



    type Orientation =
        | [<Name "vertical">] Vertical
        | [<Name "horizontal">] Horizontal
    

    type SliderConfiguration =
        {
            [<Name "animate">]
            Animate : bool
            [<Name "orientation">]
            Orientation : Orientation
            [<Name "value">]
            Value : int
            [<Name "min">]
            Min : int
            [<Name "max">]
            Max : int
            [<Name "width">]
            Width : option<int>
            [<Name "height">]
            Height : option<int>
        }         
        with
            [<JavaScript>]
            static member Default =
                {
                    Animate = false
                    Orientation = Horizontal
                    Value = 0
                    Min = 0
                    Max = 100
                    Width = None
                    Height = None
                }

    /// ...
    [<JavaScript>]
    let Slider (conf: option<SliderConfiguration>) : Formlet<int> =
        MkFormlet <| fun () ->
            let conf =
                match conf with
                | Some c    -> c
                | None      -> SliderConfiguration.Default
            
            let style =
                let width w = "width: " + (string w) + "px;"
                let height h = "height: " + (string h) + "px;"
                match conf.Width, conf.Height with
                | Some w, Some h -> 
                    [Attr.Style (width w + height h)]
                | Some w, None -> 
                    [Attr.Style (width w)]
                | None, Some h ->
                    [Attr.Style (height h)]
                | None, None    ->
                    []

            let slider = JQueryUI.Slider.New(unbox (box conf))
            let state = State<_>.New()
            slider.OnChange(fun _ ->
                state.Trigger (Success slider.Value)
            )
            let reset () =
                slider.Value <- conf.Value
                state.Trigger (Success conf.Value)
            
            let slider = 
                Div style -< [slider]
                |>! OnAfterRender (fun _ -> 
                    reset ()
                )
            slider, reset, state
            
    open System
    
    [<Inline "jQuery($list.element.Body).sortable('toArray')">]
    let private ToArray list =  [||]

    type private C =
        {
            stop : JQuery -> unit
        }

    [<JavaScript>]
    let Sortable (fs: List<Formlet<'T>>)  =
        Formlet.BuildFormlet <| fun () ->
            let dict = System.Collections.Generic.Dictionary<string,  IObservable<Result<'T>> >()
            let stateEv = Event<list<string>>()
            let state =
                Reactive.Select stateEv.Publish (fun ids -> 
                    let x =
                        ids
                        |> List.map (fun id -> dict.[id])
                        |> Reactive.Sequence
                    Reactive.Select x List.ofSeq
                )
                |> Reactive.Switch
            
            let state = Reactive.Select state Result.Sequence
                            
            let list = OL [Attr.Style "list-style-type: none; margin: 0; padding: 0;"];
            let stopEv = Event<_>()
            let config  = {stop = fun _ -> stopEv.Trigger ()}
            let sortList = JQueryUI.Sortable.New(list, unbox box config)

            // Trigger new state based on the order
            let update () =
                ToArray sortList
                |> List.ofArray
                |> stateEv.Trigger

            stopEv.Publish.Subscribe (fun _ -> update ())
            |> ignore

            let reset () =
                dict.Clear()
                list.Clear()
                fs
                |> List.map (fun formlet ->
                    let form, elem = Utils.FormAndElement formlet

                    let id = NewId ()
                    dict.[id] <- Reactive.Heat form.State
                    let icon = Span [Attr.Class "ui-icon ui-icon-arrowthick-2-n-s"] 
                    let tbl = Table [TBody [TR [TD [icon] ; TD [ elem]]]]
                    LI [Attr.Id id] -< [tbl]
                    
                    
                )
                |> List.iter (fun el -> list.Append el)
                update ()

            let list = list |>! OnAfterRender (fun _ -> reset ())
            list, reset, state
    
    /// Constructs an Tabs formlet, displaying the given list of formlets on separate tabs.
    [<JavaScript>]
    let TabsList (fs: list<string * Formlet<'T>>) : Formlet<list<'T>> =
        MkFormlet <| fun () ->
            let fs = 
                fs 
                |> List.map (fun (l,f) -> 
                    let (form, elem) = Utils.FormAndElement f
                    l, form, elem
                )            
            let state =
                let state = 
                    fs
                    |> List.map (fun (_,f,_) -> f.State)
                    |> Reactive.Sequence
                Reactive.Select state (fun rs ->
                    Result.Sequence rs
                    
                )   
            let tabs =
                fs
                |> List.map (fun (label, _, elem) -> label , elem)
                |> JQueryUI.Tabs.New
                
            let reset () =
                tabs.Select 0
                fs 
                |> List.iter (fun (_,f,_) -> 
                    f.Notify null
                )
            tabs, reset , state

    [<Inline "jQuery($tabs.element.el).tabs({select: function(x,ui){$f(ui.index)}})">]
    let private onSelect tabs (f : int -> unit) = ()

    [<JavaScript>]
    let private OnSelect tabs f = 
        tabs
        |> OnAfterRender (fun _ -> onSelect tabs f)
    

    [<JavaScript>]
    let TabsChoose (fs: list<string * Formlet<'T>>) : Formlet<'T> =
        MkFormlet <| fun () ->
            // Keep track of last results
            let res = Array.zeroCreate fs.Length
            let fs = 
                fs 
                |> List.mapi (fun ix (l,f) -> 
                    let form, elem = Utils.FormAndElement f
                    l, (Reactive.Heat form.State), form.Notify, elem
                )
                |> Array.ofList
            
            let tabs =
                fs
                |> Array.map (fun (label, _, _, elem) -> label , elem)
                |> List.ofArray
                |> JQueryUI.Tabs.New
            
            let state = Event<_>()
            let update index =
                let (_, s, _, _) = fs.[index]
                state.Trigger s
                
            OnSelect tabs update
                
            let reset () =
                fs |> Array.iter (fun (_,_,n,_) -> n null)
                tabs.Select 0
                update 0


            tabs.OnSelect (fun ev ui->
                update ui.index
            )

            // Initialize reset
            let tabs = 
                tabs |>! OnAfterRender (fun _ -> reset ())
            tabs, reset , Reactive.Switch state.Publish

    
    type private Dr =
        {
            draggable : JQuery
        }
    type private D =
        {
            accept : string
            drop:   JQuery.Event * Dr -> unit
        }
    
    type DragAndDropConfig = 
        {
            AcceptMany : bool
            DropContainerClass : string
            DragContainerClass : string
            DroppableClass : string
            DraggableClass : string
            DropContainerStyle : option<string>
            DragContainerStyle : option<string>
            DroppableStyle : option<string>
            DraggableStyle : option<string>
        }
        with
            [<JavaScript>]
            static member Default =
                {
                    AcceptMany = true
                    DropContainerClass = CC.DropContainerClass
                    DragContainerClass = CC.DragContainerClass
                    DroppableClass = CC.DroppableClass
                    DraggableClass = CC.DraggableClass
                    DragContainerStyle = None
                    DroppableStyle = None
                    DraggableStyle = None
                    DropContainerStyle = None
                }

    [<JavaScript>]
    let private GetClass (c: option<string>) =
        match c with
        | Some v    -> [Attr.Class v]
        | None      -> []
    
    [<JavaScript>]
    let private GetStyle (c: option<string>) =
        match c with
        | Some v    -> [Attr.Style v]
        | None      -> []
    

    
    [<JavaScript>]
    let DragAndDrop (dc:option<DragAndDropConfig>) (vs: list<string * 'T * bool>) : Formlet<list<'T>> =
        MkFormlet <| fun () ->
            let dc =
                match dc with
                | Some dc   -> dc
                | None      -> DragAndDropConfig.Default

            let resList = ref []
            let state = State<_>.New()

            let dict = System.Collections.Generic.Dictionary<string, string * 'T>()

            let dragElem id label =
                Span (GetStyle dc.DraggableStyle) -<
                [Attr.Class dc.DraggableClass] -< 
                [Attr.Id id] -< [Text label]

            let dropElem id label =
                Span (GetStyle dc.DroppableStyle) -<
                [Attr.Class dc.DroppableClass]-< 
                [Attr.Id id] -< [Text label]

            // Drag
            let dragCnf = JQueryUI.DraggableConfiguration()
            dragCnf.Helper <- "clone"
            
            let idDrs =
                vs
                |> List.map (fun (label, value, added)  ->
                    let id = NewId ()
                    dict.[id] <- (label, value)
                    let elem = dragElem id label
                    id , JQueryUI.Draggable.New(elem, dragCnf)  , added 
                )

            let ids = List.map (fun (x,_,_) -> x) idDrs
            let draggables = List.map (fun (_,y,_) -> y) idDrs
            let initials = List.choose (fun (id,_,add) -> if add then Some id else None) idDrs

            let dropPanel = 
                Div [Attr.Class dc.DropContainerClass] -< 
                (GetStyle dc.DropContainerStyle) -<
                (GetStyle dc.DropContainerStyle)

            let update () =
                resList.Value
                |> List.rev
                |> List.map (fun (_, x) -> x)
                |> Success
                |> state.Trigger

            let addItem id =
                let (label, value) = dict.[id]
                let newId = NewId ()
                
                let isAdded = List.exists (fun (i, _) -> i = id) resList.Value

                if not dc.AcceptMany then
                    JQuery.Of("#" + id).Hide().Ignore

                // Not added or we accept many
                if (not isAdded) || dc.AcceptMany then
                    // New element
                    let elem = 
                        dropElem newId label
                        |>! Events.OnClick (fun el _ ->
                            el.Remove()
                            resList := List.filter (fun (elId, _) -> elId <> newId) resList.Value
                            if not dc.AcceptMany then
                                JQuery.Of("#" + id).Show().Ignore
                            update ()
                        )
                    dropPanel.Append elem
                    resList := (newId, value) :: resList.Value
                    update ()

            // Drop
            let dropCnf = {
                accept = "." + dc.DraggableClass
                drop = fun (ev,d) ->
                    addItem <| (string <| d.draggable.Attr("id"))
            }

            let reset () =
                for id in ids do
                    JQuery.Of("#" + id).Show().Ignore
                    resList := []
                    dropPanel.Clear()
                    List.iter addItem initials
                    update ()


            let droppable = 
                JQueryUI.Droppable.New(dropPanel, unbox box dropCnf)
            
            // Drag Panel
            let dragPanel =
                Div [Attr.Class dc.DragContainerClass] -< 
                (GetStyle dc.DragContainerStyle) -<
                draggables

            let body =
                Div  [dragPanel; dropPanel]
                |>! OnAfterRender (fun _ -> 
                    reset ()
                )
            body, reset, state