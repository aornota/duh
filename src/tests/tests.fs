module Aornota.Duh.Tests.Tests

open Aornota.Duh.Common.AdaptiveValues
open Aornota.Duh.Common.ChangeableValues
open Aornota.Duh.Common.DependencyPaths
open Aornota.Duh.Common.Domain
open Aornota.Duh.Common.DomainData
open Aornota.Duh.Common.Utility

open Expecto

open FSharp.Data.Adaptive

let [<Tests>] domainDataTests =
    testList "domain data tests" [
        test "no unknown SolutionName values in projectMap" {
            let invalid =
                projectMap
                |> List.ofSeq
                |> List.map (fun kvp -> kvp.Value)
                |> List.filter (fun project -> not (solutionMap.ContainsKey project.SolutionName))
                |> List.map (fun project -> project.Name)
            Expect.isEmpty invalid (sprintf "Projects exist in projectMap for which SolutionName is not in solutionMap: %s" (invalid |> concatenatePipe)) }
        test "no unknown ProjectName values in projectsDependencies" {
            let invalid =
                projectsDependencies
                |> List.filter (fun pd -> not (projectMap.ContainsKey pd.ProjectName))
                |> List.map (fun pd -> pd.ProjectName)
            Expect.isEmpty invalid (sprintf "Projects exist in projectsDependencies for which ProjectName is not in projectMap: %s" (invalid |> concatenatePipe)) }
        test "no unknown project names for Dependencies in projectsDependencies" {
            let invalid =
                projectsDependencies
                |> List.choose (fun pd ->
                    match pd.Dependencies |> List.ofSeq |> List.map dependencyProjectName |> List.filter (projectMap.ContainsKey >> not) with
                    | [] -> None
                    | invalid -> Some (sprintf "%s (%s)" pd.ProjectName (invalid |> concatenate "; ")))
            Expect.isEmpty invalid (sprintf "Projects exist in projectsDependencies for which Dependencies are not in projectMap: %s" (invalid |> concatenatePipe)) }
        test "no cyclic dependencies or self-references in projectsDependencies" {
            (* Notes:
                -- This assumes that forcing evaluation of projectsDependencyPaths will fail if (and only if) there are cyclic dependencies or self-references.
                -- Need to use try/with as otherwise test will be reported as "errored" (rather than "failed") and calling code will not know to skip remaining [<Tests>]. *)
            try projectsDependencyPaths.Force () |> ignore
            with | exn -> failtest exn.Message } ]

let [<Tests>] adaptiveAnalysisScenarioTests =
    testList "adaptive analysis scenario tests" [
        test "simple scenario" {
            let setHasCodeChanges projectName = transact (fun _ -> cPackagedProjectStatusMap.[projectName] <- { ProjectName = projectName ; HasCodeChanges = true })
            (* TODO-NMB:
                - "Transact" cPackagedProjectStatusMap (and cCurrentTab &c.?)...
                    -- then "force" aAnalysis...
                    ...and check as expected (possibly using simplified structure, e.g. sorted by max-depth | ordered by project name and by direct dependency only?)... *)
            setHasCodeChanges "Product.Interfaces"
            let analysis = aAnalysis |> AVal.force
            //failtestf "%A" analysis
            Expect.equal true true "never fails" } ] // TEMP-NMB...

let [<Tests>] warningOnlyTests =
    testList "warning-only tests" [
        (* Note: Remember to be cautious when removing superfluous package references, e.g. if removing the last package reference/s for a project (which must mean that the
                 implicit reference/s are via project reference/s), the implicit references will only work if the project (now with no package references) is explicity configured
                 to use "new-style" package references. *)
        test "no superfluous package references" {
            let isPackageDependency = function | PackageDependency _ -> true | _ -> false
            let invalid =
                projectsDependencyPaths.Force ()
                |> List.choose (fun pdp ->
                    let moreThanOnce =
                        pdp.DependencyPaths
                        |> List.map (fun (DependencyPath dp) -> dp |> List.last)
                        |> List.filter (fun di -> isPackageDependency di.DependencyType)
                        |> List.filter (fun di ->
                            pdp.DependencyPaths
                            |> List.map (fun (DependencyPath dp) -> dp)
                            |> List.filter (fun dp -> dp |> List.last <> di)
                            |> List.exists (fun dp -> dp |> List.exists (fun diOther -> isPackageDependency diOther.DependencyType && diOther.ProjectName = di.ProjectName)))
                    match moreThanOnce with
                    | [] -> None
                    | moreThanOnce ->
                        let moreThanOnceNames = moreThanOnce |> List.map (fun di -> di.ProjectName) |> List.distinct |> concatenate "; "
                        Some (sprintf "%s (%s)" pdp.ProjectName moreThanOnceNames))
            Expect.isEmpty invalid (sprintf "Projects exist in projectsDependencies with superflous package references: %s" (invalid |> concatenatePipe)) } ]
