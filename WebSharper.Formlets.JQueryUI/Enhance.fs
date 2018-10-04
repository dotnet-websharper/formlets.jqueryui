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
open WebSharper.Html.Client.Events
open IntelliFactory.Formlets.Base
open WebSharper.Formlets
open WebSharper.JQuery
open IntelliFactory.Formlets.Base.Tree
open Utils

module Enhance =
    
    open IntelliFactory.Reactive
    module CC = WebSharper.Formlets.JQueryUI.CssConstants

    [<JavaScript>]
    let WithValidationIcon formlet =
        formlet
        |> Enhance.WithCustomValidationIcon 
            {
                ValidIconClass = CC.ValidIconClass
                ErrorIconClass = CC.ErrorIconClass
            }


    

    [<JavaScript>]
    let private MakeWithSubmitAndResetButtons 
                            (submitLabel: option<string>) 
                            (resetLabel: option<string>) 
                            (formlet: Formlet<'T>) : Formlet<'T> =
        BaseFormlet.New <| fun _ ->
            let submitButton = 
                submitLabel
                |> Option.map (fun label ->
                    let isRendered = ref false
                    let button =
                        JQueryUI.Button.New (
                            JQueryUI.ButtonConfiguration(
                                Disabled = true,
                                Label = label
                            )
                        )
                        |>! OnAfterRender (fun _ ->
                            isRendered := true
                        )
                    button, isRendered
                )

            // Reset button
            let resetButton = 
                resetLabel
                |> Option.map (fun label ->
                    JQueryUI.Button.New (label)
                )

            let formBody =
                {
                    Label = None
                    Element = 
                        [Option.map fst submitButton; resetButton]
                        |> List.choose id
                        |> Div
                }

            // Build the form
            let form = Formlet.BuildForm formlet

            form.State.Subscribe (fun res ->
                submitButton
                |> Option.iter (fun (button , isRendered)->
                    match res with
                    | Result.Success _ ->
                        if isRendered.Value then
                            button.Enable()
                        else
                            button
                            |> OnAfterRender (fun _ ->
                                button.Enable()
                            )
                    | Result.Failure fs ->
                        if isRendered.Value then
                            button.Disable ()
                )
            )
            |> ignore

            // State of the formlet
            let state =
                match submitButton with
                | Some (button, _) ->
                    let count = ref 0
                    let submitButtonClickState = new Event<int>()
                    button.OnClick (fun _ ->
                        incr count
                        submitButtonClickState.Trigger count.Value
                    )
                    let latestCount = ref 0
                    RX.CombineLatest 
                        form.State 
                        submitButtonClickState.Publish 
                        (fun value count -> value, count)
                    |> RChoose (fun (value, count) ->
                        if count <> latestCount.Value then
                            latestCount := count
                            Some value
                        else
                            None
                    )
                | None ->
                    form.State

            // Create the body stream
            let body = 
                let right =
                    Tree.Leaf formBody
                    |> Tree.Edit.Replace
                    |> Tree.Edit.Right
                    |> RX.Return
                let left =
                    form.Body
                    |> RMap Tree.Edit.Left
                RX.Merge left right

            // Reset function
            let reset x =
                form.Notify x

            // Trigger reset
            resetButton
            |> Option.iter (fun button ->
                button.OnClick (fun _ ->
                    reset ()
                )
            )
            {
                Body = body
                Dispose = fun () -> ()
                State   = state
                Notify = reset
            }
        |> Data.OfIFormlet

    [<JavaScript>]
    let WithSubmitButton (label: string) (formlet: Formlet<'T>) : Formlet<'T> =
        MakeWithSubmitAndResetButtons (Some label) None formlet

    [<JavaScript>]
    let WithResetButton (label: string) (formlet: Formlet<'T>) : Formlet<'T> =
        MakeWithSubmitAndResetButtons None (Some label) formlet

    [<JavaScript>]
    let WithSubmitAndResetButtons (submitLabel: string) (resetLabel: string) (formlet: Formlet<'T>) : Formlet<'T> =
        MakeWithSubmitAndResetButtons (Some submitLabel) (Some resetLabel) formlet

    /// Creates a formlet wrapped inside a legend element with the
    /// given label. This function merges the internal form body components.
    [<JavaScript>]
    let WithLegend (label: string) (formlet : Formlet<'T>) : Formlet<'T> =
        formlet
        |> Formlet.MapBody (fun body ->
            let element =
                FieldSet [Attr.Class CssConstants.LegendClass] -< [
                    Legend [Tags.Text label]
                    (
                        match body.Label with
                        | Some label ->
                            Table [
                                TBody [
                                    TR [
                                        TD [label ()]
                                        TD [body.Element]
                                    ]
                                ]
                            ]
                        | None ->
                            body.Element
                    )
                ]
            {Element = element ; Label = None}
        )
    [<JavaScript>]
    let Many formlet =
        formlet
        |> Enhance.CustomMany
            {Enhance.ManyConfiguration.Default with
                AddIconClass = CC.AddIconClass
                RemoveIconClass = CC.RemIconClass
            }

    [<Require(typeof<Resources.SkinResource>)>]
    [<JavaScript>]
    let WithFormContainer formlet =
        formlet
        |> Formlet.MapElement (fun el -> 
            Div [Attr.Class CC.FormContainerClass] -< [el]
        )

    [<JavaScript>]
    let WithDialog (title: string) (formlet: Formlet<'T>) =
        Formlet.BuildFormlet <| fun _ ->
            let state = HotStream<_>.New(Failure [])
            let conf =
                JQueryUI.DialogConfiguration (
                    Modal = true,
                    DialogClass = "dialog",
                    Title = title
                )
            let dialogOpt : ref<option<JQueryUI.Dialog>> = 
                ref None
            let el =
                Div [
                    formlet
                    |> Formlet.Run (fun confirmed ->
                        match dialogOpt.Value with
                        | Some dialog ->
                            state.Trigger (Result<_>.Success confirmed)
                            dialog.Close()
                        | None ->
                            ()
                    )
                ]
            let dialog = 
                JQueryUI.Dialog.New(el, conf)
            dialogOpt := Some dialog

            let reset () =
                dialog.Close ()
                state.Trigger (Failure [])
            Div [dialog] , reset , state
