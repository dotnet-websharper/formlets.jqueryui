namespace IntelliFactory.WebSharper.Formlet.JQueryUI.XTest

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Web
open IntelliFactory.WebSharper

open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.Formlet.JQueryUI

module Tests =

    [<JavaScript>]
    let Inspect (formlet: Formlet<'T>)  =
        Formlet.Do {
            let! value = 
                formlet
                |> Formlet.Map (fun x -> string (box x))
                |> Enhance.WithSubmitAndResetButtons "Submit" "Reset"
            return!
                Formlet.OfElement (fun _ ->
                    Div [Attr.Style "margin-top: 10px;padding:10px; border: 1px dotted #cccccc;"] -< [
                        Text value
                    ]
                )
        }
        


    // TESTED
    [<JavaScript>]
    let TestAccordionChoose =
        let f =
            Controls.Button "Button"
            |> Formlet.Map string
        [
            "Number 2", Controls.TextArea ""
            "Number 1", f
            "Number 3", Controls.Input ""
        ]
        |> List.map (fun (l, f) ->
            let f  = 
                f 
                |> Formlet.MapElement (fun el ->
                    Div [Attr.Style "width: 400px"] -< [el]
                )
            
            l,f
        )
        |> Controls.AccordionChoose

    // TESTED
    [<JavaScript>]
    let TestAccordionList =
        [
            "Number 2", Controls.TextArea ""
            "Number 1", ["Item 1", "0"; "Item 2", "1"] |> Controls.Select 0 
            "Number 3", Controls.Input ""
        ]
        |> Controls.AccordionList
        |> Formlet.Map (fun xs ->
            List.fold (fun x y-> x + "," + y) "" xs
        )

    [<JavaScript>]
    let TestButton =
        Formlet.Do {
            let! _ = Controls.Button "Click Me"
            let! _ = Controls.Dialog (fun _ -> Div [Text "Viva Espana!"])
            return "Done"
        }

    [<JavaScript>]
    let TestAutocomplete =
        ["Adam";  "Anton"; "Diego"; "Joel"; "Vlad"]
        |> Controls.Autocomplete ""

    [<JavaScript>]
    let TestDatePicker  =
        let date = new EcmaScript.Date()
        Controls.Datepicker (Some date)

    [<JavaScript>]
    let TestSlider =
        let conf = 
            { Controls.SliderConfiguration.Default with
                Min = 20
                Max = 30
                Width = Some 500
                Orientation = Controls.Orientation.Vertical
            }
        Controls.Slider (Some conf)
    
    [<JavaScript>]
    let TestSortable =
        [
            Controls.Input "A"
            Controls.Input "B"
            Controls.Input "C"
        ]
        |> Controls.Sortable
        |> Formlet.Map (fun xs ->
            List.fold (fun x y-> x + "," + y) "" xs
        )

    [<JavaScript>]
    let TestTabsList =
        [
            "Tab 1", Controls.Input ""
            "Tab 2", Controls.TextArea ""
            "Tab 3", Controls.TextArea ""
        ]
        |> Controls.TabsList
        |> Formlet.Map (fun xs ->
            List.fold (fun x y-> x + "," + y) "" xs
        )

    [<JavaScript>]
    let TestTabsChoose =
        [
            "Tab 1", Controls.Input ""
            "Tab 2", Controls.TextArea ""
            "Tab 3", Controls.TextArea ""
        ]
        |> Controls.TabsChoose

    [<JavaScript>]
    let TestDragAndDrop =
        let conf =
            {Controls.DragAndDropConfig.Default with
                DragContainerStyle = 
                    Some "padding: 20px; border: 1px solid #AAA; width: 300px"
                DropContainerStyle = 
                    Some "padding: 20px; border: 1px solid #AAA; width: 300px; margin-top : 10px"
                DraggableStyle = 
                    Some "background-color: #AAA; margin-right: 5px; display: inline-block; padding: 3px;"
                DroppableStyle = 
                    Some "background-color: #AAA; margin-right: 5px; display: inline-block; padding: 3px;"
            }
        [
            "Item 1", 1, false
            "Item 2", 2, false
            "Item 3", 3, true
        ]
        |> Controls.DragAndDrop (Some conf)
        |> Formlet.Map (fun xs ->
            List.fold (fun x y-> string x + "," + string y) "" xs
        )

    [<JavaScript>]
    let TestComposed =
        let name =
            TestAutocomplete
            |> Validator.IsNotEmpty "Not empty"
            |> Enhance.WithValidationIcon
            |> Enhance.WithTextLabel "Name"
        let date =
            Controls.Datepicker None
            |> Enhance.WithTextLabel "Date"
        let mood =
            let conf =
                {Controls.SliderConfiguration.Default with
                    Width = Some 300
                    Min = 0
                    Max = 100
                }
            Controls.Slider (Some conf)
            |> Enhance.WithTextLabel "Mood"
        
        let rowConf =
            { Layout.FormRowConfiguration.Default with
                Padding = Some {Left = Some 10; Right = Some 10; Top = Some 10; Bottom = Some 10;}
            }

        let labels = 
            TestDragAndDrop
            |> Enhance.WithTextLabel "Labels"
        (
            Formlet.Yield (fun n e d dd -> ()
            )
            <*> name
            <*> date
            <*> mood
            <*> labels
        )
        |> Enhance.WithRowConfiguration rowConf
        |> Enhance.WithCustomSubmitButton Enhance.FormButtonConfiguration.Default
        |> Enhance.WithFormContainer

    [<JavaScript>]
    let TestComposedSimple =
        let name =
            TestAutocomplete
            |> Validator.IsNotEmpty "Not empty"
            |> Enhance.WithValidationIcon
            |> Enhance.WithTextLabel "Name"
        let date =
            Controls.Datepicker None
            |> Enhance.WithTextLabel "Date"
        (
            Formlet.Yield (fun n d -> ())
            <*> name
            <*> date        )

    [<JavaScript>]
    let TestWithButton =
        Formlet.Do {
            let! x = Controls.Button "click"
            return! Formlet.OfElement (fun _ -> Div [Text "HOHO"])
        }
        |> Enhance.WithSubmitButton "GO"

    [<JavaScript>]
    let TestNotInitButton =
        Formlet.Do {
            let! _ = Controls.Button "First"
            let! x = Controls.Slider None
            let! _ = Controls.Button "Click"
            return ()
        }

    [<JavaScript>]
    let Foo () =
        [
            "AccordionChoose",  Inspect TestAccordionChoose
            "AccordionList", Inspect TestAccordionList
            "AccordionAutocomplete", Inspect TestAutocomplete
            "DatePicker", Inspect TestDatePicker
            "Button", Inspect TestButton
            "Slider", Inspect TestSlider
            "TabsList", Inspect TestTabsList
            "TabsChoose", Inspect TestTabsChoose
            "DragAndDrop", Inspect TestDragAndDrop
            "Sortable" , Inspect TestSortable
            "Composed", Inspect TestComposed
        ]
        |> Controls.TabsList
        |> Formlet.Map ignore

