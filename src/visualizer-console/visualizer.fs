// "Adapted" from Sergey Tihon's example (https://gist.github.com/sergey-tihon/46824acffb8c288fc5fe).

module Aornota.Duh.VisualizerConsole.Visualizer

open Aornota.Duh.Common.Domain
open Aornota.Duh.Common.ProjectDependencyData
open Aornota.Duh.Common.SourcedLogger

open System
open System.Diagnostics
open System.IO

open Serilog

let [<Literal>] private GRAPH_VIZ__DOT_EXE = @"C:\Program Files (x86)\Graphviz2.38\bin\dot.exe"
let [<Literal>] private GRAPH_VIZ__INPUT_FILENAME = "visualization.dot.graphviz"

let [<Literal>] private VISUALIZATION_FILENAME = "visualization.png" // note: keep synchronized with ../../build.fsx

let [<Literal>] private SOURCE = "VisualizerConsole.Visualizer"

let private quoteName n = sprintf "\"%s\"" n

let private toCsv separator items = match items with | [] -> String.Empty | _ -> List.reduce (fun item1 item2 -> sprintf "%s%s%s" item1 separator item2) items

let private writeProjectDependencies writer (projectDependencies:ProjectDependencies) =
    let fromNode = quoteName (projectOrPackageName projectDependencies.ProjectOrPackage)
    let toNodes =
        projectDependencies.PackageReferences
        |> Seq.map (fun (Package project) -> quoteName project.Name)
        |> Seq.sort
        |> Seq.toList
        |> toCsv "; "
    fprintfn writer "   %s -> { rank=none; %s }" fromNode toNodes

let private createGraphvizInputFile (logger:ILogger) (inputFile:string) projectsDependencies =
    logger.Information("Creating Graphviz input file {inputFile} from dependencies", inputFile)
    use writer = new StreamWriter(path=inputFile)
    fprintfn writer "digraph G {"
    fprintfn writer "    page=\"40,60\"; "
    fprintfn writer "    ratio=auto;"
    fprintfn writer "    rankdir=LR;"
    fprintfn writer "    fontsize=10;"
    projectsDependencies
    |> Seq.sortBy (fun projectDependencies -> projectOrPackageName projectDependencies.ProjectOrPackage)
    |> Seq.iter (writeProjectDependencies writer)
    projectsDependencies
    |> Seq.iter (fun projectDependencies->
        let fromNode = quoteName (projectOrPackageName projectDependencies.ProjectOrPackage)
        let colour = quoteName (projectOrPackageColour projectDependencies.ProjectOrPackage)
        fprintfn writer "   %s [color=%s,style=filled];" fromNode colour)
    fprintfn writer "   }"

let private startProcessAndCaptureStandardOutput (logger:ILogger) cmd cmdParams =
    logger.Information("Starting process {cmd} {cmdParams}", cmd, cmdParams);
    let si = ProcessStartInfo(cmd, cmdParams)
    si.UseShellExecute <- false
    si.RedirectStandardError <- true
    si.RedirectStandardOutput <- true
    use p = new Process()
    p.StartInfo <- si
    if p.Start() then
        use stdError = p.StandardError
        let error = stdError.ReadToEnd()
        if not (String.IsNullOrWhiteSpace(error)) then failwith error
        use stdOut = p.StandardOutput
        let output = stdOut.ReadToEnd()
        if not (String.IsNullOrWhiteSpace(output)) then logger.Warning("Process completed with the following output (which might indicate a problem) -> {output}", output)
        else logger.Information("Process completed")
    else failwith "Unable to start process"

let private generateVisualizationFile (logger:ILogger) (inputFile:string) (visualizationFile:string) =
    logger.Information("Generating visualization file {visualizationFile} using Graphviz input file {inputFile}", visualizationFile, inputFile)
    let cmd = quoteName GRAPH_VIZ__DOT_EXE
    let cmdParams = sprintf "-T png -o %s %s" (quoteName visualizationFile) (quoteName inputFile)
    startProcessAndCaptureStandardOutput logger cmd cmdParams

let visualize logger =
    let logger = logger |> sourcedLogger SOURCE

    let inputFile = Path.Combine(__SOURCE_DIRECTORY__, GRAPH_VIZ__INPUT_FILENAME)
    createGraphvizInputFile logger inputFile projectsDependencies

    let visualizationFile = Path.Combine(__SOURCE_DIRECTORY__, VISUALIZATION_FILENAME)
    generateVisualizationFile logger inputFile visualizationFile
