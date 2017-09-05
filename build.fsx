#load "tools/includes.fsx"
open IntelliFactory.Build

let bt =
    BuildTool().PackageId("WebSharper.Formlets.JQueryUI")
        .VersionFrom("WebSharper", versionSpec = "(,4.0)")
        .WithFSharpVersion(FSharpVersion.FSharp30)
        .WithFramework(fun fw -> fw.Net40)
        .References(fun r ->
            [
                r.Assembly "System.Web"
                r.NuGet("WebSharper.Html").Version("(,4.0)").ForceFoundVersion().Reference()
                r.NuGet("WebSharper.JQueryUI").Version("(,4.0)").ForceFoundVersion().Reference()
                r.NuGet("IntelliFactory.Reactive").ForceFoundVersion().Reference()
                r.NuGet("WebSharper.Formlets").Version("(,4.0)").ForceFoundVersion().Reference()
            ])

let main =
    bt.WebSharper.Library("WebSharper.Formlets.JQueryUI")
        .SourcesFromProject()

let test =
    bt.WebSharper.HtmlWebsite("WebSharper.Formlets.JQueryUI.Tests")
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
