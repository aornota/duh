module Aornota.Duh.Tests.Tests

open Aornota.Duh.Common.AdaptiveValues
open Aornota.Duh.Common.ChangeableValues
open Aornota.Duh.Common.DependencyPaths
open Aornota.Duh.Common.Domain
open Aornota.Duh.Common.DomainData
open Aornota.Duh.Common.Utility

open Expecto

open FSharp.Data.Adaptive

let private isPackageDependency = function | PackageDependency _ -> true | _ -> false

let [<Tests>] domainDataTests =
    testList "domain data tests" [
        test "no solutions in solutionMap that are not referenced in projectMap" {
            let invalid =
                solutionMap
                |> List.ofSeq
                |> List.map (fun kvp -> kvp.Value)
                |> List.filter (fun solution -> projectMap |> List.ofSeq |> List.map (fun kvp -> kvp.Value) |> List.exists (fun project -> project.SolutionName = solution.Name) |> not)
                |> List.map (fun solution -> solution.Name)
            Expect.isEmpty invalid (sprintf "Solutions exist in solutionMap that are not referenced by any projects in projectMap: %s" (invalid |> concatenatePipe)) }
        test "no unknown SolutionName values in projectMap" {
            let invalid =
                projectMap
                |> List.ofSeq
                |> List.map (fun kvp -> kvp.Value)
                |> List.filter (fun project -> not (solutionMap.ContainsKey project.SolutionName))
                |> List.map (fun project -> project.Name)
            Expect.isEmpty invalid (sprintf "Projects exist in projectMap for which SolutionName is not in solutionMap: %s" (invalid |> concatenatePipe)) }
        test "no projects in projectMap that are not in projectsDependencies" {
            let invalid =
                projectMap
                |> List.ofSeq
                |> List.map (fun kvp -> kvp.Value)
                |> List.filter (fun project -> projectsDependencies |> List.exists (fun pd -> pd.ProjectName = project.Name) |> not)
                |> List.map (fun project -> project.Name)
            Expect.isEmpty invalid (sprintf "Projects exist in projectMap that are not in projectsDependencies: %s" (invalid |> concatenatePipe)) }
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
                - This assumes that forcing evaluation of projectsDependencyPaths will fail if (and only if) there are cyclic dependencies or self-references.
                - Need to use try/with as otherwise test will be reported as "errored" (rather than "failed") and calling code will not know to skip remaining [<Tests>]. *)
            try projectsDependencyPaths.Force() |> ignore
            with | exn -> failtest exn.Message } ]

