#r "paket: groupref build //"
#if !FAKE
// See https://github.com/ionide/ionide-vscode-fsharp/issues/839#issuecomment-396296095.
#r "netstandard"
#r "Facades/netstandard"
#endif

#load "./.fake/build.fsx/intellisense.fsx"

#nowarn "52"

open System

open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Tools.Git

let [<Literal>] private VISUALIZATION_FILENAME = "visualization.svg" // keep synchronized with ./src/ui/app.fs and ./src/visualizer-console/visualizer.fs

let private testsDir = Path.getFullName "./src/tests"

let private uiDir = Path.getFullName "./src/ui"
let private uiPublicDir = uiDir </> "public"
let private uiPublishDir = uiDir </> "publish"

let private visualizerConsoleDir = Path.getFullName "./src/visualizer-console"

let private platformTool tool winTool =
    let tool = if Environment.isUnix then tool else winTool
    match ProcessUtils.tryFindFileOnPath tool with
    | Some t -> t
    | None -> failwithf "%s not found in path. Please install it and make sure it's available from your path. See https://safe-stack.github.io/docs/quickstart/#install-pre-requisites for more info." tool

let private yarnTool = platformTool "yarn" "yarn.cmd"

let private runTool cmd args workingDir =
    let arguments = args |> String.split ' ' |> Arguments.OfArgs
    RawCommand (cmd, arguments)
    |> CreateProcess.fromCommand
    |> CreateProcess.withWorkingDirectory workingDir
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

let private runDotNet cmd workingDir =
    let result = DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd String.Empty
    if result.ExitCode <> 0 then failwithf "'dotnet %s' failed in %s." cmd workingDir

let private openBrowser url =
    ShellCommand url
    |> CreateProcess.fromCommand
    |> CreateProcess.ensureExitCodeWithMessage "Unable to open browser."
    |> Proc.run
    |> ignore

Target.create "clean" (fun _ ->
    !! (uiDir </> "bin")
    ++ (uiDir </> "obj")
    ++ uiPublishDir
    |> Seq.iter Shell.cleanDir)

Target.create "restore" (fun _ ->
    printfn "Yarn version:"
    runTool yarnTool "--version" __SOURCE_DIRECTORY__
    runTool yarnTool "install --frozen-lockfile" __SOURCE_DIRECTORY__
    runDotNet "restore" uiDir)

Target.create "run-tests" (fun _ -> runDotNet "run -c Release" testsDir)

Target.create "run-visualizer-console" (fun _ -> runDotNet "run" visualizerConsoleDir)

Target.create "copy-visualization-file" (fun _ ->
    let visualizationFile = visualizerConsoleDir </> VISUALIZATION_FILENAME
    if File.exists visualizationFile then
        Shell.copyFile (uiPublicDir </> VISUALIZATION_FILENAME) visualizationFile)

Target.create "run" (fun _ ->
    let client = async { runTool yarnTool "webpack-dev-server" __SOURCE_DIRECTORY__ }
    let browser = async {
        do! Async.Sleep 2500
        openBrowser "http://localhost:8080" }
    Async.Parallel [ client ; browser ] |> Async.RunSynchronously |> ignore)

Target.create "build" (fun _ -> runTool yarnTool "webpack-cli -p" __SOURCE_DIRECTORY__)

Target.create "publish-gh-pages" (fun _ ->
    let tempGhPagesDir = __SOURCE_DIRECTORY__ </> "temp-gh-pages"
    Shell.cleanDir tempGhPagesDir
    Repository.cloneSingleBranch "" "https://github.com/aornota/duh.git" "gh-pages" tempGhPagesDir
    Repository.fullclean tempGhPagesDir
    Shell.copyRecursive uiPublishDir tempGhPagesDir true |> Trace.logfn "%A"
    Staging.stageAll tempGhPagesDir
    Commit.exec tempGhPagesDir (sprintf "Publish gh-pages (%s)" (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")))
    Branches.push tempGhPagesDir)

Target.create "help" (fun _ ->
    printfn "\nThese useful build targets can be run via 'fake build -t {target}':"
    printfn "\n\trun -> builds and runs [Release] tests; builds and runs [Debug] visualizer-console (and copies output to .\\src\\ui\\public); and builds, runs and watches [non-production] ui (served via webpack-dev-server)"
    printfn "\n\tbuild -> builds and runs [Release] tests; builds and runs [Debug] visualizer-console (and copies output to .\\src\\ui\\public); and builds [production] ui (which writes output to .\\src\\ui\\publish)"
    printfn "\n\tpublish-gh-pages -> builds and runs [Release] tests; builds and runs [Debug] visualizer-console (and copies output to .\\src\\ui\\public); builds [production] ui (which writes output to .\\src\\ui\\publish); and pushes to gh-pages branch"
    printfn "\n\trun-tests -> builds and runs [Release] tests"
    printfn "\n\trun-visualizer-console -> builds and runs [Debug] visualizer-console (which writes output to .\\src\\visualizer-console)"
    printfn "\n\thelp -> shows this list of build targets\n")

"clean" ==> "restore"
"run-tests" ==> "run-visualizer-console" ==> "copy-visualization-file"
"restore" ==> "copy-visualization-file" ==> "run"
"restore" ==> "copy-visualization-file" ==> "build" ==> "publish-gh-pages"

Target.runOrDefaultWithArguments "help"
