module Aornota.Duh.Common.Domain

open System

type Colour = | Gold | LightSkyBlue | Plum | SandyBrown | Tomato | YellowGreen

type Repo = | AzureDevOps | Subversion

type Solution = { Name:string ; Repo:Repo ; Colour:Colour ; SortOrder:int option }
type SolutionMap = Map<string, Solution>

type ProjectType = | Packaged of sortOrder:int option * testsProjectName:string option | Tests

type Project = { Name:string ; SolutionName:string ; ProjectType:ProjectType option }
type ProjectMap = Map<string, Project>

type Dependency = | PackageReference of projectName:string | ProjectReference of projectName:string

type ProjectDependencies = { ProjectName:string ; Dependencies:Dependency Set }

let sortOrder sortOrder = match sortOrder with | Some ordinal -> ordinal | None -> Int32.MaxValue
let solutionSortOrder (solution:Solution) = sortOrder solution.SortOrder

let dependencyProjectName = function | PackageReference projectName | ProjectReference projectName -> projectName
