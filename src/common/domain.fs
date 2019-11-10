module Aornota.Duh.Common.Domain

type Colour = | Blue | Coral | Cyan | Goldenrod | GoldenrodYellow | Grey | Pink | Salmon | SeaGreen | SkyBlue | SlateBlue | SlateGrey | SteelBlue | Yellow

type Repo = | AzureDevOps | Subversion

type Solution = { Name : string ; Repo : Repo ; RootPath : string ; Colour : Colour }

type Project = { Name : string ; Solution : Solution ; ExtraPath : string option }

type Package = | Package of Project

type ProjectOrPackage = | Proj of Project | Pack of Package

type ProjectDependencies = { ProjectOrPackage : ProjectOrPackage ; PackageReferences : Package Set }

let colourText (colour:Colour) = colour.ToString().ToLower()
let colourLightText colour = sprintf "light%s" (colourText colour)

let repoText = function | AzureDevOps -> "Azure DevOps" | Subversion -> "Subversion"

let solutionFileText (solution:Solution) = sprintf "%s.sln" solution.Name
let solutionFolderText (solution:Solution) = sprintf "/%s/%s" solution.RootPath solution.Name
let solutionPathText solution = sprintf "%s/%s" (solutionFolderText solution) (solutionFileText solution)
let solutionFileAndRepoText solution = sprintf "%s (%s)" (solutionFileText solution) (repoText solution.Repo)
let solutionPathAndRepoText solution = sprintf "%s (%s)" (solutionPathText solution) (repoText solution.Repo)

let projectFileText (project:Project) = sprintf "%s.csproj" project.Name
let projectAndSolutionFileText project = sprintf "%s (%s)" (projectFileText project) (solutionFileText project.Solution)
let projectAndSolutionFolderText (project:Project) =
    match project.ExtraPath with
    | Some extraPath -> sprintf "%s/%s/%s" (solutionFolderText project.Solution) extraPath project.Name
    | None -> sprintf "%s/%s" (solutionFolderText project.Solution) project.Name
let projectAndSolutionPathText project = sprintf "%s/%s" (projectAndSolutionFolderText project) (projectFileText project)

let packageText (Package project) = sprintf "%s (package)" project.Name

let projectOrPackageName = function | Proj project | Pack (Package project) -> project.Name
let projectOrPackageText = function | Proj project -> projectFileText project | Pack package -> packageText package
let projectOrPackagePathText = function | Proj project | Pack (Package project) -> projectAndSolutionPathText project
let projectOrPackageColour = function | Proj project -> colourLightText project.Solution.Colour | Pack (Package project) -> colourText project.Solution.Colour
