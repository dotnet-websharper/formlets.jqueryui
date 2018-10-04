// $begin{copyright}
//
// This file is part of WebSharper
//
// Copyright (c) 2008-2018 IntelliFactory
//
// Licensed under the Apache License, Version 2.0 (the "License"); you
// may not use this file except in compliance with the License.  You may
// obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
// implied.  See the License for the specific language governing
// permissions and limitations under the License.
//
// $end{copyright}
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

    let HomePage ctx =
        Content.Page(
            Title = "WebSharper Formlets for jQuery UI Tests",
            Body = [Div [new SampleControl()]]
        )

    let Main = Sitelet.Content "/" Index HomePage

[<Sealed>]
type Website() =
    interface IWebsite<Act> with
        member this.Sitelet = Site.Main
        member this.Actions = [Act.Index]

[<assembly: Website(typeof<Website>)>]
do ()
