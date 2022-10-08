module Aornota.Duh.Ui.App

open Aornota.Duh.Common.AdaptiveValues
open Aornota.Duh.Common.ChangeableValues
open Aornota.Duh.Common.DependencyPaths
open Aornota.Duh.Common.Domain
open Aornota.Duh.Common.DomainData
open Aornota.Duh.Common.Utility

open Browser.Dom

open Fable.React.Adaptive
module ReactHB = Fable.React.HookBindings

open Feliz
open Feliz.MaterialUI

open FSharp.Data.Adaptive

open System

let [<Literal>] private DUH = "duh"

// (α | β | γ | δ) | *ε* | ζ | η | θ | ι | κ | λ | μ | ν | ξ | ο | π | ρ | σ | τ | υ | φ | χ | ψ | ω
let [<Literal>] private DUH_VERSION = "ε" // note: keep synchronized with  ./index.html | ../../package.json | ../../README.md

let [<Literal>] private DUH_LOGO = "duh-24x24.png"
let [<Literal>] private VISUALIZATION_FILENAME = "visualization.svg" // note: keep synchronized with ../visualizer-console/visualizer.fs and ../../build.fsx

let [<Literal>] private BULLET = "●"

let private repoText = function | AzureDevOps -> "Azure DevOps" | Subversion -> "Subversion"

let private projectColour = function
    | Gold -> color.gold | LightSkyBlue -> color.lightSkyBlue | Plum -> color.plum | SandyBrown -> color.sandyBrown | Tomato -> color.tomato | YellowGreen -> color.yellowGreen

let private isProjectDependency = function | ProjectDependency _ -> true | _ -> false

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

let private solutionCodeChanges (solution:Solution, packagedProjectStatuses:PackagedProjectStatus list) =
    let sorted = packagedProjectStatuses |> List.sortBy (fun pps -> sortOrder pps.SortOrder, pps.ProjectName)
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

let private codeChanges (packagedProjectStatuses:PackagedProjectStatus list) =
    let groupedAndSorted =
        packagedProjectStatuses
        |> List.groupBy (fun pps -> solution pps.ProjectName)
        |> List.sortBy (fun (solution, _) -> solutionSortOrder solution, solution.Name)
    Html.div [
        Mui.typography [
            typography.paragraph false
            typography.color.secondary
            typography.children [
                Html.text "Please select the packaged projects for which code changes have been made locally:" ] ]
        Mui.grid [
            prop.style [ style.paddingTop 20 ; style.paddingLeft 20 ; style.paddingBottom 20 ]
            grid.container true
            grid.spacing._4
            grid.children [
                yield! groupedAndSorted |> List.map solutionCodeChanges ] ] ]

