namespace IntelliFactory.WebSharper.Formlet.JQueryUI.XTest

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Web
open IntelliFactory.WebSharper

open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.Formlet.JQueryUI

module Tests =

    [<JavaScript>]
    let Inspect (formlet: Formlet<'T>) =
        Formlet.Do {
            let! res = 
                formlet
                |> Formlet.Map (fun x -> 
                    string (box x)
                )
                |> Formlet.LiftResult
            return!
                Formlet.OfElement (fun _ ->
                    match res with
                    | Result.Success v ->
                        Div [Attr.Style "margin-top: 10px;padding:10px; border: 1px dotted #cccccc;background-color : #DFF8BD;"] -< [
                            Text v
                        ]
                    | Result.Failure fs ->
                        Div [Attr.Style "margin-top: 10px;padding:10px; border: 1px dotted #cccccc;background-color : #f9e3e3;"] -< [
                            Text "Fail"
                        ]
                )
        }
        |> Formlet.MapElement (fun el ->
            
            Div [Attr.Style "border: 1px solid gray; margin: 10px; padding: 10px;"] -< [
                el
            ]
        )


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
        Controls.Button "Click Me"

    [<JavaScript>]
    let TestDialog =
        Controls.Dialog (fun _ -> Div [Text "Viva Espana!"])

    [<JavaScript>]
    let TestAutocomplete =
        ["Adam";  "Anton"; "Diego"; "Joel"; "Vlad"]
        |> Controls.Autocomplete ""

    [<JavaScript>]
    let TestDatePicker  =
        // Controls.Datepicker None
        Controls.InputDatepicker None

    [<JavaScript>]
    let TestTabsChoose =
        [
            "Tab 1", Controls.Input ""
            "Tab 2", Controls.TextArea ""
            "Tab 3", Controls.TextArea ""
        ]
        |> Controls.TabsChoose 2

    [<JavaScript>]
    let TestSlider =
        let conf = 
            { Controls.SliderConfiguration.Default with
                Min = 10
                Max = 60
                Range = Controls.RangeConfig.Min
                Width = Some 500
                Values = [| 20 |]
                Orientation = Controls.Orientation.Horizontal
            }
        Controls.Slider (Some conf)
        |> Formlet.Map (fun xs ->
            xs |> List.map string |> List.fold (fun x y -> x + " , " + y) ""
        )
    
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
        |> Controls.TabsList 2
        |> Formlet.Map (fun xs ->
            List.fold (fun x y-> x + "," + y) "" xs
        )
        |> Enhance.WithSubmitAndResetButtons "S" "R"


    [<JavaScript>]
    let TestDragAndDrop =
        [
            "Item 1", 1, false
            "Item 2", 2, false
            "Item 3", 3, false
        ]
        |> Controls.DragAndDrop None
        |> Formlet.Map (fun xs ->
            List.fold (fun x y-> string x + "," + string y) "" xs
        )

    [<JavaScript>]
    let TestComposed =
        let name =
            TestAutocomplete
            |> Validator.IsNotEmpty "Not empty"
            |> Validator.IsEmail "Email"
            |> Enhance.WithValidationIcon
            |> Enhance.WithTextLabel "Name"
            |> Enhance.Many
        
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

        let rec more () =
            Formlet.Do {
                let! _ = 
                    Formlet.Yield (fun _ _ -> ())
                    <*> Controls.Input ""
                    <*> Controls.Button "Next"
                    |> Formlet.Horizontal
                let! _ =
                    Formlet.OfElement <| fun _ ->
                        Div [more ()]
                return ()
            }
        (
            Formlet.Yield (fun n e _ d _ dd -> 
                ", " + 
                (string e) +
                ", " + 
                (string d) + 
                ", " + 
                (string dd)
            )
            <*> (Enhance.WithLegend "Inside Legend" name)
            <*> date
            <*> mood
            <*> TestSortable
            <*> (more ())
            <*> labels
        )
        |> Enhance.WithSubmitButton "Submit"

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
    let TestSubmitAndResetButtons =
        
        let valid = 
            Controls.Input "Valid"

        let invalid =
            Controls.Input "Must be int" 
            |> Validator.IsInt "" 
            |> Enhance.WithValidationIcon

        let delayed =
            Controls.Button "Click"

        [

            valid
            |> Enhance.WithSubmitButton "Submit"
            |> Inspect

            valid
            |> Enhance.WithSubmitAndResetButtons "Submit" "Reset"
            |> Inspect

            valid
            |> Enhance.WithResetButton "Reset"
            |> Inspect

            invalid
            |> Enhance.WithSubmitButton "Submit"
            |> Inspect

            invalid
            |> Enhance.WithSubmitAndResetButtons "Submit" "Reset"
            |> Inspect

            invalid
            |> Enhance.WithResetButton "Reset"
            |> Inspect

            delayed
            |> Enhance.WithSubmitButton "Submit"
            |> Inspect

            delayed
            |> Enhance.WithSubmitAndResetButtons "Submit" "Reset"
            |> Inspect

            delayed
            |> Enhance.WithResetButton "Reset"
            |> Inspect

            // Bugzilla 490
            [
                ("tab1", Controls.Input "" |> Validator.IsNotEmpty "")
                ("tab2", Controls.Input "" |> Validator.IsNotEmpty "")
            ]
            |> Controls.TabsChoose 0
            |> Inspect

        ]
        |> Formlet.Sequence
        |> Formlet.Map ignore

    [<JavaScript>]
    let TestWithDialog =
        [
            Controls.Input "X"
            |> Enhance.WithDialog "Title"
            |> Inspect

            Formlet.Do {
                let! x = Controls.Button "Show"
                return!
                    Controls.Input ""
                    |> Enhance.WithSubmitAndResetButtons "S" "R"
                    |> Enhance.WithDialog "Title"

            }
            |> Enhance.WithSubmitAndResetButtons "S" "R"
            |> Inspect
        ]
        |> Formlet.Sequence
        |> Formlet.Map ignore

    [<JavaScript>]
    let TestTabsMany =
        
        let newF =
            Formlet.Choose [
                
                Controls.Input "" 
                |> Formlet.Map Some 
                
                Controls.Button "Remove" 
                |> Formlet.Map (fun  _ -> None) 
            ]
        let fs =
            [
                "0", Controls.Input "0" |> Formlet.Map Some
                "1", Controls.Input "1" |> Formlet.Map Some
                "2", Controls.Input "2" |> Formlet.Map Some
            ]

        let addF =
            Controls.Button "Add"
            |> Formlet.Map (fun ix ->
                string (ix + (List.length fs)) , newF
            )

        Controls.TabsMany 1 fs addF
        |> Enhance.WithSubmitAndResetButtons "Submit" "Reset"
        |> Formlet.Map (List.fold (fun x y -> x + " " + y) "")
        |> Inspect

    [<JavaScript>]
    let AllTests () = 
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
            "Submit and Reset" , TestSubmitAndResetButtons
            "Test WithDialog" , TestWithDialog
            "Test TabsMany", TestTabsMany
        ]
        |> Controls.TabsList 0
        |> Formlet.Map ignore
        |> Enhance.WithFormContainer

