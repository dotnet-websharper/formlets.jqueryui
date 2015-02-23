namespace WebSharper.Formlets.JQueryUI.XTest

open WebSharper
open WebSharper.Html.Client
open WebSharper.Web
open WebSharper.JavaScript

open WebSharper.Formlets
open WebSharper.Formlets.JQueryUI
open WebSharper.JQueryUI
open IntelliFactory.Formlets.Base
open IntelliFactory.Reactive

open System

module Utils =
    [<Inline "console.log($x)">]
    let Log x = ()

module F =

    [<JavaScript>]
    let BaseFormlet = FormletProvider(UtilsProvider ())

    [<JavaScript>]
    let RX = Formlets.Data.UtilsProvider().Reactive

    [<JavaScript>]
    let RMap f s= RX.Select s f

    [<JavaScript>]
    let FormAndElement formlet =
        let formlet = Formlet.WithLayoutOrDefault formlet
        let form = Formlet.BuildForm formlet
        match (formlet :> IFormlet<_,_>).Layout.Apply(form.Body) with
        | Some (body, _) -> form, body.Element
        | None           -> form, Div []

[<Sealed>]
type SampleControl () =
    inherit Web.Control()

    [<JavaScript>]
    override this.Body =
        Tests.AllTests ()
        :> _


open WebSharper.Sitelets

type Act = | Index

module Site =

    open WebSharper.Html.Server

    let HomePage =
        Sitelets.Content.PageContent <| fun ctx ->
            { Page.Default with
                Title = Some "WebSharper Formlets for jQuery UI Tests"
                Body = [Div [new SampleControl()]] }

    let Main = Sitelet.Content "/" Index HomePage

[<Sealed>]
type Website() =
    interface IWebsite<Act> with
        member this.Sitelet = Site.Main
        member this.Actions = [Act.Index]

[<assembly: Website(typeof<Website>)>]
do ()
