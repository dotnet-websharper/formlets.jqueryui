namespace IntelliFactory.WebSharper.Formlet.JQueryUI

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Html.Events
open IntelliFactory.Formlet.Base
open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.JQuery
open Utils

module Enhance =
    
    module CC = IntelliFactory.WebSharper.Formlet.JQueryUI.CssConstants

    [<JavaScript>]
    let WithValidationIcon formlet =
        formlet
        |> Enhance.WithCustomValidationIcon 
            {
                ValidIconClass = CC.ValidIconClass
                ErrorIconClass = CC.ErrorIconClass
            }

    
    [<JavaScript>]
    let RX = Formlet.Data.UtilsProvider.Reactive

    [<JavaScript>]
    let RMap f s= RX.Select s f

    [<JavaScript>]
    let RChoose f s=  RX.Choose s f

    [<JavaScript>]
    let BaseFormlet = FormletProvider(UtilsProvider)

    open IntelliFactory.Formlet.Base.Tree

    [<JavaScript>]
    let private MakeWithSubmitAndResetButtons 
                            (submitLabel: option<string>) 
                            (resetLabel: option<string>) 
                            (formlet: Formlet<'T>) : Formlet<'T> =
        BaseFormlet.New <| fun _ ->
            
            // Submit button
            let submitButton = 
                submitLabel
                |> Option.map (fun label ->
                    JQueryUI.Button.New (label)
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
                        [submitButton; resetButton]
                        |> List.choose id
                        |> Div
                }

            // Build the form
            let form = Formlet.BuildForm formlet

            // Enable/Disable button depending on state
            submitButton
            |> Option.iter (fun button ->
                button
                |> OnAfterRender (fun _ ->
                    button.Disable ()
                    form.State.Subscribe (fun res ->
                        match res with
                        | Result.Success _ ->
                            button.Enable ()
                        | Result.Failure fs ->
                            button.Disable ()
                    )
                    |> ignore
                )
            )

            // State of the formlet
            let state =
                match submitButton with
                | Some button ->
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