module Aornota.Duh.DevConsole.Program

open Aornota.Duh.Common.Console
open Aornota.Duh.Common.SourcedLogger

open System

open Microsoft.Extensions.Configuration

open Giraffe.SerilogExtensions

open Serilog

let [<Literal>] private SOURCE = "DevConsole.Program"

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
        (* TEMP-NMB... *)
        writeNewLine "\nLogging examples:\n" ConsoleColor.DarkYellow
        let test = Some 3.14
        sourcedLogger.Debug("This is a debug message")
        sourcedLogger.Information("This is an information message: {test}", test)
        sourcedLogger.Warning("This is a warning message")
        failwith "Fake error. Sad!"

        // TODO-NMB...

    with | exn ->
        sourcedLogger.Error("Unexpected error: {errorMessage}\n{stackTrace}", exn.Message, exn.StackTrace)
        retval <- 1

    writeNewLine "Press any key to exit..." ConsoleColor.Magenta
    Console.ReadKey() |> ignore
    writeBlankLine()
    return retval }

[<EntryPoint>]
let main argv =
    async {
        do! Async.SwitchToThreadPool()
        return! mainAsync argv
    } |> Async.RunSynchronously
