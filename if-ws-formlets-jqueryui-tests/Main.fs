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


    /// Given a sequenec of formlets, returns a formlet whose result
    /// is the last triggered formlet value of any of the
    /// formlets of the sequence.
    [<JavaScript>]
    let Choose (fs : seq<Formlet<'T>>) =
        let count = ref 0
        fs
        |> Seq.map (fun f ->
            f
            |> Formlet.Map (fun x ->
                incr count
                (x,count.Value)
            )
            |> Formlet.InitWithFailure
            |> Formlet.LiftResult
        )
        |> Formlet.Sequence
        |> Formlet.Map (fun xs ->
            xs
            |> List.choose (fun x ->
                match x with
                | Result.Success v  -> Some v
                | _                 -> None
            )
            |> List.sortBy (fun (_,ix) -> ix)
            |> List.rev
            |> List.tryPick (fun (x,_) -> Some x)
        )
        |> Validator.Is (fun x -> Option.isSome x) ""
        |> Formlet.Map (fun x -> x.Value)
    
    [<JavaScript>]
    let YesNo () =
        [
            Controls.Button "Yes" |> Formlet.Map (fun _ -> true)
            Controls.Button "No" |> Formlet.Map (fun _ -> false)
        ]
        |> Choose
        |> Formlet.Horizontal

    let confirmationForm order =
        let title = DialogConfiguration(title = "Are you sure you want to place the order?")
        let form = 
            Formlet.Return ()
            |> Enhance.WithCustomSubmitAndResetButtons
                { Enhance.FormButtonConfiguration.Default with Label = Some "Yes" }
                { Enhance.FormButtonConfiguration.Default with Label = Some "No" }
            |> Enhance.WithFormContainer
        let rec dialog = Dialog.New(Div [ result ], title)
        and result =
            Formlet.Do {
                let! _ = form |> Enhance.WithResetAction (fun _ -> dialog.Close(); true)
                dialog.Close()
                return true
            } 
            |> Enhance.WithFormContainer
        (dialog :> IPagelet).Render()

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