let private analysis (affected:(int * ProjectDependencyPaths list) list) currentTab latestDone =
    let latestDone = latestDone |> Option.defaultValue 0
    let step (ordinal, projectsDependencyPaths:ProjectDependencyPaths list) = [
        let stepProject isDone (projectDependencyPaths:ProjectDependencyPaths) =
            let projectName = projectDependencyPaths.ProjectName
            let project = projectMap.[projectName]
            let testsProjectName, isTestsProject =
                match project.ProjectType with
                | Some (Packaged (_, Some testsProjectName)) -> Some testsProjectName, false
                | Some Tests -> None, true
                | _ -> None, false
            let solution = solutionMap.[project.SolutionName]
            let selfOrDirects = projectDependencyPaths.DependencyPaths |> List.map (fun dp -> dp |> List.last)
            let packageDependencies = selfOrDirects |> List.choose (fun di -> if isPackageDependency di.DependencyType then Some di.ProjectName else None)
            let updatePackageReferences =
                match packageDependencies with
                | [] -> []
                | _ ->
                    let packages = packageDependencies |> List.sort |> concatenatePipe
                    let plural = if packageDependencies.Length > 1 then "s" else String.Empty
                    let source = match currentTab with | Development -> "local" | CommittingPushing -> "published"
                    [
                        Html.text (sprintf "%s update reference%s to %s (using the %s package source) for " BULLET plural packages source)
                    ]
            let projectDependenciesNotes =
                match selfOrDirects |> List.choose (fun di -> if isProjectDependency di.DependencyType then Some di.ProjectName else None) with
                | [] -> []
                | projectDependencies ->
                    let projects = projectDependencies |> List.sort |> concatenatePipe
                    let projectReferences, plural = if projectDependencies.Length > 1 then "project references", "s" else "a project reference", String.Empty
                    let also = if packageDependencies.Length > 0 then "also " else String.Empty
                    [
                        Html.text (sprintf "which %shas %s to the %s project%s updated above" also projectReferences projects plural)
                    ]
            let projectLines =
                let buildText = if isTestsProject then "build and run tests for" else "build"
                let action =
                    match currentTab with
                    | Development -> buildText
                    | CommittingPushing ->
                        let selfChanged = selfOrDirects |> List.exists (fun di -> match di.DependencyType with | Self -> true | _ -> false)
                        if selfChanged || packageDependencies.Length > 0 then
                            sprintf "commit%s changes for" (match solution.Repo with | AzureDevOps -> " and push" | Subversion -> String.Empty)
                        else buildText
                [
                    if updatePackageReferences.Length > 0 then
                        let additionalAction =
                            match currentTab, testsProjectName, isTestsProject with
                            | CommittingPushing, Some testsProjectName, _ -> sprintf " run tests for %s (see below)," testsProjectName
                            | CommittingPushing, _, true -> " build and run tests,"
                            | _ -> String.Empty
                        yield! updatePackageReferences
                        Html.strong projectName
                        Html.text (sprintf " (%s solution),%s then %s this project" solution.Name additionalAction action)
                        if projectDependenciesNotes.Length > 0 then
                            Html.text " ("
                            yield! projectDependenciesNotes
                            Html.text ")"
                    else
                        let additionalAction =
                            match currentTab, testsProjectName with
                            | CommittingPushing, Some testsProjectName -> sprintf "run tests for %s (see below), then " testsProjectName
                            | _ -> String.Empty
                        Html.text (sprintf "%s %s%s " BULLET additionalAction action)
                        Html.strong projectName
                        Html.text (sprintf " (%s solution)" solution.Name)
                        if projectDependenciesNotes.Length > 0 then
                            Html.text ", "
                            yield! projectDependenciesNotes
                ]
            Mui.typography [
                prop.style [ style.paddingLeft 20 ]
                typography.paragraph false
                if isDone then typography.color.textSecondary
                typography.children [
                    yield! projectLines ] ]
        let isDone = ordinal <= latestDone
        let isNext = ordinal = latestDone + 1
        let waitForPublish =
            match currentTab with
            | Development -> []
            | CommittingPushing ->
                projectsDependencyPaths
                |> List.choose (fun pdp -> match projectMap.[pdp.ProjectName].ProjectType with | Some (Packaged _) -> Some pdp.ProjectName | _ -> None)
        Mui.typography [
            typography.variant.h6
            typography.paragraph false
            if isDone then typography.color.textSecondary
            else if isNext then typography.color.primary
            typography.children [
                Html.strong (sprintf "Step %i" ordinal) ] ]
        yield! projectsDependencyPaths |> List.map (stepProject isDone)
        match waitForPublish with
        | [] -> ()
        | _ ->
            let packages = waitForPublish |> List.sort |> concatenatePipe
            let plural = if waitForPublish.Length > 1 then "s" else String.Empty
            Mui.typography [
                prop.style [ style.paddingLeft 20 ]
                typography.paragraph false
                if isDone then typography.color.textSecondary
                typography.children [
                    Html.text (sprintf "%s wait for updated " BULLET)
                    Html.strong packages
                    Html.text (sprintf " package%s to be published" plural)
                     ] ]
        if isNext then
            Mui.button [
                button.variant.text
                button.color.primary
                prop.onClick (fun _ -> transact (fun () -> cTabLatestDoneMap.[currentTab] <- Some ordinal ))
                button.children [ Html.text "Done" ] ] ]
    let tabNotes =
        match currentTab with
        | Development -> [
            Mui.typography [
                typography.paragraph true
                typography.children [
                    Html.text "Please ensure that you have configured "
                    Html.em (sprintf "/%s" PACKAGE_SOURCE__LOCAL)
                    Html.text " as a local package source." ] ]
            Mui.typography [
                typography.paragraph true
                typography.children [
                    Html.text "You may decide to defer later steps, e.g. if you are confident that the projects involved will not be affected by the changes in the earlier steps."] ] ]
        | CommittingPushing -> [
            let eg = if IS_SCENARIO_TEST_DATA then "e.g. " else String.Empty
            Mui.typography [
                typography.paragraph true
                typography.children [
                    Html.text (sprintf "The published package source (%s " eg)
                    Html.em PACKAGE_SOURCE__AZURE
                    Html.text ") should already be configured." ] ] ]
    Html.div [
        prop.style [ style.paddingLeft 20 ; style.paddingBottom 20 ]
        prop.children [
            yield! tabNotes
            yield! affected |> List.map step |> List.collect id ] ]

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
                    icon.classes.root iconClassName
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
                yield! analysisTabs |> List.map muiTab ] ]
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
                            Html.text (sprintf "%s packaged projects are shown as oblongs" BULLET) ] ]
                    Mui.typography [
                        typography.paragraph false
                        typography.children [
                            Html.text (sprintf "%s tests projects are shown with a dashed border" BULLET) ] ]
                    Mui.typography [
                        typography.paragraph false
                        typography.children [
                            Html.text (sprintf "%s projects in the same solution are shown with the same colour" BULLET) ] ]
                    Mui.typography [
                        typography.paragraph false
                        typography.children [
                            Html.text (sprintf "%s solid arrows indicate package references" BULLET) ] ]
                    Mui.typography [
                        typography.paragraph false
                        typography.children [
                            Html.text (sprintf "%s dashed arrows indicate project references" BULLET) ] ] ] ] ]

let private app =
    React.functionComponent (fun () ->
        let packagedProjectStatuses = ReactHB.Hooks.useAdaptive aPackagedProjectStatuses
        let analysis = ReactHB.Hooks.useAdaptive aAnalysis
        let showingVisualization = ReactHB.Hooks.useAdaptive cShowingVisualization
        Html.div [
            preamble
            codeChanges packagedProjectStatuses
            match analysis with
            | Some (affected, currentTab, tabLatestDoneMap) -> analysisTabs affected currentTab tabLatestDoneMap.[currentTab]
            | None -> ()
            Mui.divider []
            visualization showingVisualization ])

ReactDOM.render (app (), document.getElementById "app") // needs to match id of div in index.html
