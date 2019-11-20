module Aornota.Duh.Tests.Program

open Aornota.Duh.Common.Console
open Aornota.Duh.Common.SourcedLogger

open System

open Expecto

open Giraffe.SerilogExtensions

open Microsoft.Extensions.Configuration

open Serilog

let [<Literal>] private SOURCE = "Tests.Program"

let private configuration =
    ConfigurationBuilder()
        .AddJsonFile("appsettings.json", false)
        .Build()

do Log.Logger <-
    LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .Destructure.FSharpTypes()
        .CreateLogger()

let private sourcedLogger = Log.Logger |> sourcedLogger SOURCE

let private mainAsync argv = async {
    writeNewLine (sprintf "Running %s.mainAsync" SOURCE) ConsoleColor.Magenta
    write (sprintf " %A" argv) ConsoleColor.DarkMagenta
    write "..." ConsoleColor.Magenta

    let mutable retval = 0

    try
        writeNewLine "\ndomain data tests:\n" ConsoleColor.DarkYellow
        retval <- runTestsWithArgs defaultConfig argv Tests.domainDataTests

        // Note: Skip remaining tests if any domain data tests failed (but otherwise run all remaining tests, even if some of them fail).

        if retval = 1 then
            writeBlankLine ()
            sourcedLogger.Warning "Skipping remaining tests because one or more domain data tests failed"
        else
            writeNewLine "adaptive analysis scenario tests:\n" ConsoleColor.DarkYellow
            if runTestsWithArgs defaultConfig argv Tests.adaptiveAnalysisScenarioTests = 1 then retval <-1

            writeNewLine "warning-only tests:\n" ConsoleColor.DarkYellow
            if runTestsWithArgs defaultConfig argv Tests.warningOnlyTests = 1 then
                writeBlankLine ()
                sourcedLogger.Warning "One or more warning-only tests failed"
            else
                writeBlankLine ()
                sourcedLogger.Information "All tests passed"

        if retval = 1 then
            writeBlankLine ()
            sourcedLogger.Error "One or more tests failed"
    with | exn ->
        sourcedLogger.Error ("Unexpected error: {errorMessage}\n{stackTrace}", exn.Message, exn.StackTrace)
        retval <- 1

    return retval }

[<EntryPoint>]
let main argv =
    async {
        do! Async.SwitchToThreadPool()
        return! mainAsync argv
    } |> Async.RunSynchronously
