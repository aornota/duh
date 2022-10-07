module Aornota.Duh.Common.ChangeableValues

open Aornota.Duh.Common.Domain
open Aornota.Duh.Common.DomainData

open FSharp.Data.Adaptive

type PackagedProjectStatus = { ProjectName:string ; SortOrder:int option ; TestsProjectName:string option ; HasCodeChanges:bool }

type AnalysisTab = | Development | CommittingPushing

let [<Literal>] private SHOW_VISUALIZATION__DEFAULT = false

let private packagedProjectStatuses =
    projectsDependencies
    |> List.choose (fun pd ->
        match projectMap.[pd.ProjectName].ProjectType with
        | Some (Packaged (sortOrder, testsProjectName)) ->
            Some (pd.ProjectName, { ProjectName = pd.ProjectName ; SortOrder = sortOrder ; TestsProjectName = testsProjectName ; HasCodeChanges = false })
        | _ -> None)

let cPackagedProjectStatusMap = packagedProjectStatuses |> cmap

let cCurrentTab = cval Development
let cTabLatestDoneMap : ChangeableHashMap<AnalysisTab, int option> = [ (Development, None) ; (CommittingPushing, None) ] |> cmap

let cShowingVisualization = cval SHOW_VISUALIZATION__DEFAULT

let analysisTabs = [ Development ; CommittingPushing ]
