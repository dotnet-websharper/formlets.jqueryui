namespace IntelliFactory.WebSharper.Formlet.JQueryUI.XTest

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Web
open IntelliFactory.WebSharper

open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.Formlet.JQueryUI
open IntelliFactory.WebSharper.JQueryUI

module F =
    [<Inline "console.log($x)">]
    let Log x = ()

module J =

    [<JavaScript>]
    let InDialog (title: string) (formlet: Formlet<'T>) =
        Formlet.BuildFormlet <| fun _ ->
            let state = new Event<_>()
            let conf =
                JQueryUI.DialogConfiguration (
                    modal = true,
                    dialogClass = "dialog",
                    title = title
                )
            let dialogOpt : ref<option<JQueryUI.Dialog>> = 
                ref None
            let el =
                Div [
                    formlet
                    |> Formlet.Run (fun confirmed ->
                        match dialogOpt.Value with
                        | Some dialog ->
                            state.Trigger (Result.Success confirmed)
                            dialog.Close()
                        | None ->
                            ()
                    )
                ]
            let dialog = 
                JQueryUI.Dialog.New(el, conf)
            dialogOpt := Some dialog
            Div [dialog] , ignore , state.Publish

    [<JavaScript>]
    let Main () =
        Formlet.Do {
            let! _ =  Controls.Button "Show"
            let! name = 
                Controls.Input ""
                |> Enhance.WithSubmitAndResetButtons "Submit" "Reset"
                |> InDialog "Enter your name" 
            return ()
        }
        |> Enhance.WithFormContainer

    [<JavaScript>]
    let RX = Formlet.Data.UtilsProvider().Reactive

    [<JavaScript>]
    let RMap f s= RX.Select s f

[<Sealed>]
type SampleControl () =
    inherit Web.Control()

    [<JavaScript>]
    override this.Body =
        Tests.AllTests ()
        :> _
