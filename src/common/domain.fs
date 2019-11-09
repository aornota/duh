module Aornota.Duh.Common.Domain

type Colour = | Blue | Coral | Cyan | Goldenrod | GoldenrodYellow | Grey | Pink | Salmon | SeaGreen | SkyBlue | SlateBlue | SlateGrey | SteelBlue | Yellow

type Repo = | AzureDevOps | Subversion

type Solution = { Name : string ; Repo : Repo ; Colour : Colour }

type Project = { Name : string ; Solution : Solution }

type Package = | Package of Project

type ProjectOrPackage = | Proj of Project | Pack of Package

type ProjectDependencies = { ProjectOrPackage : ProjectOrPackage ; PackageReferences : Package Set }

let colourText (colour:Colour) = colour.ToString().ToLower()
let colourLightText colour = sprintf "light%s" (colourText colour)

let repoText = function | AzureDevOps -> "Azure DevOps" | Subversion -> "Subversion"

let solutionText (solution:Solution) = sprintf "%s.sln" solution.Name
let solutionAndRepoText solution = sprintf "%s (%s)" (solutionText solution) (repoText solution.Repo)

let projectText (project:Project) = sprintf "%s.csproj" project.Name
let projectAndSolutionText project = sprintf "%s (%s)" (projectText project) (solutionText project.Solution)

let packageText (Package project) = sprintf "%s (package)" project.Name

let projectOrPackageName = function | Proj project | Pack (Package project) -> project.Name
let projectOrPackageText = function | Proj project -> projectText project | Pack package -> packageText package
let projectOrPackageColour = function | Proj project -> colourLightText project.Solution.Colour | Pack (Package project) -> colourText project.Solution.Colour
