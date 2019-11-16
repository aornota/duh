module Aornota.Duh.Ui.App

open Aornota.Duh.Common.Adaptive
open Aornota.Duh.Common.Domain

open Browser.Dom

open Fable.React.Adaptive
module ReactHB = Fable.React.HookBindings

open Feliz
open Feliz.MaterialUI

open FSharp.Data.Adaptive

type private Tab = | Development | CommittingPushing

let [<Literal>] private DUH = "duh"

// α | *β* | γ | δ | ε | ζ | η | θ | ι | κ | λ | μ | ν | ξ | ο | π | ρ | σ | τ | υ | φ | χ | ψ | ω
let [<Literal>] private DUH_VERSION = "β" // note: keep synchronized with  ./index.html | ../../package.json | ../../README.md

let [<Literal>] private DUH_LOGO = "duh-24x24.png"
let [<Literal>] private VISUALIZATION_FILENAME = "visualization.png" // note: keep synchronized with ../visualizer-console/visualizer.fs and ../../build.fsx

let [<Literal>] private BULLET = "●"

let private projectColour = function
    | Blue -> color.blue | Coral -> color.coral | Cyan -> color.cyan | Goldenrod -> color.goldenRod | Grey -> color.gray | Pink -> color.pink | Salmon -> color.salmon
    | SeaGreen -> color.seaGreen | SkyBlue -> color.skyBlue | SlateBlue -> color.slateBlue | SlateGrey -> color.slateGray | SteelBlue -> color.steelBlue | Yellow -> color.yellow

let private cCurrentTab = cval Development

let private cShowingVisualization = cval false // remember to reset to false before committing

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

let private packageCheckbox (key, packagedProjectStatus:PackagedProjectStatus) =
    let project = packagedProjectStatus.Project
    let onClick = (fun _ -> transact (fun () -> cPackagedProjectStatuses.[key] <- { packagedProjectStatus with HasCodeChanges = not packagedProjectStatus.HasCodeChanges } ))
    Mui.formControlLabel [
        formControlLabel.label project.Name
        formControlLabel.control (
            Mui.checkbox [
                prop.style [ style.color (projectColour project.Solution.Colour) ]
                checkbox.checked' packagedProjectStatus.HasCodeChanges
                prop.onClick onClick ] ) ]

let private solutionCodeChanges (solution:Solution, packagedProjectStatuses:seq<string * PackagedProjectStatus>) =
    let sorted = packagedProjectStatuses |> List.ofSeq |> List.sortBy (fun (_, pps) -> pps.Project.Name)
    Mui.grid [
        grid.item true
        grid.children [
            Mui.typography [
                typography.color.primary
                typography.children [
                   Html.strong solution.Name
                   Html.text (sprintf " solution (%s)" (repoText solution.Repo)) ] ]
            Mui.formGroup [
                formGroup.row false
                formGroup.children [
                    yield! sorted |> List.map packageCheckbox ] ] ] ]

let private codeChanges packagedProjectStatuses =
    let solution (packagedProjectStatus:PackagedProjectStatus) = packagedProjectStatus.Project.Solution
    let groupedAndSorted = packagedProjectStatuses |> List.ofSeq |> List.groupBy (snd >> solution) |> List.sortBy (fun (solution, _) -> solutionSortOrder solution, solution.Name)
    Html.div [
        Mui.typography [
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

let private analysis (currentTab:Tab) = // TODO-NMB...
    let tabValue = function | Development -> 0 | CommittingPushing -> 1 // seemingly needs to be zero-based
    let onClick tab = (fun _ -> transact (fun () -> cCurrentTab.Value <- tab ))
    let muiTab tab' =
        let label = match tab' with | Development -> "Development" | CommittingPushing -> "Committing / pushing"
        let iconClassName = match tab' with | Development -> "far fa-file-code" | CommittingPushing -> "fas fa-code-branch"
        Mui.tab [
            tab.label label
            tab.icon (
                Mui.icon [
                    icon.classes [ classes.icon.root iconClassName ]
                    if tab' = currentTab then icon.color.primary ] )
            tab.value (tabValue tab')
            prop.onClick (onClick tab') ]
    Html.div [
        Mui.typography [
            typography.color.secondary
            typography.paragraph true
            typography.children [
                Html.strong "TODO-NMB..."
                Html.text "Optimal order of package reference updates:" ] ]
        Mui.tabs [
            prop.style [ style.paddingLeft 20 ; style.paddingBottom 20 ]
            tabs.indicatorColor.primary
            tabs.value (tabValue currentTab)
            tabs.children [
                muiTab Development
                muiTab CommittingPushing ] ]
        match currentTab with
        | Development ->
            Html.div [
                prop.style [ style.paddingLeft 20 ; style.paddingBottom 20 ]
                prop.children [
                    Mui.typography [
                        typography.paragraph true
                        typography.children [
                            Html.strong "TODO-NMB..."
                            Html.text "Development instructions" ] ] ] ]
        | CommittingPushing ->
            Html.div [
                prop.style [ style.paddingLeft 20 ; style.paddingBottom 20 ]
                prop.children [
                    Mui.typography [
                        typography.paragraph true
                        typography.children [
                            Html.strong "TODO-NMB..."
                            Html.text "Committing / pushing instructions" ] ] ] ] ]

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
                            prop.onClick (fun _ -> transact (fun () -> cShowingVisualization.Value <- not cShowingVisualization.Value ) ) ] ) ] ] ]
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
                        typography.children [
                            Html.text (sprintf "%s projects within the same solution share the same colour (with a darker shade used for " BULLET)
                            Html.strong "packaged"
                            Html.text " projects)" ] ]
                    Mui.typography [
                        typography.children [
                            Html.text (sprintf "%s solid lines indicate project-to-" BULLET)
                            Html.strong "package"
                            Html.text " references" ] ]
                    Mui.typography [
                        typography.children [
                            Html.text (sprintf "%s dotted lines indicate project-to-project references" BULLET) ] ] ] ] ]

let private app =
    React.functionComponent (fun () ->
        let packagedProjectStatuses = ReactHB.Hooks.useAdaptive cPackagedProjectStatuses
        let currentTab = ReactHB.Hooks.useAdaptive cCurrentTab
        let showingVisualization = ReactHB.Hooks.useAdaptive cShowingVisualization
        Html.div [
            preamble
            codeChanges packagedProjectStatuses
            if packagedProjectStatuses |> List.ofSeq |> List.map snd |> List.exists (fun pps -> pps.HasCodeChanges) then
                analysis currentTab
            Mui.divider []
            visualization showingVisualization ] )

ReactDOM.render (app, document.getElementById "app") // needs to match id of div in index.html
