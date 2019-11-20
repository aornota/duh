module Aornota.Duh.VisualizerConsole.Program

open Aornota.Duh.Common.Console
open Aornota.Duh.Common.SourcedLogger
open Aornota.Duh.VisualizerConsole.Visualizer

open System

open Giraffe.SerilogExtensions

open Microsoft.Extensions.Configuration

open Serilog

let [<Literal>] private SOURCE = "VisualizerConsole.Program"

let private configuration =
    ConfigurationBuilder()
        .AddJsonFile("appsettings.json", false)
        .Build()

do Log.Logger <-
    LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .Destructure.FSharpTypes()
        .CreateLogger()

let private logger = Log.Logger
let private sourcedLogger = logger |> sourcedLogger SOURCE

let private mainAsync argv = async {
    writeNewLine (sprintf "Running %s.mainAsync" SOURCE) ConsoleColor.Magenta
    write (sprintf " %A" argv) ConsoleColor.DarkMagenta
    write "..." ConsoleColor.Magenta

    let mutable retval = 0

    try
        writeNewLine "\nvisualize:\n" ConsoleColor.DarkYellow
        visualize logger
    with | exn ->
        sourcedLogger.Error ("Unexpected error -> {errorMessage}\n{stackTrace}", exn.Message, exn.StackTrace)
        retval <- 1

    writeBlankLine ()
    return retval }

[<EntryPoint>]
let main argv =
    async {
        do! Async.SwitchToThreadPool ()
        return! mainAsync argv
    } |> Async.RunSynchronously
