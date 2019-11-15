module Aornota.Duh.Common.Adaptive

open Aornota.Duh.Common.Domain
open Aornota.Duh.Common.ProjectDependencyData

open FSharp.Data.Adaptive

type PackagedProjectStatus = { Project : Project ; HasCodeChanges : bool }

let cPackagedProjectStatuses =
    projectsDependencies
    |> List.filter (fun pd -> pd.Project.Packaged )
    |> List.map (fun pd -> pd.Project.Name, { Project = pd.Project ; HasCodeChanges = false } )
    |> cmap
