module Aornota.Duh.Common.AdaptiveValues

open Aornota.Duh.Common.ChangeableValues
open Aornota.Duh.Common.DependencyPaths

open FSharp.Data.Adaptive

let private depth = function | Self -> 0 | PackageDependency depth | ProjectDependency depth -> depth

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
                -- Do we want to filter out direct-to-project dependencies?...
                    - can give weird results (e.g. for Common.Interfaces code changes, Repositories.Tests is in step 4 - but this has a project reference to Repositories, which is in step 5)...
                    - so maybe handle appropriately when displaying analysis?... *)
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
    let packagedProjectStatuses = packagedProjectStatusMap |> List.ofSeq
    if packagedProjectStatuses |> List.map snd |> List.exists (fun pps -> pps.HasCodeChanges) then
        let affected = (projectsDependencyPaths.Force ()) |> List.choose affectedProjectDependencyPaths
        transact (fun () -> cTabLatestDoneMap.[Development] <- None)
        transact (fun () -> cTabLatestDoneMap.[CommittingPushing] <- None)
        return! AVal.map3 (fun a b c -> Some(a, b, c)) (AVal.constant affected) cCurrentTab (cTabLatestDoneMap |> AMap.toAVal)
    else return! AVal.constant None }
