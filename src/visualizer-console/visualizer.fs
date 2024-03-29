﻿// "Adapted" from Sergey Tihon's example (https://gist.github.com/sergey-tihon/46824acffb8c288fc5fe).

module Aornota.Duh.VisualizerConsole.Visualizer

open Aornota.Duh.Common.Domain
open Aornota.Duh.Common.DomainData
open Aornota.Duh.Common.SourcedLogger
open Aornota.Duh.Common.Utility

open System
open System.Diagnostics
open System.IO

open Serilog

let [<Literal>] private GRAPH_VIZ__DOT_EXE = @"C:\Program Files (x86)\Graphviz2.38\bin\dot.exe"
let [<Literal>] private GRAPH_VIZ__INPUT_FILENAME = "visualization.dot.graphviz"

let [<Literal>] private VISUALIZATION_FILENAME = "visualization.svg" // note: keep synchronized with ../ui/app.fs and ../../build.fsx

let [<Literal>] private SOURCE = "VisualizerConsole.Visualizer"

let private quoteName n = sprintf "\"%s\"" n

let private isPackageReference = function | PackageReference _ -> true | ProjectReference _ -> false

let private dependencyIsPackaged dependency = match projectMap.[dependencyProjectName dependency].ProjectType with | Some (Packaged _) -> true | _ -> false

let private writeProjectDependencies writer (projectDependencies:ProjectDependencies) =
    let getNodes dependencies = dependencies |> List.map (dependencyProjectName >> quoteName) |> List.sort |> concatenateComma
    let fromNode = quoteName projectDependencies.ProjectName
    let packageReferences = projectDependencies.Dependencies |> List.ofSeq |> List.filter isPackageReference
    if packageReferences.Length > 0 then
        let notPackaged = packageReferences |> List.filter (dependencyIsPackaged >> not)
        if notPackaged.Length > 0 then
            let errant = notPackaged |> List.map dependencyProjectName |> concatenatePipe
            failwithf "The following project/s are listed as package references for %s but are not packaged: %s" projectDependencies.ProjectName errant
        fprintfn writer "   %s -> { rank=none; %s }" fromNode (getNodes packageReferences)
    let projectReferences = projectDependencies.Dependencies |> Seq.filter (isPackageReference >> not) |> List.ofSeq
    if projectReferences.Length > 0 then fprintfn writer "   %s -> { rank=none; %s } [style=dashed]" fromNode (getNodes projectReferences)

let private createGraphvizInputFile (logger:ILogger) (inputFile:string) projectsDependencies =
    logger.Information ("Creating Graphviz input file {inputFile} from dependencies", inputFile)
    use writer = new StreamWriter (path = inputFile)
    fprintfn writer "digraph G {"
    fprintfn writer "    page=\"40,60\"; "
    fprintfn writer "    ratio=auto;"
    fprintfn writer "    rankdir=LR;"
    fprintfn writer "    fontsize=10;"
    projectsDependencies
    |> List.sortBy (fun pd -> pd.ProjectName)
    |> List.iter (writeProjectDependencies writer)
    projectsDependencies
    |> List.iter (fun pd->
        let fromNode = quoteName pd.ProjectName
        let project = projectMap.[pd.ProjectName]
        let shape = match project.ProjectType with | Some (Packaged _) -> "shape=box," | _ -> String.Empty
        let border = match project.ProjectType with | Some Tests -> "dashed" | _ -> "solid"
        let colour = quoteName (solutionMap.[project.SolutionName].Colour.ToString().ToLower())
        fprintfn writer "   %s [%sfillcolor=%s,style=\"filled,%s\"];" fromNode shape colour border)
    fprintfn writer "   }"

let private startProcessAndCaptureStandardOutput (logger:ILogger) cmd cmdParams =
    logger.Information ("Starting process {cmd} {cmdParams}", cmd, cmdParams);
    let si = ProcessStartInfo (cmd, cmdParams)
    si.UseShellExecute <- false
    si.RedirectStandardError <- true
    si.RedirectStandardOutput <- true
    use p = new Process()
    p.StartInfo <- si
    if p.Start() then
        use stdError = p.StandardError
        let error = stdError.ReadToEnd()
        if not (String.IsNullOrWhiteSpace error) then failwith error
        use stdOut = p.StandardOutput
        let output = stdOut.ReadToEnd()
        if not (String.IsNullOrWhiteSpace output) then logger.Warning ("Process completed with the following output (which might indicate a problem) -> {output}", output)
        else logger.Information ("Process completed")
    else failwith "Unable to start process"

let private generateVisualizationFile (logger:ILogger) (inputFile:string) (visualizationFile:string) =
    logger.Information ("Generating visualization file {visualizationFile} using Graphviz input file {inputFile}", visualizationFile, inputFile)
    let cmd = quoteName GRAPH_VIZ__DOT_EXE
    let cmdParams = sprintf "-T svg -o %s %s" (quoteName visualizationFile) (quoteName inputFile)
    startProcessAndCaptureStandardOutput logger cmd cmdParams

let visualize logger =
    let logger = logger |> sourcedLogger SOURCE

    let inputFile = Path.Combine(__SOURCE_DIRECTORY__, GRAPH_VIZ__INPUT_FILENAME)
    createGraphvizInputFile logger inputFile projectsDependencies

    let visualizationFile = Path.Combine(__SOURCE_DIRECTORY__, VISUALIZATION_FILENAME)
    generateVisualizationFile logger inputFile visualizationFile
