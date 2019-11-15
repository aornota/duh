module Aornota.Duh.Common.Domain

open System

type Colour = | Blue | Coral | Cyan | Goldenrod | Grey | Pink | Salmon | SeaGreen | SkyBlue | SlateBlue | SlateGrey | SteelBlue | Yellow

type Repo = | AzureDevOps | Subversion

type Solution = { Name : string ; Repo : Repo ; RootPath : string ; Colour : Colour ; SortOrder : int option }

type Project = { Name : string ; Solution : Solution ; ExtraPath : string option ; Packaged : bool }

type Dependency = | PackageReference of Project | ProjectReference of Project

type ProjectDependencies = { Project : Project ; Dependencies : Dependency Set }

let colourText (colour:Colour) = colour.ToString().ToLower()
let colourLightText colour = sprintf "light%s" (colourText colour)

let repoText = function | AzureDevOps -> "Azure DevOps" | Subversion -> "Subversion"

let solutionFileText (solution:Solution) = sprintf "%s.sln" solution.Name
let solutionAndRepoText (solution:Solution) = sprintf "%s (%s)" (solution.Name) (repoText solution.Repo)
let solutionFolderText solution = sprintf "/%s/%s" solution.RootPath solution.Name
let solutionPathText solution = sprintf "%s/%s" (solutionFolderText solution) (solutionFileText solution)
let solutionFileAndRepoText solution = sprintf "%s (%s)" (solutionFileText solution) (repoText solution.Repo)
let solutionPathAndRepoText solution = sprintf "%s (%s)" (solutionPathText solution) (repoText solution.Repo)
let solutionSortOrder solution = match solution.SortOrder with | Some ordinal -> ordinal | None -> Int32.MaxValue

let projectFileText (project:Project) = sprintf "%s.csproj" project.Name
let projectAndSolutionFileText project = sprintf "%s (%s)" (projectFileText project) (solutionFileText project.Solution)
let projectAndSolutionFolderText project =
    match project.ExtraPath with
    | Some extraPath -> sprintf "%s/%s/%s" (solutionFolderText project.Solution) extraPath project.Name
    | None -> sprintf "%s/%s" (solutionFolderText project.Solution) project.Name
let projectAndSolutionPathText project = sprintf "%s/%s" (projectAndSolutionFolderText project) (projectFileText project)
let projectColour project = if project.Packaged then colourText project.Solution.Colour else colourLightText project.Solution.Colour

let isPackageReference = function | PackageReference _ -> true | ProjectReference _ -> false
let dependencyName = function | PackageReference project | ProjectReference project -> project.Name
let dependencyIsPackaged = function | PackageReference project | ProjectReference project -> project.Packaged
