module Aornota.Duh.Common.ChangeableValues

open Aornota.Duh.Common.Domain
open Aornota.Duh.Common.DomainData

open FSharp.Data.Adaptive

type PackagedProjectStatus = { ProjectName : string ; HasCodeChanges : bool }

type AnalysisTab = | Development | CommittingPushing

let [<Literal>] private SHOW_VISUALIZATION__DEFAULT = false

let private packagedProjectStatuses =
    projectsDependencies
    |> List.filter (fun pd -> projectMap.[pd.ProjectName].Packaged)
    |> List.map (fun pd -> pd.ProjectName, { ProjectName = pd.ProjectName ; HasCodeChanges = false })

let cPackagedProjectStatusMap = packagedProjectStatuses |> cmap

let cCurrentTab = cval Development
let cTabLatestDoneMap : ChangeableMap<AnalysisTab, int option> = [ (Development, None) ; (CommittingPushing, None) ] |> cmap

let cShowingVisualization = cval SHOW_VISUALIZATION__DEFAULT

let analysisTabs = [ Development ; CommittingPushing ]
