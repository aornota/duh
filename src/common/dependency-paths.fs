module Aornota.Duh.Common.DependencyPaths

open Aornota.Duh.Common.Domain
open Aornota.Duh.Common.DomainData
open Aornota.Duh.Common.Utility

type DependencyType = | PackageDependency of depth:int | ProjectDependency of depth:int | Self

type DependencyInfo = { ProjectName:string ; DependencyType:DependencyType }

type DependencyPath = DependencyInfo list

type ProjectDependencyPaths = { ProjectName:string ; DependencyPaths:DependencyPath list }

let private findProjectDependencies projectName = projectsDependencies |> List.find (fun pd -> pd.ProjectName = projectName)

(* Notes:
    - No attempt made to optimize this (e.g. by memoizing paths).
    - However, it will fail with an informative exception (rather than a "stack overflow") if cyclic dependencies - or self-references - are detected. *)
let private dependencyPaths (projectDependencies:ProjectDependencies) =
    let direct currentDependencyInfo (dependencies:Dependency Set) =
        let nextDepth =
            match currentDependencyInfo with
            | Some di ->
                match di.DependencyType with
                | PackageDependency depth -> depth + 1
                | ProjectDependency depth -> depth + 1
                | Self -> failwithf "direct called with invalid (Self) current DependencyInfo: %A" currentDependencyInfo
            | None -> 1
        dependencies
        |> List.ofSeq
        |> List.map (fun d ->
            match d with
            | PackageReference projectName -> { ProjectName = projectName ; DependencyType = PackageDependency nextDepth }
            | ProjectReference projectName -> { ProjectName = projectName ; DependencyType = ProjectDependency (nextDepth - 1) })
    let rec traverse currentDepth (currentPaths:DependencyPath list) =
        currentPaths
        |> List.map (fun currentPath ->
            match currentPath with
            | [] -> [ currentPath ]
            | di :: _ ->
                let projectDependencies = findProjectDependencies di.ProjectName
                let dependencies = projectDependencies.Dependencies
                if dependencies.IsEmpty then [ currentPath ]
                else
                    let currentDepth = currentDepth + 1
                    let directN = dependencies |> direct (Some di)
                    let cycles =
                        directN
                        |> List.choose (fun di ->
                            match currentPath |> List.rev |> List.map (fun diOther -> diOther.ProjectName) |> List.skipWhile (fun projectName -> projectName <> di.ProjectName) with
                            | [] -> None
                            | cyclicPath ->
                                let cyclicPath = (di.ProjectName :: (cyclicPath |> List.rev)) |> List.rev
                                Some (cyclicPath |> concatenate " -> "))
                    if cycles.Length > 0 then failwithf "One or more cyclic dependencies detected: %s" (cycles |> concatenatePipe)
                    let newPaths = directN |> List.map (fun di -> di :: currentPath)
                    newPaths |> traverse currentDepth)
        |> List.collect id
    let project = projectMap.[projectDependencies.ProjectName]
    let self = match project.ProjectType with | Some (Packaged _) -> Some [ { ProjectName = project.Name ; DependencyType = Self } ] | _ -> None
    let direct1 = projectDependencies.Dependencies |> direct None
    if direct1 |> List.exists (fun di -> di.ProjectName = project.Name) then failwithf "%s has one or more self-references" project.Name
    let paths = direct1 |> List.map (fun di -> [ di ]) |> traverse 1
    let dependencyPaths = match self with | Some self -> self :: paths | None -> paths
    { ProjectName = project.Name ; DependencyPaths = dependencyPaths |> List.filter (fun dp -> not dp.IsEmpty) }

let isPackageDependency = function | PackageDependency _ -> true | _ -> false

let projectsDependencyPaths = lazy (projectsDependencies |> List.map dependencyPaths)
