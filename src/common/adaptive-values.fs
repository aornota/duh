module Aornota.Duh.Common.AdaptiveValues

open Aornota.Duh.Common.ChangeableValues
open Aornota.Duh.Common.DependencyPaths
open Aornota.Duh.Common.Domain
open Aornota.Duh.Common.DomainData

open FSharp.Data.Adaptive

let private depth = function | Self -> 0 | PackageDependency depth | ProjectDependency depth -> depth

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

let aAnalysis = adaptive {
    let! packagedProjectStatusMap = cPackagedProjectStatusMap |> AMap.toAVal
    let hasCodeChanges projectName = match packagedProjectStatusMap.TryFind projectName with | Some pps -> pps.HasCodeChanges | None -> false
    let affectedProjectDependencyPaths (projectDependencyPaths:ProjectDependencyPaths) =
        let affectedDependencyPath (DependencyPath dependencyPath) =
            match dependencyPath |> List.mapi (fun i di -> if hasCodeChanges di.ProjectName then Some i else None) |> List.choose id with
            | [] -> None
            | affectedIndices ->
                let minAffectedIndex = affectedIndices |> List.min
                dependencyPath |> List.skip minAffectedIndex |> DependencyPath |> Some
        match projectDependencyPaths.DependencyPaths |> List.choose affectedDependencyPath with
        | [] -> None
        | affectedDependencyPaths ->
            (* TODO-NMB:
                - Do we want to filter out direct-to-project dependencies?...
                    -- can give weird results (e.g. for Common.Interfaces code changes, Repositories.Tests is in step 4 - but this has a project reference to Repositories, which is in step 5)...
                    -- so maybe handle appropriately when displaying analysis?... *)
            (*let affectedDependencyPaths =
                affectedDependencyPaths
                |> List.filter (fun (DependencyPath dp) ->
                    let selfOrDirect = dp |> List.last
                    match selfOrDirect.DependencyType with | ProjectDependency _ -> false | _ -> true)
            if affectedDependencyPaths.Length = 0 then None
            else*)
                let uniqueSelfOrDirectWithMaxDepth =
                    affectedDependencyPaths
                    |> List.groupBy (fun (DependencyPath dp) ->
                        let selfOrDirect = dp |> List.last
                        selfOrDirect.ProjectName)
                    |> List.map (fun (_, dependencyPaths) ->
                        dependencyPaths
                        |> List.map (fun (DependencyPath dp) ->
                            let deepest = dp |> List.head
                            DependencyPath dp, depth deepest.DependencyType)
                        |> List.maxBy snd)
                let maxDepth = uniqueSelfOrDirectWithMaxDepth |> List.map snd |> List.max
                Some ({ ProjectName = projectDependencyPaths.ProjectName ; DependencyPaths = uniqueSelfOrDirectWithMaxDepth |> List.map fst }, maxDepth)
    if packagedProjectStatusMap |> List.ofSeq |> List.map snd |> List.exists (fun pps -> pps.HasCodeChanges) then
        let! tabLatestDoneMap = cTabLatestDoneMap |> AMap.toAVal
        let affected =
            projectsDependencyPaths.Force()
            |> List.choose affectedProjectDependencyPaths
            |> List.groupBy snd
            |> List.sortBy fst
            |> List.map (fun (maxDepth, paths) ->
                let sorted = paths |> List.map fst |> List.sortBy (fun pdp -> solutionSortOrder (solution pdp.ProjectName), pdp.ProjectName)
                maxDepth + 1, sorted)
        (* Note: Not entirely sure why this needs to be explicitly ignoed - but otherwise get a "This control construct may only be used if the computation expression builder
                 defines a 'Zero' method" compilation error. *)
        (if Some affected <> lastAffected then resetLatestDone affected tabLatestDoneMap) |> ignore
        return! AVal.map3 (fun a b c -> Some(a, b, c)) (AVal.constant affected) cCurrentTab (cTabLatestDoneMap |> AMap.toAVal)
    else
        lastAffected <- None
        analysisTabs |> List.iter (fun tab -> transact (fun () -> cTabLatestDoneMap.[tab] <- None))
        return! AVal.constant None }
