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
        // Note: This test will be skipped unless IS_SCENARIO_TEST_DATA (in ../src/common/domain-data.fs) is true.
        test "simple scenario" {
            if not IS_SCENARIO_TEST_DATA then skiptest "Test can only be run when domain data is scenario test data"

            let setHasCodeChanges projectName = transact (fun _ -> cPackagedProjectStatusMap.[projectName] <- { ProjectName = projectName ; HasCodeChanges = true })

            let mutable analysis = aAnalysis |> AVal.force
            Expect.isNone analysis "Analysis should be None before marking any packages as having code changes"

            setHasCodeChanges "Common.Extensions"
            analysis <- aAnalysis |> AVal.force
            // TODO-NMB: The expected results are a pain to maintain - so only check a (sorted?) subset and/or a summary?...
            let mutable expectedAffected = [
                ({
                    ProjectName = "Common.Extensions"
                    DependencyPaths = [
                        DependencyPath [
                            { ProjectName = "Common.Extensions" ; DependencyType = Self } ] ]
                }, 0)
                ({
                    ProjectName = "Repositories.Tests"
                    DependencyPaths = [
                        DependencyPath [
                            { ProjectName = "Common.Extensions" ; DependencyType = PackageDependency 1 } ] ]
                }, 1)
                ({
                    ProjectName = "Tools"
                    DependencyPaths = [
                        DependencyPath [
                            { ProjectName = "Common.Extensions" ; DependencyType = PackageDependency 1 } ] ]
                }, 1);
                ({
                    ProjectName = "Tools.Tests"
                    DependencyPaths = [
                        DependencyPath [
                            { ProjectName = "Common.Extensions" ; DependencyType = PackageDependency 2 }
                            { ProjectName = "Tools" ; DependencyType = ProjectDependency 1 } ] ]
                }, 2) ]
            let mutable expectedCurrentTab = Development
            let mutable expectedTabLatestDoneMap = [(Development, None); (CommittingPushing, None)] |> HashMap.ofList
            Expect.equal analysis (Some (expectedAffected, expectedCurrentTab, expectedTabLatestDoneMap))
                "Specific analysis expected after marking Common.Extensions as having code changes"

            transact (fun _ -> cCurrentTab.Value <- CommittingPushing)
            analysis <- aAnalysis |> AVal.force
            expectedCurrentTab <- CommittingPushing
            Expect.equal analysis (Some (expectedAffected, expectedCurrentTab, expectedTabLatestDoneMap))
                "Same specific analysis expected - but current tab should now be CommittingPushing - after changing the current tab to CommittingPushing"

            transact (fun _ -> cTabLatestDoneMap.[CommittingPushing] <- Some 1)
            analysis <- aAnalysis |> AVal.force
            expectedTabLatestDoneMap <- [(Development, None); (CommittingPushing, Some 1)] |> HashMap.ofList
            Expect.equal analysis (Some (expectedAffected, expectedCurrentTab, expectedTabLatestDoneMap))
                "Same specific analysis and current tab expected - but with step 1 marked as done - after marking step 1 as done"

            setHasCodeChanges "Order.Models"
            analysis <- aAnalysis |> AVal.force
            expectedAffected <- [
                ({
                    ProjectName = "Common.Extensions"
                    DependencyPaths = [
                        DependencyPath [
                            { ProjectName = "Common.Extensions" ; DependencyType = Self } ] ]
                }, 0)
                ({
                    ProjectName = "Order.Models"
                    DependencyPaths = [
                        DependencyPath [
                            { ProjectName = "Order.Models" ; DependencyType = Self } ] ]
                }, 0)
                ({
                    ProjectName = "Repositories"
                    DependencyPaths = [
                        DependencyPath [
                            { ProjectName = "Order.Models" ; DependencyType = PackageDependency 1 } ] ]
                }, 1)
                ({
                    ProjectName = "Repositories.Tests"
                    DependencyPaths = [
                        DependencyPath [
                            { ProjectName = "Common.Extensions" ; DependencyType = PackageDependency 1 } ]
                        DependencyPath [
                            { ProjectName = "Order.Models" ; DependencyType = PackageDependency 2 }
                            { ProjectName = "Repositories" ; DependencyType = ProjectDependency 1 } ] ]
                }, 2)
                ({
                    ProjectName = "Tools"
                    DependencyPaths = [
                        DependencyPath [
                            { ProjectName = "Common.Extensions" ; DependencyType = PackageDependency 1 } ]
                        DependencyPath [
                            { ProjectName = "Order.Models" ; DependencyType = PackageDependency 2 }
                            { ProjectName = "Repositories" ; DependencyType = PackageDependency 1 } ] ]
                }, 2)
                ({
                    ProjectName = "Tools.Tests"
                    DependencyPaths = [
                        DependencyPath [
                            { ProjectName = "Order.Models" ; DependencyType = PackageDependency 3 }
                            { ProjectName = "Repositories" ; DependencyType = PackageDependency 2 }
                            { ProjectName = "Tools" ; DependencyType = ProjectDependency 1 } ] ]
                }, 3) ]
            expectedTabLatestDoneMap <- [(Development, None); (CommittingPushing, None)] |> HashMap.ofList
            Expect.equal analysis (Some (expectedAffected, expectedCurrentTab, expectedTabLatestDoneMap))
                "Different specific analysis expected - with current tab still CommittingPushing but no steps marked as done - after also marking Order.Models as having code changes" } ]

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
            Expect.isEmpty invalid (sprintf "Projects exist in projectsDependencies with superfluous package references: %s" (invalid |> concatenatePipe)) } ]
