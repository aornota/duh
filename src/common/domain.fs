module Aornota.Duh.Common.Domain

open System

type Colour = | Gold | LightSkyBlue | Plum | SandyBrown | Tomato | YellowGreen

type Repo = | AzureDevOps | Subversion

type Solution = { Name : string ; Repo : Repo ; Colour : Colour ; SortOrder : int option }
type SolutionMap = Map<string, Solution>

type Project = { Name : string ; SolutionName : string ; Packaged : bool }
type ProjectMap = Map<string, Project>

type Dependency = | PackageReference of projectName : string | ProjectReference of projectName : string

type ProjectDependencies = { ProjectName : string ; Dependencies : Dependency Set }

let solutionSortOrder solution = match solution.SortOrder with | Some ordinal -> ordinal | None -> Int32.MaxValue

let dependencyProjectName = function | PackageReference projectName | ProjectReference projectName -> projectName
