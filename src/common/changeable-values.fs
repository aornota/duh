module Aornota.Duh.Common.ChangeableValues

open Aornota.Duh.Common.Domain
open Aornota.Duh.Common.ProjectDependencyData

open FSharp.Data.Adaptive

type PackagedProjectStatus = { Project : Project ; HasCodeChanges : bool }

type AnalysisTab = | Development | CommittingPushing

let [<Literal>] private SHOW_VISUALIZATION__DEFAULT = false

let key (project:Project) = project.Name

let private packagedProjectStatuses =
    projectsDependencies
    |> List.filter (fun pd -> pd.Project.Packaged)
    |> List.map (fun pd -> key pd.Project, { Project = pd.Project ; HasCodeChanges = false })

let cPackagedProjectStatusMap = packagedProjectStatuses |> cmap

let cCurrentTab = cval Development
let cTabLatestDoneMap : ChangeableMap<AnalysisTab, int option> = [ (Development, None) ; (CommittingPushing, None) ] |> cmap

let cShowingVisualization = cval SHOW_VISUALIZATION__DEFAULT
