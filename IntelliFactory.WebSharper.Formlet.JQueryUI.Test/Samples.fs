namespace IntelliFactory.WebSharper.Formlet.JQueryUI.XTest

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Web
open IntelliFactory.WebSharper

open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.Formlet.JQueryUI

module Samples =

    [<JavaScript>]
    let AddLabels () =
        Formlet.Do {
            // Formlet for inputting a list of labels
            let! labels = 
                Controls.Input "" 
                |> Validator.IsNotEmpty "Empty value not allowed"
                |> Enhance.WithValidationIcon
                |> Enhance.Many
                |> Enhance.WithLegend "Add Label"
            // Formlet for sorting the list
            let! orderedLabels =
                labels
                |> List.map (fun label ->
                    Formlet.OfElement (fun _ -> Label [Text label])
                    |> Formlet.Map (fun _ -> label)
                )
                |> Controls.Sortable
                |> Enhance.WithLegend "Order of Precedence"
                |> Enhance.WithSubmitAndResetButtons "Test" "Reset"
            // Formlet for showing a text box with auto complete
            return! 
                Controls.Autocomplete "" orderedLabels
                |> Enhance.WithTextLabel "Label"
                |> Enhance.WithLegend "Show suggestions"
        }

