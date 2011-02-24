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
    let WithSubmitButton (label: string) (formlet: Formlet<'T>) : Formlet<'T> =
        Enhance.WithSubmitFormlet formlet (fun res ->
            match res with
            | Result.Success _ -> 
                JQueryUI.Controls.Button label
            | Result.Failure _ ->
                let conf = JQueryUI.ButtonConfiguration(Label = label)
                conf.Disabled <- true
                JQueryUI.Controls.CustomButton conf
            |> Formlet.Map ignore
        )

    [<JavaScript>]
    let WithResetButton (label: string) (formlet: Formlet<'T>) : Formlet<'T> =
        Enhance.WithResetFormlet formlet (JQueryUI.Controls.Button label)

    [<JavaScript>]
    let WithSubmitAndResetButtons (submitLabel: string) (resetLabel: string) (formlet: Formlet<'T>) : Formlet<'T> =
        let submitReset (reset : obj -> unit) (result : Result<'T>) : Formlet<'T> =
            
            let submit : Formlet<'T> =
                match result with
                | Success (value: 'T) -> 
                    JQueryUI.Controls.Button submitLabel
                    |> Formlet.Map (fun _ -> value)
                | Failure fs -> 
                    let conf = JQueryUI.ButtonConfiguration(Label = submitLabel)
                    conf.Disabled <- true
                    JQueryUI.Controls.CustomButton conf
                    |> Formlet.MapResult (fun _ -> Failure fs)
            
            let reset =
                Formlet.Do {
                    let! res = Formlet.LiftResult (JQueryUI.Controls.Button resetLabel)
                    do
                        match res with
                        | Success _  ->
                            reset null
                        | _          -> ()
                    return ()
                }
            (
                Formlet.Return (fun v _ -> v)
                <*> submit
                <*> reset
            )            
            |> Formlet.Horizontal
        
        Enhance.WithSubmitAndReset formlet submitReset

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