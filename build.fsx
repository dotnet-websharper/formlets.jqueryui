#load "tools/includes.fsx"
open IntelliFactory.Build

let bt =
    BuildTool().PackageId("WebSharper.Formlets.JQueryUI", "2.5")
        .References(fun r ->
            [
                r.NuGet("WebSharper.JQueryUI").Reference()
                r.Assembly "System.Web"
            ])

let main =
    bt.WebSharper.Library("IntelliFactory.WebSharper.Formlet.JQueryUI")
        .SourcesFromProject()

let test =
    bt.WebSharper.HtmlWebsite("IntelliFactory.WebSharper.Formlet.JQueryUI.Tests")
        .SourcesFromProject()
        .References(fun r -> [r.Project main])

bt.Solution [
    main
    test

    bt.NuGet.CreatePackage()
        .Configure(fun c ->
            { c with
                Title = Some "WebSharper.Formlet.JQueryUI"
                LicenseUrl = Some "http://websharper.com/licensing"
                Description = "WebSharper Formlets for JQueryUI"
                RequiresLicenseAcceptance = true })
        .Add(main)

]
|> bt.Dispatch
