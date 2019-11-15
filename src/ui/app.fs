module Aornota.Duh.Ui.App

open Aornota.Duh.Common.Adaptive
open Aornota.Duh.Common.Domain

open Browser.Dom

open Fable.React.Adaptive
module ReactHB = Fable.React.HookBindings

open Feliz
open Feliz.MaterialUI

open FSharp.Data.Adaptive

let [<Literal>] private DUH = "duh"

// α | *β* | γ | δ | ε | ζ | η | θ | ι | κ | λ | μ | ν | ξ | ο | π | ρ | σ | τ | υ | φ | χ | ψ | ω
let [<Literal>] private DUH_VERSION = "β" // note: keep synchronized with  ./index.html | ../../package.json | ../../README.md

let [<Literal>] private DUH_LOGO = "duh-24x24.png"
let [<Literal>] private VISUALIZATION_FILENAME = "visualization.png" // note: keep synchronized with ../visualizer-console/visualizer.fs and ../../build.fsx

let [<Literal>] private BULLET = "●"

let private projectColour = function
    | Blue -> color.blue | Coral -> color.coral | Cyan -> color.cyan | Goldenrod -> color.goldenRod | Grey -> color.gray | Pink -> color.pink | Salmon -> color.salmon
    | SeaGreen -> color.seaGreen | SkyBlue -> color.skyBlue | SlateBlue -> color.slateBlue | SlateGrey -> color.slateGray | SteelBlue -> color.steelBlue | Yellow -> color.yellow

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
                Html.text "both during development and when committing/pushing changes." ] ] ]

let private packageCheckbox (key, packagedProjectStatus:PackagedProjectStatus) =
    let project = packagedProjectStatus.Project
    let onClick = (fun _ -> transact (fun () -> cPackagedProjectStatuses.[key] <- { packagedProjectStatus with HasCodeChanges = not packagedProjectStatus.HasCodeChanges } ) )
    Mui.formControlLabel [
        formControlLabel.label project.Name
        formControlLabel.control (
            Mui.checkbox [
                prop.style [ style.color (projectColour project.Solution.Colour) ]
                checkbox.checked' packagedProjectStatus.HasCodeChanges
                prop.onClick onClick ] ) ]

let private solutionCodeChanges (solution:Solution, packagedProjectStatuses:seq<string * PackagedProjectStatus>) =
    let sorted = packagedProjectStatuses |> List.ofSeq |> List.sortBy (fun (_, pps) -> pps.Project.Name)
    Html.div [
        prop.style [ style.paddingBottom 20 ]
        prop.children [
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
        Html.div [
            prop.style [ style.paddingLeft 20 ; style.paddingTop 20 ]
            prop.children [
                yield! groupedAndSorted |> List.map solutionCodeChanges ] ] ]

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

let private analyis = // TODO-NMB...
    Html.div [
        Mui.typography [
            typography.color.textSecondary
            typography.paragraph true
            typography.children [
                Html.strong "TODO-NMB:"
                Html.text " Analysis of optimal order of package reference updates (during development and when committing/pushing changes)..." ] ] ]

let private app =
    React.functionComponent (fun () ->
        let showingVisualization = ReactHB.Hooks.useAdaptive cShowingVisualization
        let packagedProjectStatuses = ReactHB.Hooks.useAdaptive cPackagedProjectStatuses
        Html.div [
            preamble
            codeChanges packagedProjectStatuses
            analyis
            Mui.divider []
            visualization showingVisualization ] )

ReactDOM.render (app, document.getElementById "app") // needs to match id of div in index.html
