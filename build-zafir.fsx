#load "tools/includes.fsx"
open IntelliFactory.Build

let bt =
    BuildTool().PackageId("Zafir.Formlets.JQueryUI")
        .VersionFrom("Zafir")
        .WithFSharpVersion(FSharpVersion.FSharp30)
        .WithFramework(fun fw -> fw.Net40)
        .References(fun r ->
            [
                r.Assembly "System.Web"
                r.NuGet("Zafir.Html").Latest(true).ForceFoundVersion().Reference()
                r.NuGet("Zafir.JQueryUI").Latest(true).ForceFoundVersion().Reference()
                r.NuGet("Zafir.Reactive").Latest(true).ForceFoundVersion().Reference()
                r.NuGet("Zafir.Formlets").Latest(true).ForceFoundVersion().Reference()
            ])

let main =
    bt.Zafir.Library("WebSharper.Formlets.JQueryUI")
        .SourcesFromProject()

let test =
    bt.Zafir.HtmlWebsite("WebSharper.Formlets.JQueryUI.Tests")
        .SourcesFromProject()
        .References(fun r -> [r.Project main])

bt.Solution [
    main
    test

    bt.NuGet.CreatePackage()
        .Configure(fun c ->
            { c with
                Title = Some "Zafir.Formlets.JQueryUI"
                LicenseUrl = Some "http://websharper.com/licensing"
                Description = "WebSharper Formlets for JQueryUI"
                RequiresLicenseAcceptance = true })
        .Add(main)

]
|> bt.Dispatch
