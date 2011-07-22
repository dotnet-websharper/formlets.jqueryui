namespace IntelliFactory.WebSharper.Formlet.JQueryUI.XTest

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Web
open IntelliFactory.WebSharper

open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.Formlet.JQueryUI

module F =
    [<Inline "console.log($x)">]
    let Log x = ()

module JQueryFormlet =

    open IntelliFactory.Formlet.Base
    let RX = IntelliFactory.Reactive.Reactive.Default

    [<JavaScript>]
    let RMap f s= RX.Select s f

    [<JavaScript>]
    let J () =
        let form = 
            Controls.Datepicker (Some <| EcmaScript.Date())
            |> Formlet.BuildForm

        form.State.Subscribe (fun res ->
            match res with
            | Result.Failure fs ->
                F.Log ("Failure_", fs)
            | Result.Success x ->
                F.Log ("Success_", x)
        )
        |> ignore

        Div []
        |>! OnAfterRender (fun el ->
            form.Body.Subscribe (fun edit ->
                match edit with
                | Tree.Edit.Replace t ->
                    match t with
                    | Tree.Leaf x ->
                        el.Append (x.Element)
                    | _ -> 
                        ()
                | _ ->
                    ()
            )
            |> ignore
        )



    [<JavaScript>]
    let Main () = 
        let f1 =
            Controls.InputDatepicker (Some <| EcmaScript.Date())
            |> Enhance.WithSubmitAndResetButtons "S" "R"
            |> Tests.Inspect

        let f2 =
            Controls.InputDatepicker None
            |> Enhance.WithSubmitAndResetButtons "S" "R"
            |> Tests.Inspect
        
        let f3 =
            Controls.Datepicker (Some <| EcmaScript.Date())
            |> Enhance.WithSubmitAndResetButtons "S" "R"
            |> Tests.Inspect

        let f4 =
            Controls.Datepicker None
            |> Enhance.WithSubmitAndResetButtons "S" "R"
            |> Tests.Inspect

        Div [
            f1
            f2
            f3
            f4
        ]



type SampleControl () =
    inherit Web.Control()

    [<JavaScript>]
    override this.Body = 
        Tests.AllTests () :> _
        //JQueryFormlet.Main () :> _
    


