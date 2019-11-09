﻿module Aornota.Duh.VisualizerConsole.Program

open Aornota.Duh.Common.Console
open Aornota.Duh.Common.SourcedLogger
open Aornota.Duh.VisualizerConsole.Visualizer

open System

open Microsoft.Extensions.Configuration

open Giraffe.SerilogExtensions

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
    // #region "Running SOURCE.mainAsync..."
    writeNewLine (sprintf "Running %s.mainAsync" SOURCE) ConsoleColor.Green
    write (sprintf " %A" argv) ConsoleColor.DarkGreen
    write "..." ConsoleColor.Green
    // #endregion

    try
        writeNewLine "\nvisualize:\n" ConsoleColor.Magenta
        visualize logger
    with | exn -> sourcedLogger.Error("Unexpected error -> {errorMessage}\n{stackTrace}", exn.Message, exn.StackTrace)

    // #region "Press any key to exit..."
    writeNewLine "Press any key to exit..." ConsoleColor.Green
    Console.ReadKey() |> ignore
    writeBlankLine()
    return 0
    // #endregion
}

[<EntryPoint>]
let main argv =
    async {
        do! Async.SwitchToThreadPool()
        return! mainAsync argv
    } |> Async.RunSynchronously
