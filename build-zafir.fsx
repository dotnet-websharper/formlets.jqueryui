#load "tools/includes.fsx"
open IntelliFactory.Build

let bt =
    BuildTool().PackageId("WebSharper.Formlets.JQueryUI")
        .VersionFrom("WebSharper")
        .WithFSharpVersion(FSharpVersion.FSharp30)
        .WithFramework(fun fw -> fw.Net40)
        .References(fun r ->
            [
                r.Assembly "System.Web"
                r.NuGet("WebSharper.Html").Latest(true).ForceFoundVersion().Reference()
                r.NuGet("WebSharper.JQueryUI").Latest(true).ForceFoundVersion().Reference()
                r.NuGet("WebSharper.Reactive").Latest(true).ForceFoundVersion().Reference()
                r.NuGet("WebSharper.Formlets").Latest(true).ForceFoundVersion().Reference()
            ])

let main =
    bt.WebSharper4.Library("WebSharper.Formlets.JQueryUI")
        .SourcesFromProject()

let test =
    bt.WebSharper4.HtmlWebsite("WebSharper.Formlets.JQueryUI.Tests")
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
