#load "tools/includes.fsx"
open IntelliFactory.Build

let bt =
    BuildTool().PackageId("WebSharper.Formlets.JQueryUI", "3.0-alpha")
        .References(fun r ->
            [
                r.NuGet("WebSharper.JQueryUI").Reference()
                r.Assembly "System.Web"
            ])
    |> fun bt -> bt.WithFramework(bt.Framework.Net40)

let main =
    bt.WebSharper.Library("IntelliFactory.WebSharper.Formlets.JQueryUI")
        .SourcesFromProject()

let test =
    bt.WebSharper.HtmlWebsite("IntelliFactory.WebSharper.Formlets.JQueryUI.Tests")
        .SourcesFromProject()
        .References(fun r -> [r.Project main])

bt.Solution [
    main
    test

    bt.NuGet.CreatePackage()
        .Configure(fun c ->
            { c with
                Title = Some "WebSharper.Formlets.JQueryUI"
                LicenseUrl = Some "http://websharper.com/licensing"
                Description = "WebSharper Formlets for JQueryUI"
                RequiresLicenseAcceptance = true })
        .Add(main)

]
|> bt.Dispatch
