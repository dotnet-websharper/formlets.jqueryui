namespace IntelliFactory.WebSharper.Formlet.JQueryUI.XTest

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Web
open IntelliFactory.WebSharper

open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.Formlet.JQueryUI

module F =

    [<JavaScript>]
    let WithErrorSummary formlet =
        Formlet.Do {
            let! res = 
                Formlet.LiftResult formlet
            let! _ = Controls.Button "Submit"
            return! 
                match res with
                | Result.Success s -> 
                    Formlet.Return s
                | Result.Failure fs ->
                    Formlet.Do {
                        let! _ =
                            Formlet.OfElement <| fun _ ->
                                fs 
                                |> List.map (fun f -> LI [Text f])
                                |> UL
                        return! Formlet.Never ()
                    }
        }

    [<JavaScript>]
    let WithValidation  (p: 'T -> bool) (msg: 'T -> string) (formlet: Formlet<'T>) =
        formlet
        |> Formlet.MapResult (fun res ->
            match res with
            | Result.Success s ->
                if p s then
                    Result.Success s
                else
                    Result.Failure [msg s]
            | Result.Failure fs ->
                Result.Failure fs
        )
    [<JavaScript>]
    let TestWithValidation =
        Controls.Input ""
        |> WithValidation (fun x -> x = "hello") (fun x -> x + " not valid")
        |> Enhance.WithValidationIcon
        |> WithErrorSummary

type SampleControl() =
    inherit Web.Control()

    [<JavaScript>]
    override this.Body =
        Tests.Foo ()
        // Sample.Main
        |> Enhance.WithFormContainer
        :> _