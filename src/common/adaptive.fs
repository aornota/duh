module Aornota.Duh.Common.Adaptive

open Aornota.Duh.Common.Domain
open Aornota.Duh.Common.ProjectDependencyData

open FSharp.Data.Adaptive

type PackagedProjectStatus = { Project : Project ; HasCodeChanges : bool }

type DependencyUpdated = | Self of Project | Project of Project * depth : int

let packagedProjectStatuses =
    projectsDependencies
    |> List.filter (fun pd -> pd.Project.Packaged )
    |> List.map (fun pd -> pd.Project.Name, { Project = pd.Project ; HasCodeChanges = false } )

let cPackagedProjectStatuses = packagedProjectStatuses |> cmap
