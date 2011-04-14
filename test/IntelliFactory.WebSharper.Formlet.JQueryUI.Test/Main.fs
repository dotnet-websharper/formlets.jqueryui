namespace IntelliFactory.WebSharper.Formlet.JQueryUI.XTest

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Web
open IntelliFactory.WebSharper

open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.Formlet.JQueryUI

type SampleControl() =
    inherit Web.Control()

    [<JavaScript>]
    override this.Body =
        let form =
            Controls.Input ""
            |> Validator.IsNotEmpty "Not empty"
            |> Enhance.WithValidationIcon
            |> Enhance.WithSubmitAndResetButtons "Update" "Reset"
            |> Enhance.WithFormContainer

        form
        :> _