let [<Tests>] adaptiveAnalysisScenarioTests =
    testList "adaptive analysis scenario tests" [
        // Note: This test will be skipped unless IS_SCENARIO_TEST_DATA (in ../src/common/domain-data.fs) is true.
        test "simple scenario" {
            if not IS_SCENARIO_TEST_DATA then skiptest "Test can only be run when domain data is scenario test data"

            let setHasCodeChanges projectName value = transact (fun _ -> cPackagedProjectStatusMap.[projectName] <- { ProjectName = projectName ; HasCodeChanges = value })
            let analysisSummary (analysis:(((int * ProjectDependencyPaths list) list) * AnalysisTab * HashMap<AnalysisTab, int option>) option) =
                match analysis with
                | Some (affected, currentTab, tabLatestDoneMap) ->
                    let affectedSummary =
                        affected
                        |> List.map (fun (ordinal, projectsDependencyPaths) ->
                            let ordinalSummary =
                                projectsDependencyPaths
                                |> List.map (fun pdp ->
                                    let pathsSummary =
                                        pdp.DependencyPaths
                                        |> List.map (fun di ->
                                            let selfOrDirect = di |> List.last
                                            selfOrDirect.ProjectName, isPackageDependency selfOrDirect.DependencyType)
                                    pdp.ProjectName, pathsSummary)
                            ordinal, ordinalSummary)
                    Some (affectedSummary, currentTab, tabLatestDoneMap)
                | None -> None

            let mutable analysis = aAnalysis |> AVal.force
            Expect.isNone analysis "Analysis should be None before marking any packages as having code changes"

            setHasCodeChanges "Common.Extensions" true
            analysis <- aAnalysis |> AVal.force
            let mutable expectedAffectedSummary = [
                (1, [
                    ("Common.Extensions", [
                        ("Common.Extensions", false) ]) ])
                (2, [
                    ("Repositories.Tests", [
                        ("Common.Extensions", true) ])
                    ("Tools", [
                        ("Common.Extensions", true) ]) ])
                (3, [
                    ("Tools.Tests", [
                        ("Tools", false) ]) ]) ]
            let mutable expectedCurrentTab = Development
            let mutable expectedTabLatestDoneMap = [(Development, None); (CommittingPushing, None)] |> HashMap.ofList
            Expect.equal (analysisSummary analysis) (Some (expectedAffectedSummary, expectedCurrentTab, expectedTabLatestDoneMap))
                "Specific analysis expected - with current tab defaulting to Development (with no steps marked as done) - after marking Common.Extensions as having code changes"

            transact (fun _ -> cCurrentTab.Value <- CommittingPushing)
            analysis <- aAnalysis |> AVal.force
            expectedCurrentTab <- CommittingPushing
            Expect.equal (analysisSummary analysis) (Some (expectedAffectedSummary, expectedCurrentTab, expectedTabLatestDoneMap))
                "Same specific analysis expected - but current tab should now be CommittingPushing (with no steps marked as done) - after changing the current tab to CommittingPushing"

            setHasCodeChanges "Order.Models" true
            analysis <- aAnalysis |> AVal.force
            expectedAffectedSummary <- [
                (1, [
                    ("Common.Extensions", [
                        ("Common.Extensions", false) ])
                    ("Order.Models", [
                        ("Order.Models", false) ]) ])
                (2, [
                    ("Order.Extensions", [
                        ("Order.Models", true) ])
                    ("Repositories", [
                        ("Order.Models", true) ]) ])
                (3, [
                    ("Repositories.Tests", [
                        ("Common.Extensions", true)
                        ("Repositories", false) ])
                    ("Tools", [
                        ("Common.Extensions", true)
                        ("Repositories", true) ]) ])
                (4, [
                    ("Tools.Tests", [
                        ("Order.Extensions", true)
                        ("Tools", false) ]) ]) ]
            Expect.equal (analysisSummary analysis) (Some (expectedAffectedSummary, expectedCurrentTab, expectedTabLatestDoneMap))
                "Different specific analysis expected - with current tab still CommittingPushing (with no steps marked as done) - after also marking Order.Models as having code changes"

            transact (fun _ -> cTabLatestDoneMap.[CommittingPushing] <- Some 1)
            analysis <- aAnalysis |> AVal.force
            expectedTabLatestDoneMap <- [(Development, None); (CommittingPushing, Some 1)] |> HashMap.ofList
            Expect.equal (analysisSummary analysis) (Some (expectedAffectedSummary, expectedCurrentTab, expectedTabLatestDoneMap))
                "Same specific analysis expected - and current tab still CommittingPushing (but with step 1 marked as done) - after marking step 1 as done for CommittingPushing"

            setHasCodeChanges "Repositories" true
            analysis <- aAnalysis |> AVal.force
            expectedAffectedSummary <- [
                (1, [
                    ("Common.Extensions", [
                        ("Common.Extensions", false) ])
                    ("Order.Models", [
                        ("Order.Models", false) ]) ])
                (2, [
                    ("Order.Extensions", [
                        ("Order.Models", true) ])
                    ("Repositories", [
                        ("Repositories", false)
                        ("Order.Models", true) ]) ])
                (3, [
                    ("Repositories.Tests", [
                        ("Common.Extensions", true)
                        ("Repositories", false) ])
                    ("Tools", [
                        ("Common.Extensions", true)
                        ("Repositories", true) ]) ])
                (4, [
                    ("Tools.Tests", [
                        ("Order.Extensions", true)
                        ("Tools", false) ]) ]) ]
            Expect.equal (analysisSummary analysis) (Some (expectedAffectedSummary, expectedCurrentTab, expectedTabLatestDoneMap))
                "Different specific analysis expected - with current tab still CommittingPushing (and step 1 still marked as done) - after also marking Repositories as having code changes"

            transact (fun _ -> cTabLatestDoneMap.[CommittingPushing] <- Some 2)
            analysis <- aAnalysis |> AVal.force
            expectedTabLatestDoneMap <- [(Development, None); (CommittingPushing, Some 2)] |> HashMap.ofList
            Expect.equal (analysisSummary analysis) (Some (expectedAffectedSummary, expectedCurrentTab, expectedTabLatestDoneMap))
                "Same specific analysis expected - and current tab still CommittingPushing (but with step 2 marked as done) - after marking step 2 as done for CommittingPushing"

            setHasCodeChanges "Order.Models" false
            analysis <- aAnalysis |> AVal.force
            expectedAffectedSummary <- [
                (1, [
                    ("Common.Extensions", [
                        ("Common.Extensions", false) ])
                    ("Repositories", [
                        ("Repositories", false) ]) ])
                (2, [
                    ("Repositories.Tests", [
                        ("Common.Extensions", true)
                        ("Repositories", false) ])
                    ("Tools", [
                        ("Common.Extensions", true)
                        ("Repositories", true) ]) ])
                (3, [
                    ("Tools.Tests", [
                        ("Tools", false) ]) ]) ]
            expectedTabLatestDoneMap <- [(Development, None); (CommittingPushing, None)] |> HashMap.ofList
            Expect.equal (analysisSummary analysis) (Some (expectedAffectedSummary, expectedCurrentTab, expectedTabLatestDoneMap))
                "Different specific analysis expected - with current tab still CommittingPushing (but with no steps marked as done) - after marking Order.Models as not having code changes" } ]

let [<Tests>] warningOnlyTests =
    testList "warning-only tests" [
        (* Note: Remember to be cautious when removing superfluous package references, e.g. if removing the last package reference/s for a project (which must mean that the
                 implicit reference/s are via project reference/s), the implicit references will only work if the project (now with no package references) is explicity configured
                 to use "new-style" package references. (The implicit references might seem to work locally - depending on default package reference settings in Visual Studio - but
                 explicit configuration will be required for, e.g., Jenkins.) *)
        test "no superfluous package references" {
            let invalid =
                projectsDependencyPaths.Force()
                |> List.choose (fun pdp ->
                    let moreThanOnce =
                        pdp.DependencyPaths
                        |> List.map (fun dp -> dp |> List.last)
                        |> List.filter (fun di -> isPackageDependency di.DependencyType)
                        |> List.filter (fun di ->
                            pdp.DependencyPaths
                            |> List.filter (fun dp -> dp |> List.last <> di)
                            |> List.exists (fun dp -> dp |> List.exists (fun diOther -> isPackageDependency diOther.DependencyType && diOther.ProjectName = di.ProjectName)))
                    match moreThanOnce with
                    | [] -> None
                    | moreThanOnce ->
                        let moreThanOnceNames = moreThanOnce |> List.map (fun di -> di.ProjectName) |> List.distinct |> concatenate "; "
                        Some (sprintf "%s (%s)" pdp.ProjectName moreThanOnceNames))
            Expect.isEmpty invalid (sprintf "Projects exist in projectsDependencies with superfluous package references: %s" (invalid |> concatenatePipe)) } ]
