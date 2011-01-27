namespace IntelliFactory.WebSharper.Formlet.JQueryUI

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Html.Events
open IntelliFactory.Formlet.Base
open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.JQuery
open Utils

module Enhance =
    [<JavaScript>]
    let WithSubmitButton (label: string) (formlet: Formlet<'T>) : Formlet<'T> =
        Enhance.WithSubmitFormlet formlet (fun res ->
            match res with
            | Result.Success _ -> 
                Log "Success"
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
    
