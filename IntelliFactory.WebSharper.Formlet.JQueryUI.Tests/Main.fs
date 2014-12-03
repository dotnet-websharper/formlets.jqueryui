﻿namespace IntelliFactory.WebSharper.Formlet.JQueryUI.XTest

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Web
open IntelliFactory.WebSharper

open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.Formlet.JQueryUI
open IntelliFactory.WebSharper.JQueryUI
open IntelliFactory.Formlet.Base
open IntelliFactory.Reactive

open System

module Utils =
    [<Inline "console.log($x)">]
    let Log x = ()

module F =

    [<JavaScript>]
    let BaseFormlet = FormletProvider(UtilsProvider ())

    [<JavaScript>]
    let RX = Formlet.Data.UtilsProvider().Reactive

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


open IntelliFactory.WebSharper.Sitelets

type Act = | Index

module Site =

    open IntelliFactory.Html

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