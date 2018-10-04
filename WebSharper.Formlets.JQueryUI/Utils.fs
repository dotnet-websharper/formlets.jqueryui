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
namespace WebSharper.Formlets.JQueryUI

open WebSharper
open WebSharper.JavaScript
open WebSharper.Html.Client
open WebSharper.Formlets
open IntelliFactory.Formlets.Base
open System


module CssConstants =

    [<JavaScript>]
    let ValidIconClass = "validIcon ui-icon ui-icon-check"

    [<JavaScript>]
    let ErrorIconClass = "errorIcon icon ui-icon ui-icon-alert"
    
    [<JavaScript>]
    let AddIconClass = "addIcon ui-icon ui-icon-circle-plus"

    [<JavaScript>]
    let RemIconClass = "removeIcon ui-icon ui-icon-circle-minus"

    [<JavaScript>]
    let LegendClass = "ui-widget-content ui-corner-all"

    [<JavaScript>]
    let FormContainerClass = "formlet-jqueryui"

    [<JavaScript>]
    let DropContainerClass = "dropContainer"

    [<JavaScript>]
    let DragContainerClass = "dragContainer"

    [<JavaScript>]
    let DraggableClass = "draggableItem"

    [<JavaScript>]
    let DroppableClass = "droppableItem"


module internal Utils =

    [<JavaScript>]
    let BaseFormlet = FormletProvider(UtilsProvider ())

    [<JavaScript>]
    let RX = Formlets.Data.UtilsProvider().Reactive

    [<JavaScript>]
    let RMap f s= RX.Select s f

    [<JavaScript>]
    let RChoose f s=  RX.Choose s f

    [<JavaScript>]
    let MkFormlet f =
        Formlet.BuildFormlet <| fun () ->
            let b, r, s = f ()
            let panel =
                Div [b]
            panel, r, s

    [<JavaScript>]
    let FormAndElement formlet =
        let formlet = Formlet.WithLayoutOrDefault formlet
        let form = Formlet.BuildForm formlet
        match (formlet :> IFormlet<_,_>).Layout.Apply(form.Body) with
        | Some (body, _) -> form, body.Element
        | None           -> form, Div []



