module Aornota.Duh.Common.Domain

open System

type Colour = | Gold | LightSkyBlue | Plum | SandyBrown | Tomato | YellowGreen

type Repo = | AzureDevOps | Subversion

type Solution = { Name : string ; Repo : Repo ; RootPath : string ; Colour : Colour ; SortOrder : int option }
type SolutionMap = Map<string, Solution>

type Project = { Name : string ; SolutionName : string ; ExtraPath : string option ; Packaged : bool }
type ProjectMap = Map<string, Project>

type Dependency = | PackageReference of projectName : string | ProjectReference of projectName : string

type ProjectDependencies = { ProjectName : string ; Dependencies : Dependency Set }

let repoText = function | AzureDevOps -> "Azure DevOps" | Subversion -> "Subversion"

let solutionFileText (solution:Solution) = sprintf "%s.sln" solution.Name
let solutionAndRepoText (solution:Solution) = sprintf "%s (%s)" (solution.Name) (repoText solution.Repo)
let solutionFolderText solution = sprintf "/%s/%s" solution.RootPath solution.Name
let solutionPathText solution = sprintf "%s/%s" (solutionFolderText solution) (solutionFileText solution)
let solutionFileAndRepoText solution = sprintf "%s (%s)" (solutionFileText solution) (repoText solution.Repo)
let solutionPathAndRepoText solution = sprintf "%s (%s)" (solutionPathText solution) (repoText solution.Repo)
let solutionSortOrder solution = match solution.SortOrder with | Some ordinal -> ordinal | None -> Int32.MaxValue

let projectFileText (project:Project) = sprintf "%s.csproj" project.Name
let projectAndSolutionFileText (solutionMap:SolutionMap) project = sprintf "%s (%s)" (projectFileText project) (solutionFileText solutionMap.[project.SolutionName])
let projectAndSolutionFolderText (solutionMap:SolutionMap) project =
    match project.ExtraPath with
    | Some extraPath -> sprintf "%s/%s/%s" (solutionFolderText solutionMap.[project.SolutionName]) extraPath project.Name
    | None -> sprintf "%s/%s" (solutionFolderText solutionMap.[project.SolutionName]) project.Name
let projectAndSolutionPathText (solutionMap:SolutionMap) project = sprintf "%s/%s" (projectAndSolutionFolderText solutionMap project) (projectFileText project)

let isPackageReference = function | PackageReference _ -> true | ProjectReference _ -> false
let dependencyProjectName = function | PackageReference projectName | ProjectReference projectName -> projectName
let dependencyProject (projectMap:ProjectMap) dependency = projectMap.[dependencyProjectName dependency]
let dependencyIsPackaged (projectMap:ProjectMap) dependency = (dependencyProject projectMap dependency).Packaged
