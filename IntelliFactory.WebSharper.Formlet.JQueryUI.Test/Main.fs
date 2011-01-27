﻿namespace IntelliFactory.WebSharper.Formlet.JQueryUI.XTest

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Web
open IntelliFactory.WebSharper

open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.Formlet.JQueryUI

module Sample =

    [<JavaScript>]
    let Main () = 
        Tests.TestAccordionChoose ()

type SampleControl() =
    inherit Web.Control()

    [<JavaScript>]
    override this.Body =
        Div [Sample.Main ()] :> _