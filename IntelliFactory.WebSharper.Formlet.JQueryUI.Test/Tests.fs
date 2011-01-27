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
            let! value = formlet
            let! _ = Controls.Input ""
            return ()
        }
        |> Enhance.WithSubmitAndResetButtons "Submit" "Reset"



    
    // TESTED
    [<JavaScript>]
    let TestAccordionChoose () =
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

