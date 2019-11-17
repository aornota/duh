module Aornota.Duh.Common.AdaptiveValues

open Aornota.Duh.Common.ChangeableValues
open Aornota.Duh.Common.Domain
open Aornota.Duh.Common.ProjectDependencyData

open FSharp.Data.Adaptive

type DependencyType = | Self | PackageDependency of int | ProjectDependency of int

type DependencyInfo = { Project : Project ; DependencyType : DependencyType }

type DependencyPath = | DependencyPath of DependencyInfo list

type ProjectDependencyPaths = { Project : Project ; DependencyPaths : DependencyPath list }

let private depth = function | Self -> 0 | PackageDependency depth | ProjectDependency depth -> depth

let private findProjectDependencies project = projectsDependencies |> List.find (fun pd -> pd.Project = project)

// Note: No attempt made to optimize this - nor check for cyclic dependencies.
let private dependencyPaths (projectDependencies:ProjectDependencies) =
    let direct depth (dependencies:Dependency Set) =
        dependencies
        |> List.ofSeq
        |> List.map (fun d ->
            match d with
            | PackageReference project -> { Project = project ; DependencyType = PackageDependency depth }
            | ProjectReference project -> { Project = project ; DependencyType = ProjectDependency depth })
    let rec traverse currentDepth (currentPaths:DependencyPath list) =
        currentPaths
        |> List.map (fun (DependencyPath currentPath) ->
            match currentPath with
            | [] -> [ DependencyPath currentPath ]
            | h :: _ ->
                let projectDependencies = findProjectDependencies h.Project
                let dependencies = projectDependencies.Dependencies
                if dependencies.IsEmpty then [ DependencyPath currentPath ]
                else
                    let currentDepth = currentDepth + 1
                    let directN = direct currentDepth dependencies
                    let newPaths = directN |> List.map (fun di -> DependencyPath (di :: currentPath))
                    newPaths |> traverse currentDepth)
        |> List.collect id
    let project = projectDependencies.Project
    let self = if project.Packaged then Some [ { Project = project ; DependencyType = Self } ] else None
    let direct1 = projectDependencies.Dependencies |> direct 1
    let paths = direct1 |> List.map (fun di -> DependencyPath [ di ]) |> traverse 1
    let dependencyPaths = match self with | Some self -> DependencyPath self :: paths | None -> paths
    { Project = project ; DependencyPaths = dependencyPaths |> List.filter (fun (DependencyPath dp) -> not dp.IsEmpty) }

let private projectsDependencyPaths = projectsDependencies |> List.map dependencyPaths

let aAnalysis = adaptive {
    let! packagedProjectStatusMap = cPackagedProjectStatusMap |> AMap.toAVal
    let hasCodeChanges project = match packagedProjectStatusMap.TryFind (key project) with | Some pps -> pps.HasCodeChanges | None -> false
    let affectedProjectDependencyPaths (projectDependencyPaths:ProjectDependencyPaths) =
        let affectedDependencyPath (DependencyPath dependencyPath) =
            match dependencyPath |> List.mapi (fun i di -> if hasCodeChanges di.Project then Some i else None) |> List.choose id with
            | [] -> None
            | affectedIndices ->
                let minAffectedIndex = affectedIndices |> List.min
                dependencyPath |> List.skip minAffectedIndex |> DependencyPath |> Some
        match projectDependencyPaths.DependencyPaths |> List.choose affectedDependencyPath with
        | [] -> None
        | affectedDependencyPaths ->
            (* TODO-NMB:
                -- Do we want to filter out "direct to project" dependencies?...
                    - can give weird results (e.g. for Common.Interfaces code changes, RepositoriesTests has maxDepth = 3 but Repositories has maxDepth = 3)...
                    - so maybe handle appropriately when displaying analysis?... *)
            (*let selfOrPackage =
                affectedDependencyPaths
                |> List.filter (fun (DependencyPath dp) ->
                    let selfOrDirect = dp |> List.last
                    match selfOrDirect.DependencyType with | ProjectDependency _ -> false | _ -> true)
            if selfOrPackage.Length = 0 then None
            else*)
                let uniqueSelfOrDirectWithMaxDepth =
                    affectedDependencyPaths
                    |> List.groupBy (fun (DependencyPath dp) ->
                        let selfOrDirect = dp |> List.last
                        selfOrDirect.Project)
                    |> List.map (fun (_, dependencyPaths) ->
                        dependencyPaths
                        |> List.map (fun (DependencyPath dp) ->
                            let deepest = dp |> List.head
                            DependencyPath dp, depth deepest.DependencyType)
                        |> List.maxBy snd)
                let maxDepth = uniqueSelfOrDirectWithMaxDepth |> List.map snd |> List.max
                Some ({ Project = projectDependencyPaths.Project ; DependencyPaths = uniqueSelfOrDirectWithMaxDepth |> List.map fst }, maxDepth)
    let packagedProjectStatuses = packagedProjectStatusMap |> List.ofSeq
    if packagedProjectStatuses |> List.map snd |> List.exists (fun pps -> pps.HasCodeChanges) then
        let affected = projectsDependencyPaths |> List.choose affectedProjectDependencyPaths
        transact (fun () -> cTabLatestDoneMap.[Development] <- None)
        transact (fun () -> cTabLatestDoneMap.[CommittingPushing] <- None)
        return! AVal.map3 (fun a b c -> Some(a, b, c)) (AVal.constant affected) cCurrentTab (cTabLatestDoneMap |> AMap.toAVal)
    else return! AVal.constant None }
