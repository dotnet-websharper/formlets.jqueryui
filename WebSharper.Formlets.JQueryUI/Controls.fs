// $begin{copyright}
//
// This file is part of WebSharper
//
// Copyright (c) 2008-2018 IntelliFactory
//
// Licensed under the Apache License, Version 2.0 (the "License"); you
// may not use this file except in compliance with the License.  You may
// obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
// implied.  See the License for the specific language governing
// permissions and limitations under the License.
//
// $end{copyright}
namespace WebSharper.Formlets.JQueryUI

open WebSharper
open WebSharper.JavaScript
open WebSharper.Html.Client
open WebSharper.Html.Client.Events
open IntelliFactory.Formlets.Base
open WebSharper.Formlets
open WebSharper.JQuery
open Utils

module Controls =
    open IntelliFactory.Reactive
    module CC = WebSharper.Formlets.JQueryUI.CssConstants



    /// Constructs a button formlet. The integer value of the formlet
    /// indicates the number of clicks. The value 0 is triggered the first time
    /// the button is clicked.
    [<JavaScript>]
    let CustomButton (conf : JQueryUI.ButtonConfiguration) =
        MkFormlet <| fun () ->
            let state = HotStream<_>.New(Failure [])
            let count = ref 0
            let button =
                let genEl () = Button[Text conf.Label]
                JQueryUI.Button.New(genEl, conf)
            // Make sure to try to render the button
            try
                button.Render()
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
            let state = HotStream<_>.New(Failure [])
            let dialog =
                JQueryUI.Dialog.New(genEl ())
            dialog.OnClose (fun _ ->
                state.Trigger (Result<_>.Success ())
            )
            dialog.Enable ()
            dialog.Open ()

            let reset () =
                dialog.Close ()
                state.Trigger (Result<_>.Failure [])

            Div [dialog], reset , state

    /// Constructs an Accordion formlet, displaying
    /// the given list of formlets on separate tabs.
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
                    Result<_>.Sequence rs

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

            
            let state = HotStream<_>.New()
            
            let update index =
                let (_, s, _, _) = fs.[index]
                state.Trigger s

            let accordion =
                fs
                |> Array.map (fun (label, _, _, elem) -> label , elem)
                |> List.ofArray
                |> JQueryUI.Accordion.New
                |>! OnAfterRender (fun acc ->
                    update 0
                    acc.Activate 0
                )
            let reset () =
                fs |> Array.iter (fun (_,_,n,_) -> n null)
                accordion.Activate 0
                update 0

            // Update on tab select
            // OnSelectA accordion update
            accordion.OnActivate (fun ev _->
                accordion.Body
                |> ActiveAccordionIndex
                |> update
            )

            // State
            let state =
                Reactive.Switch state

            accordion, reset , state


    [<JavaScript>]
    let Autocomplete def (source: seq<string>) : Formlet<string> =
        MkFormlet <| fun () ->
            let state = HotStream<_>.New(Success def)
            let input = Input [Attr.Value def]
            let upd () =
                Success input.Value
                |> state.Trigger

            // Update on key up events
            input
            |> OnKeyUp (fun el _ ->
                upd ()
            )
            // Update on change events
            input
            |> OnChange (fun _ ->
                upd ()
            )
            let ac =
                JQueryUI.Autocomplete.New(
                    input ,
                    JQueryUI.AutocompleteConfiguration(
                        Source = JQueryUI.AutocompleteSource.Listing (Array.ofSeq source)
                    )
                )

            ac.OnChange(fun ev el ->
                upd ()
            )
            let reset () =
                input.Value <- def
                state.Trigger (Success def)

            ac, reset, state

    [<JavaScript>]
    let private DatepickerInput showCalendar (def: option<Date>) : Formlet<Date> =
        MkFormlet <| fun () ->
            let inp =
                if showCalendar then
                    None
                else
                    Some <| Input []
            let date =
                match inp with
                | None ->
                    JQueryUI.Datepicker.New()
                | Some inp ->
                    JQueryUI.Datepicker.New inp


            let state =
                match def with
                | Some date -> HotStream<_>.New(Success date)
                | None      -> HotStream<_>.New()

            date.OnSelect (fun date _ ->
                state.Trigger (Success date)
            )

            let reset () =
                match def with
                | Some d ->
                    date.SetDate(d)
                    state.Trigger (Success d)
                | None ->
                    match inp with
                    | Some inp  -> inp.Value <- ""
                    | None      -> ()
                    state.Trigger (Failure [])

            date
            |> OnAfterRender (fun _ ->
                reset ()
            )

            date, reset, state

    [<JavaScript>]
    let Datepicker (def: option<Date>) : Formlet<Date> =
        DatepickerInput true def

    [<JavaScript>]
    let InputDatepicker (def: option<Date>) : Formlet<Date> =
        DatepickerInput false def

    type Orientation =
        | [<Constant "vertical">] Vertical
        | [<Constant "horizontal">] Horizontal

    type RangeConfig =
        | Bool of bool
        | Min
        | Max

    type SliderConfiguration =
        {
            Animate : bool
            Orientation : Orientation
            Values : int []
            Min : int
            Max : int
            Width : option<int>
            Height : option<int>
            Range : RangeConfig
        }

        with
            [<JavaScript>]
            static member Default =
                {
                    Animate = false
                    Orientation = Horizontal
                    Values = [|0|]
                    Min = 0
                    Max = 100
                    Width = None
                    Height = None
                    Range = Bool false
                }

    [<JavaScript>]
    let Slider (conf: option<SliderConfiguration>) : Formlet<list<int>> =
        MkFormlet <| fun () ->
            let conf =
                match conf with
                | Some c    -> c
                | None      -> SliderConfiguration.Default
            let jqConf =
                JQueryUI.SliderConfiguration (
                    Animate = conf.Animate,
                    Values = conf.Values,
                    Min = conf.Min,
                    Max = conf.Max
                )
            match conf.Range with
            | RangeConfig.Bool b ->
                jqConf.Range <- b
            | RangeConfig.Max ->
                jqConf.Range <- "max"
            | RangeConfig.Min ->
                jqConf.Range <- "min"

            match conf.Orientation with
            | Orientation.Horizontal ->
                jqConf.Orientation <- "horizontal"
            | Orientation.Vertical ->
                jqConf.Orientation <- "vertical"

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

            let slider = JQueryUI.Slider.New(unbox (box jqConf))
            let state = HotStream<_>.New()
            slider.OnChange(fun _ ->
                slider.Values
                |> List.ofArray
                |> Success
                |> state.Trigger
            )
            let reset () =
                slider.Values <- conf.Values
                conf.Values
                |> List.ofArray
                |> Success
                |> state.Trigger

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
            let stateEv = HotStream<list<string>>.New()
            let state =
                Reactive.Select stateEv (fun ids ->
                    let x =
                        ids
                        |> List.map (fun id -> dict.[id])
                        |> Reactive.Sequence
                    Reactive.Select x List.ofSeq
                )
                |> Reactive.Switch

            let state = Reactive.Select state Result<_>.Sequence

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
                    let icon = Tags.Span [Attr.Class "ui-icon ui-icon-arrowthick-2-n-s"]
                    let tbl = Table [TBody [TR [TD [icon] ; TD [ elem]]]]
                    LI [Attr.Id id] -< [tbl]


                )
                |> List.iter (fun el -> list.Append el)
                update ()

            let list = list |>! OnAfterRender (fun _ -> reset ())
            list, reset, state

    /// Constructs an Tabs formlet, displaying the given list of formlets on separate tabs.
    [<JavaScript>]
    let TabsList defIndex (fs: list<string * Formlet<'T>>) : Formlet<list<'T>> =
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
                    Result<_>.Sequence rs

                )
            let reset (tabs: JQueryUI.Tabs) =
                tabs.Option("active", defIndex)
                fs
                |> List.iter (fun (_,f,_) ->
                    f.Notify null
                )

            let tabs =
                fs
                |> List.map (fun (label, _, elem) -> label , elem)
                |> JQueryUI.Tabs.New
                |>! OnAfterRender (fun tabs->
                    reset tabs
                )

            tabs, (fun _ -> reset tabs) , state

    /// Creates a formlet for collecting a list of values from a tab 
    /// interface. The first argument specifies the index of the selected tab.
    /// The second argument is a list initial tab label and formlet pairs.
    /// The third argument is formlet producing a pair of label and a new formlet.
    /// Each time the formlet triggers a new value, a new tab is added.
    /// Whenever any of the internal tabs formlets triggers a value 'None', the
    /// corresponding tab is removed.
    [<JavaScript>]
    let TabsMany    (defIndex : int)
                    (fs: list<string * Formlet<option<'T>>>)
                    (add: Formlet<string * Formlet<option<'T>>>)
                    : Formlet<list<'T>> =

         BaseFormlet.New <| fun () ->

            // Reference to a list of all current forms.
            // Updated when a new tab is added or removed.
            let initLabelFormElems =
                fs
                |> List.map (fun (l,f) ->
                    let (form, elem) = FormAndElement f
                    let state = Reactive.Heat form.State
                    l, state, elem, form
                )
            let currLabelFormElems = ref initLabelFormElems

            // Event of list of the latest states.
            let states = new Event<IObservable<seq<Result<Option<'T>>>>>()

            let updateCurrState () =
                currLabelFormElems.Value
                |> List.mapi (fun i (_, state, _, _) ->
                    state
                )
                |> RX.Sequence
                |> states.Trigger

            let tabs =
                JQueryUI.Tabs.New(
                    currLabelFormElems.Value
                    |> List.map (fun (label, _, elem, _) -> label , elem),
                    JQueryUI.TabsConfiguration(Active = defIndex))

            let state =
                RX.Switch states.Publish
                |> RMap (fun xs ->
                    let res =
                        xs
                        |> Seq.mapi (fun i x -> (i,x))
                        |> Seq.choose (fun (ix, x) ->
                            match x with
                            | Result.Success None ->
                                // Remove tab
                                tabs.Remove(ix)

                                // Update current states
                                currLabelFormElems :=
                                    currLabelFormElems.Value
                                    |> List.mapi (fun ix x -> (ix, x))
                                    |> List.filter (fun (cIx, _) ->
                                        cIx <> ix
                                    )
                                    |> List.map snd
                                None
                            | _ ->
                                Some x
                        )
                        |> Result<_>.Sequence
                    res
                    |> Result<_>.Map (List.choose id)
                )

            let tabsBody =
                {
                    Label = None
                    Element = 
                        Div [tabs]
                        |>! OnAfterRender (fun _ ->
                            currLabelFormElems.Value
                            |> List.map (fun (label, state, elem, _) ->
                                state
                            )
                            |> RX.Sequence
                            |> states.Trigger
                        )
                }

            let addForm =
                Formlet.BuildForm add

            // Add a new tab whenever the 'add form' triggers.
            addForm.State.Subscribe (fun res ->
                match res with
                | Success (label , formlet) ->
                    let (form, element) = FormAndElement formlet
                    let state = Reactive.Heat form.State
                    tabs.Add (element, label)

                    // Update state
                    currLabelFormElems := 
                        currLabelFormElems.Value @ [label, state, element, form]
                    updateCurrState ()
                | Failure _ ->
                    ()
            )
            |> ignore

            // Merge body streams of formlet and add button.
            let body = 
                let left =
                    addForm.Body
                    |> RMap Tree.Edit.Left

                let right =
                    Tree.Leaf tabsBody
                    |> Tree.Edit.Replace
                    |> Tree.Edit.Right
                    |> RX.Return

                // RX.Merge left right
                RX.Merge left right
            let reset (_: obj) =
                let numInits = initLabelFormElems |> List.length

                // Remove added tabs
                [numInits .. (tabs.Length - 1)]
                |> List.rev
                |> List.iter  (fun ix ->
                    tabs.Remove ix
                )
                
                tabs.Option("active", defIndex)

                // Reset composed state
                currLabelFormElems := initLabelFormElems

                // Reset add formlet
                addForm.Notify (obj ())

                // Reset initial form states
                for (_, _, _, form) in initLabelFormElems do
                    form.Notify (obj ())

                updateCurrState ()
            {
                Body = body
                Dispose = fun () -> ()
                State   = state
                Notify = reset
            }
        |> Data.OfIFormlet



    [<Inline "jQuery($tabs.element.el).tabs({select: function(x,ui){$f(ui.index)}})">]
    let private onSelect tabs (f : int -> unit) = ()

    [<JavaScript>]
    let private OnSelect tabs f =
        tabs
        |> OnAfterRender (fun _ -> onSelect tabs f)


    [<JavaScript>]
    let TabsChoose defIndex (fs: list<string * Formlet<'T>>) : Formlet<'T> =
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

            let states = HotStream<_>.New()

            let state = Reactive.Switch states

            let update index =
                let (_, s, _, _) = fs.[index]
                states.Trigger s

            let reset (tabs: JQueryUI.Tabs) =
                fs |> Array.iter (fun (_,_,n,_) -> n null)
                tabs.Option("active", defIndex)
                update defIndex

            let tabs =
                fs
                |> Array.map (fun (label, _, _, elem) -> label , elem)
                |> List.ofArray
                |> JQueryUI.Tabs.New
                |>! OnAfterRender (fun tabs ->
                    reset tabs
                )
            OnSelect tabs update

            tabs.OnActivate (fun ev ui ->
                update (ui.NewTab.Index())
            )

            tabs, (fun _ -> reset tabs) , state


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
            let state = HotStream<_>.New()

            let dict = System.Collections.Generic.Dictionary<string, string * 'T>()

            let dragElem id label =
                Tags.Span (GetStyle dc.DraggableStyle) -<
                [Attr.Class dc.DraggableClass] -<
                [Attr.Id id] -< [Text label]

            let dropElem id label =
                Tags.Span (GetStyle dc.DroppableStyle) -<
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
