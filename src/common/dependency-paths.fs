module Aornota.Duh.Common.DependencyPaths

open Aornota.Duh.Common.Domain
open Aornota.Duh.Common.DomainData

type DependencyType = | Self | PackageDependency of int | ProjectDependency of int

type DependencyInfo = { ProjectName : string ; DependencyType : DependencyType }

type DependencyPath = | DependencyPath of DependencyInfo list

type ProjectDependencyPaths = { ProjectName : string ; DependencyPaths : DependencyPath list }

let private findProjectDependencies projectName = projectsDependencies |> List.find (fun pd -> pd.ProjectName = projectName)

// Note: No attempt made to optimize this - nor to check for cyclic dependencies.
let private dependencyPaths (projectDependencies:ProjectDependencies) =
    let direct depth (dependencies:Dependency Set) =
        dependencies
        |> List.ofSeq
        |> List.map (fun d ->
            match d with
            | PackageReference projectName -> { ProjectName = projectName ; DependencyType = PackageDependency depth }
            | ProjectReference projectName -> { ProjectName = projectName ; DependencyType = ProjectDependency depth })
    let rec traverse currentDepth (currentPaths:DependencyPath list) =
        currentPaths
        |> List.map (fun (DependencyPath currentPath) ->
            match currentPath with
            | [] -> [ DependencyPath currentPath ]
            | h :: _ ->
                let projectDependencies = findProjectDependencies h.ProjectName
                let dependencies = projectDependencies.Dependencies
                if dependencies.IsEmpty then [ DependencyPath currentPath ]
                else
                    let currentDepth = currentDepth + 1
                    let directN = direct currentDepth dependencies
                    let newPaths = directN |> List.map (fun di -> DependencyPath (di :: currentPath))
                    newPaths |> traverse currentDepth)
        |> List.collect id
    let project = projectMap.[projectDependencies.ProjectName]
    let self = if project.Packaged then Some [ { ProjectName = project.Name ; DependencyType = Self } ] else None
    let direct1 = projectDependencies.Dependencies |> direct 1
    let paths = direct1 |> List.map (fun di -> DependencyPath [ di ]) |> traverse 1
    let dependencyPaths = match self with | Some self -> DependencyPath self :: paths | None -> paths
    { ProjectName = project.Name ; DependencyPaths = dependencyPaths |> List.filter (fun (DependencyPath dp) -> not dp.IsEmpty) }

let projectsDependencyPaths = projectsDependencies |> List.map dependencyPaths
