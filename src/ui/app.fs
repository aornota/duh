module Aornota.Duh.Ui.App

open Aornota.Duh.Common.AdaptiveValues
open Aornota.Duh.Common.ChangeableValues
open Aornota.Duh.Common.DependencyPaths
open Aornota.Duh.Common.Domain
open Aornota.Duh.Common.DomainData

open Browser.Dom

open Fable.React.Adaptive
module ReactHB = Fable.React.HookBindings

open Feliz
open Feliz.MaterialUI

open FSharp.Data.Adaptive

let [<Literal>] private DUH = "duh"

// α | β | *γ* | δ | ε | ζ | η | θ | ι | κ | λ | μ | ν | ξ | ο | π | ρ | σ | τ | υ | φ | χ | ψ | ω
let [<Literal>] private DUH_VERSION = "γ" // note: keep synchronized with  ./index.html | ../../package.json | ../../README.md

let [<Literal>] private DUH_LOGO = "duh-24x24.png"
let [<Literal>] private VISUALIZATION_FILENAME = "visualization.svg" // note: keep synchronized with ../visualizer-console/visualizer.fs and ../../build.fsx

let [<Literal>] private BULLET = "●"

let private solution projectName = solutionMap.[projectMap.[projectName].SolutionName]

let private projectColour = function
    | Blue -> color.blue | Coral -> color.coral | Cyan -> color.cyan | Goldenrod -> color.goldenRod | Grey -> color.gray | Pink -> color.pink | Salmon -> color.salmon
    | SeaGreen -> color.seaGreen | SkyBlue -> color.skyBlue | SlateBlue -> color.slateBlue | SlateGrey -> color.slateGray | SteelBlue -> color.steelBlue | Yellow -> color.yellow

let private preamble =
    Html.div [
        Mui.typography [
            typography.variant.h4
            typography.paragraph true
            typography.children [
                Html.img [
                    prop.style [ style.verticalAlign.middle ]
                    prop.src DUH_LOGO
                    prop.alt DUH ]
                Html.text " | "
                Html.strong DUH
                Html.text (sprintf " (%s)" DUH_VERSION) ] ]
        Mui.typography [
            typography.paragraph true
            typography.children [
                Html.strong DUH
                Html.text " (dependency update helper) is a tool to work out the optimal order of package reference updates — "
                Html.text "both during development and when committing / pushing changes." ] ] ]

let private packageCheckbox (packagedProjectStatus:PackagedProjectStatus) =
    let projectName = packagedProjectStatus.ProjectName
    let onClick = (fun _ -> transact (fun () -> cPackagedProjectStatusMap.[projectName] <- { packagedProjectStatus with HasCodeChanges = not packagedProjectStatus.HasCodeChanges }))
    Mui.formControlLabel [
        formControlLabel.label projectName
        formControlLabel.control (
            Mui.checkbox [
                prop.style [ style.color (projectColour (solution projectName).Colour) ]
                checkbox.checked' packagedProjectStatus.HasCodeChanges
                prop.onClick onClick ]) ]

let private solutionCodeChanges (solution:Solution, packagedProjectStatuses:(string * PackagedProjectStatus) list) =
    let sorted =
        packagedProjectStatuses
        |> List.ofSeq
        |> List.map snd
        |> List.sortBy (fun pps -> pps.ProjectName)
    Mui.grid [
        grid.item true
        grid.children [
            Mui.typography [
                typography.paragraph false
                typography.color.primary
                typography.children [
                   Html.strong solution.Name
                   Html.text (sprintf " solution (%s)" (repoText solution.Repo)) ] ]
            Mui.formGroup [
                formGroup.row false
                formGroup.children [
                    yield! sorted |> List.map packageCheckbox ] ] ] ]

let private codeChanges (packagedProjectStatusMap:HashMap<string, PackagedProjectStatus>) =
    let groupedAndSorted =
        packagedProjectStatusMap
        |> List.ofSeq
        |> List.groupBy (fun (_, pps) -> solution pps.ProjectName)
        |> List.sortBy (fun (solution, _) -> solutionSortOrder solution, solution.Name)
    Html.div [
        Mui.typography [
            typography.paragraph false
            typography.color.secondary
            typography.children [
                Html.text "Please select the "
                Html.strong "packaged"
                Html.text " projects for which code changes have been made:" ] ]
        Mui.grid [
            prop.style [ style.paddingTop 20 ; style.paddingLeft 20 ; style.paddingBottom 20 ]
            grid.container true
            grid.spacing._4
            grid.children [
                yield! groupedAndSorted |> List.map solutionCodeChanges ] ] ]

