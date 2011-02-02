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
        formlet
        |> Formlet.Map ignore
//        Formlet.Do {
//            let! value = 
//                formlet
//                |> Formlet.Map (fun x -> string (box x))
//                |> Enhance.WithSubmitAndResetButtons "Submit" "Reset"
//            return!
//                Formlet.OfElement (fun _ ->
//                    Div [Attr.Style "margin-top: 10px;padding:10px; border: 1px dotted #cccccc;"] -< [
//                        Text value
//                    ]
//                )
//        }
        


    // TESTED
    [<JavaScript>]
    let TestAccordionChoose =
        let f =
            Controls.Button "Button"
            |> Formlet.Map string
            |>! OnAfterRender (fun f ->
                Log "after render"
            )
        [
            "Number 2", Controls.TextArea ""
            "Number 1", f
            "Number 3", Controls.Input ""
        ]
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

    [<JavaScript>]
    let TestButton =
        Formlet.Do {
            let! _ = Controls.Button "Click Me"
            let _ = JQueryUI.Dialog.New(Div [Text "Clicked"])
            return ()
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

    [<JavaScript>]
    let TestTabsList =
        [
            "Tab 1", Controls.Input ""
            "Tab 2", Controls.TextArea ""
            "Tab 3", Controls.TextArea ""
        ]
        |> Controls.TabsList

    [<JavaScript>]
    let TestTabsChoose =
        [
            "Tab 1", Controls.Input ""
            "Tab 2", Controls.TextArea ""
            "Tab 3", Controls.TextArea ""
        ]
        |> Controls.TabsList

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

