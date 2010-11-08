namespace IntelliFactory.WebSharper.Formlet.JQueryUI

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Html.Events
open IntelliFactory.Formlet.Base
open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.JQuery
open Utils

module Test =

    [<JavaScript>]    
    let Main () =        
        
        let date = Controls.Datepicker None        
        let div = 
            Button [Text "f"]
            |>! Events.OnClick (fun el me ->
                el.Text <- string me.X
            )
        div
        
        
//        let sortable =
//            [
//                "Item 1"
//                "Item 2"
//                "Item 3"
//            ]
//            |> List.map (fun name ->                
//                JQueryUI.Controls.Button name
//            )
//            |> Controls.Sortable
//            |> Formlet.Map (fun xs ->
//                Log ("xs: ", xs)
//            )
//        
//        Div [
//            (
//                Formlet.Yield (fun _ d xs -> ())
//                <*> Controls.Input ""
//                <*> date
//                <*> sortable
//            )
//            
//        ]        
        
       

        