let private analysis (affected:(ProjectDependencyPaths * int) list) currentTab latestDone =
    let latestDone = latestDone |> Option.defaultValue 0
    (* TODO-NMB:
        -- Tab-specific notes?...
        -- Various UI improvements, e.g.:
            - better formatting... *)
    let step (ordinal, projectsDependencyPaths:ProjectDependencyPaths list) = [
        let isDone = ordinal <= latestDone
        let isNext = ordinal = latestDone + 1
        let stepProject (projectDependencyPaths:ProjectDependencyPaths) =
            let projectName = projectDependencyPaths.ProjectName
            Mui.typography [
                typography.paragraph false
                if isDone then typography.color.textSecondary
                typography.children [
                    Html.text (sprintf "%s (%s)" projectName (solution projectName).Name) ] ]
        Mui.typography [
            typography.variant.h6
            typography.paragraph false
            if isDone then typography.color.textSecondary
            else if isNext then typography.color.primary
            typography.children [
                Html.strong (sprintf "Step %i" ordinal) ] ]
        yield! projectsDependencyPaths |> List.map stepProject
        if isNext then
            Mui.button [
                button.variant.text
                button.color.primary
                prop.onClick (fun _ -> transact (fun () -> cTabLatestDoneMap.[currentTab] <- Some ordinal ))
                button.children [ Html.text "Done" ] ] ]
    let groupedAndSorted =
        affected
        |> List.groupBy snd
        |> List.sortBy fst
        |> List.map (fun (maxDepth, paths) ->
            let sorted =
                paths
                |> List.map fst
                |> List.sortBy (fun pdp -> solutionSortOrder (solution pdp.ProjectName), pdp.ProjectName)
            maxDepth + 1, sorted)
    let current = match currentTab with | Development -> "development" | CommittingPushing -> "committing / pushing"
    Html.div [
        prop.style [ style.paddingLeft 20 ; style.paddingBottom 20 ]
        prop.children [
            Mui.typography [
                typography.paragraph true
                typography.children [
                    Html.strong "TODO-NMB: "
                    Html.text (sprintf "Specific notes about these %s instructions - plus various UI improvements..." current) ] ]
            yield! groupedAndSorted |> List.map step |> List.collect id ] ]

let private analysisTabs affected currentTab latestDone =
    let tabValue = function | Development -> 0 | CommittingPushing -> 1 // seemingly needs to be zero-based
    let onClick analysisTab = (fun _ -> transact (fun () -> cCurrentTab.Value <- analysisTab))
    let muiTab analysisTab =
        let label = match analysisTab with | Development -> "Development" | CommittingPushing -> "Committing / pushing"
        let iconClassName = match analysisTab with | Development -> "far fa-file-code" | CommittingPushing -> "fas fa-code-branch"
        Mui.tab [
            tab.label label
            tab.icon (
                Mui.icon [
                    icon.classes [ classes.icon.root iconClassName ]
                    if analysisTab = currentTab then icon.color.primary ])
            tab.value (tabValue analysisTab)
            prop.onClick (onClick analysisTab) ]
    Html.div [
        Mui.typography [
            typography.color.secondary
            typography.paragraph true
            typography.children [
                Html.text "Optimal order of package reference updates:" ] ]
        Mui.tabs [
            prop.style [ style.paddingLeft 20 ; style.paddingBottom 20 ]
            tabs.indicatorColor.primary
            tabs.value (tabValue currentTab)
            tabs.children [
                muiTab Development
                muiTab CommittingPushing ] ]
        analysis affected currentTab latestDone ]

let private visualization showingVisualization =
    Html.div [
        Mui.formGroup [
            formGroup.children [
                Mui.formControlLabel [
                    formControlLabel.label "Display visualization of project/package dependencies"
                    formControlLabel.control (
                        Mui.checkbox [
                            checkbox.color.primary
                            checkbox.checked' showingVisualization
                            prop.onClick (fun _ -> transact (fun () -> cShowingVisualization.Value <- not cShowingVisualization.Value)) ]) ] ] ]
        if showingVisualization then
            Html.div [
                prop.style [ style.paddingLeft 20 ]
                prop.children [
                    Html.img [
                        prop.style [
                            style.paddingTop 20
                            style.paddingBottom 20 ]
                        prop.src VISUALIZATION_FILENAME
                        prop.alt "Visualization of project/package dependencies" ]
                    Mui.typography [
                        typography.paragraph false
                        typography.children [
                            Html.text (sprintf "%s projects within the same solution share the same colour (with a darker shade used for " BULLET)
                            Html.strong "packaged"
                            Html.text " projects)" ] ]
                    Mui.typography [
                        typography.paragraph false
                        typography.children [
                            Html.text (sprintf "%s solid lines indicate project-to-" BULLET)
                            Html.strong "package"
                            Html.text " references" ] ]
                    Mui.typography [
                        typography.paragraph false
                        typography.children [
                            Html.text (sprintf "%s dotted lines indicate project-to-project references" BULLET) ] ] ] ] ]

let private app =
    React.functionComponent (fun () ->
        let packagedProjectStatusMap = ReactHB.Hooks.useAdaptive cPackagedProjectStatusMap
        let analysis = ReactHB.Hooks.useAdaptive aAnalysis
        let showingVisualization = ReactHB.Hooks.useAdaptive cShowingVisualization
        Html.div [
            preamble
            codeChanges packagedProjectStatusMap
            match analysis with
            | Some (affected, currentTab, tabLatestDoneMap) ->
                analysisTabs affected currentTab tabLatestDoneMap.[currentTab]
            | None -> ()
            Mui.divider []
            visualization showingVisualization ])

ReactDOM.render (app, document.getElementById "app") // needs to match id of div in index.html
