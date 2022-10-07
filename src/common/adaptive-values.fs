module Aornota.Duh.Common.AdaptiveValues

open Aornota.Duh.Common.ChangeableValues
open Aornota.Duh.Common.DependencyPaths
open Aornota.Duh.Common.Domain
open Aornota.Duh.Common.DomainData

open FSharp.Data.Adaptive

let private depth = function | PackageDependency depth | ProjectDependency depth -> depth | Self -> 0

let mutable private lastAffected : ((int * ProjectDependencyPaths list) list) option = None

let private resetLatestDone affected (tabLatestDoneMap:HashMap<AnalysisTab, int option>) =
    let resetTo (lastAffected:(int * ProjectDependencyPaths list) list) latestDone =
        let doneButChanged =
            lastAffected
            |> List.filter (fun (ordinal, paths) ->
                if ordinal <= latestDone then
                    match affected |> List.tryFind (fun (otherOrdinal, _) -> otherOrdinal = ordinal) with
                    | Some (_, otherPaths) -> otherPaths <> paths
                    | None -> true
                else false)
        match doneButChanged with
        | [] -> Some latestDone
        | _ -> match doneButChanged |> List.map fst |> List.min with 1 -> None | minChanged -> Some (minChanged - 1)
    match lastAffected with
    | Some lastAffected ->
        analysisTabs
        |> List.choose (fun tab -> match tabLatestDoneMap.[tab] with | Some latestDone -> Some (tab, resetTo lastAffected latestDone) | None -> None)
        |> List.iter (fun (tab, resetTo) -> transact (fun () -> cTabLatestDoneMap.[tab] <- resetTo))
    | None -> ()
    lastAffected <- Some affected

let solution projectName = solutionMap.[projectMap.[projectName].SolutionName]

let aPackagedProjectStatuses = adaptive {
    let! packagedProjectStatusMap = cPackagedProjectStatusMap |> AMap.toAVal
    return packagedProjectStatusMap |> List.ofSeq |> List.map snd }

let aAnalysis = adaptive {
    let! packagedProjectStatuses = aPackagedProjectStatuses
    let hasCodeChanges projectName =
        match packagedProjectStatuses |> List.tryFind (fun pps -> pps.ProjectName = projectName) with | Some pps -> pps.HasCodeChanges | None -> false
    let affectedProjectDependencyPaths (projectDependencyPaths:ProjectDependencyPaths) =
        let affectedDependencyPath (dependencyPath:DependencyPath) =
            match dependencyPath |> List.mapi (fun i di -> if hasCodeChanges di.ProjectName then Some i else None) |> List.choose id with
            | [] -> None
            | affectedIndices ->
                let minAffectedIndex = affectedIndices |> List.min
                dependencyPath |> List.skip minAffectedIndex |> Some
        match projectDependencyPaths.DependencyPaths |> List.choose affectedDependencyPath with
        | [] -> None
        | affectedDependencyPaths ->
            let uniqueSelfOrDirectWithMaxDepth =
                affectedDependencyPaths
                |> List.groupBy (fun dp ->
                    let selfOrDirect = dp |> List.last
                    selfOrDirect.ProjectName)
                |> List.map (fun (_, dependencyPaths) ->
                    dependencyPaths
                    |> List.map (fun dp ->
                        let deepest = dp |> List.head
                        dp, depth deepest.DependencyType)
                    |> List.maxBy snd)
            let maxDepth = uniqueSelfOrDirectWithMaxDepth |> List.map snd |> List.max
            Some ({ ProjectName = projectDependencyPaths.ProjectName ; DependencyPaths = uniqueSelfOrDirectWithMaxDepth |> List.map fst }, maxDepth)
    if packagedProjectStatuses |> List.exists (fun pps -> pps.HasCodeChanges) then
        let! tabLatestDoneMap = cTabLatestDoneMap |> AMap.toAVal
        let rec adjustForTestsProjects count (affected:(ProjectDependencyPaths * int) list) =
            let withHigherTestsProjectDepth =
                affected
                |> List.choose (fun (pdp, maxDepth) ->
                    let project = projectMap.[pdp.ProjectName]
                    match project.ProjectType with
                    | Some (Packaged (_, Some testsProjectName)) ->
                        match affected |> List.tryFind (fun (pdp, _) -> pdp.ProjectName = testsProjectName) with
                        | Some (_, testsProjectMaxDepth) when testsProjectMaxDepth > maxDepth -> Some (pdp.ProjectName, maxDepth, testsProjectName, testsProjectMaxDepth - maxDepth)
                        | _ -> None
                    | _ -> None)
                |> List.sortBy (fun (projectName, maxDepth, _, _) -> maxDepth, projectName)
            match count < 99, withHigherTestsProjectDepth with // note: limit count to guard against infinite recursion
            | true, (projectName, _, testsProjectName, depthOffset) :: _ ->
                // TEMP-DEBUG...Browser.Dom.console.log(sprintf "%i -> Adjusting for %s (depthOffset: %i)..." count projectName depthOffset)
                let rec adjust subCount (affected:(ProjectDependencyPaths * int) list) testsProjectName (projectNames:string list) =
                    match subCount < 99, projectNames with // note: limit subCount to guard against infinite recursion
                    | true, _ :: _ ->
                        let affectedPlus =
                            affected
                            |> List.map (fun (pdp, maxDepth) ->
                                if projectNames |> List.contains pdp.ProjectName then
                                    // TEMP-DEBUG...Browser.Dom.console.log(sprintf "...%i.%i -> Adjusting %s (maxDepth: %i -> %i)" count subCount pdp.ProjectName maxDepth (maxDepth + depthOffset))
                                    pdp, maxDepth + depthOffset, None
                                else if Some pdp.ProjectName = testsProjectName then pdp, maxDepth, None
                                else if pdp.DependencyPaths |> List.collect id |> List.exists (fun di -> projectNames |> List.contains di.ProjectName) then
                                    // TEMP-DEBUG...Browser.Dom.console.log(sprintf "...%i.%i -> Queueing %s" count subCount pdp.ProjectName)
                                    pdp, maxDepth, Some pdp.ProjectName
                                else pdp, maxDepth, None)
                        let affected = affectedPlus |> List.map (fun (paths, maxDepth, _) -> paths, maxDepth)
                        let projectNames = affectedPlus |> List.choose (fun (_, _, projectName) -> projectName)
                        adjust (subCount + 1) affected None projectNames
                    | _ -> affected
                let affected = adjust 1 affected (Some testsProjectName) [ projectName ]
                affected |> adjustForTestsProjects (count + 1)
            | _ -> affected
        let affected =
            projectsDependencyPaths.Force()
            |> List.choose affectedProjectDependencyPaths
            |> adjustForTestsProjects 1
            |> List.groupBy snd
            |> List.sortBy fst
            |> List.mapi (fun i (_, paths) ->
                let sorted = paths |> List.map fst |> List.sortBy (fun pdp -> solutionSortOrder (solution pdp.ProjectName), pdp.ProjectName)
                i + 1, sorted)
        (* Note: Not entirely sure why this needs to be explicitly ignored - but otherwise get a "This control construct may only be used if the computation expression builder
                 defines a 'Zero' method" compilation error. *)
        let _ = if Some affected <> lastAffected then resetLatestDone affected tabLatestDoneMap
        return! AVal.map3 (fun a b c -> Some(a, b, c)) (AVal.constant affected) cCurrentTab (cTabLatestDoneMap |> AMap.toAVal)
    else
        lastAffected <- None
        analysisTabs |> List.iter (fun tab -> transact (fun () -> cTabLatestDoneMap.[tab] <- None))
        return None }
