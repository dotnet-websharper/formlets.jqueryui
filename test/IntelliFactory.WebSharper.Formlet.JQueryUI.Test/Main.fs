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

        let form =
            let slider =
                JQueryUI.Slider.New(
                    JQueryUI.SliderConfiguration(
                        Values = [|10;  30 ; 50|]
                    )
                )

            let button = 
                JQueryUI.Button.New("Click")
            button.OnClick (fun _ ->
                slider.Values
                |> Seq.iter (fun x ->
                    JavaScript.Alert (unbox box x)
                )
            )
            Div [
                button :> IPagelet
                slider :> _ 
            ]
        // form
        Tests.AllTests ()
        :> _