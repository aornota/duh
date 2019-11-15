module Aornota.Duh.Ui.App

open Fable.React
module RctHB = HookBindings
open Fable.React.Adaptive
open Fable.React.Props
open FSharp.Data.Adaptive

open Feliz
open Feliz.MaterialUI

// (α | β) | γ | δ | ε | ζ | η | θ | ι | κ | λ | μ | ν | ξ | ο | π | ρ | σ | τ | υ | φ | χ | ψ | ω
let [<Literal>] private DUH = "duh (β)" // note: also update ./index.html | ../../package.json | ../../README.md

let [<Literal>] private VISUALIZATION_FILENAME = "visualization.png" // note: keep synchronized with ../visualizer-console/visualizer.fs and ../../build.fsx

(*
let rec nextPrime x =
    let isPrime = function
        | x when x <= 1 -> false
        | 2 -> true
        | x when x % 2 = 0 -> false
        | x -> seq {3..2..x/2} |> Seq.exists (fun d -> x % d = 0) |> not
    if isPrime (x+1) then (x+1) else nextPrime (x+1)

// here is our state:
//   Count is changeable
//   Primes are adaptive (they adapt to change of count)
let cCount = cval 1
let aPrime = cCount |> AVal.map nextPrime
let aPrime10 = aPrime |> AVal.map (fun x ->
    let x' = x*10
    // open browser's debug console to notice lazy evaluation of adaptive values
    printfn "prime multiplier: %d * 10 = %d" x x'
    x')

let PrimeApp =
    FunctionComponent.Of( fun () ->
        let prime = Hooks.useAdaptive aPrime
        let prime10 = Hooks.useAdaptive aPrime10
        div [] [
            p [] [ str (sprintf "next prime = %d" prime) ]
            p [] [ str (sprintf "next prime * 10 = %d" prime10) ]
        ]
    )

let CounterApp =
    FunctionComponent.Of( fun () ->
        let count = Hooks.useAdaptive cCount
        let showingPrimes = Hooks.useState true
        div [] [
            p [] [
                str (sprintf "Current count = %d " count)
                button [ OnClick (fun _ -> transact (fun () -> cCount.Value <- cCount.Value + 1)) ] [ str "+" ]
            ]
            div [] [
                button [ OnClick (fun _ -> showingPrimes.update (fun showing -> not showing)) ] [str "toggle primes"]
                if showingPrimes.current then
                    div [ Style [PaddingLeft "2em"] ] [ PrimeApp () ]
            ]
        ]
    )
*)

let private preamble =
    Html.div [
        Mui.typography [
            typography.variant.h4
            typography.paragraph true
            typography.children [
                Html.img [
                    prop.style [ style.verticalAlign.middle ]
                    prop.src "duh-24x24.png"
                    prop.alt "duh" ]
                Html.text (sprintf " | %s" DUH) ] ]
        // TODO-NMB: More...
        Mui.typography [
            typography.paragraph true
            prop.children [
                Html.strong "duh"
                Html.text " (dependency update helper) is a tool to help with "
                Html.strong [Html.em "blah blah blah..." ] ] ]
    ]

let private visualization (showingVisualization:IStateHook<bool>) =
    Html.div [
        (* TODO-NMB: Remove this...
        Mui.button [
            button.color.secondary
            button.size.small
            button.variant.outlined
            prop.onClick (fun _ -> showingVisualization.update not)
            button.children [ Html.text (sprintf "%s visualization of project/package dependencies" (if showingVisualization.current then "Hide" else "Display")) ] ] *)
        Mui.formGroup [
            // TODO-NMB: Remove this....formGroup.row false
            formGroup.children [
                Mui.formControlLabel [
                    formControlLabel.label "Display visualization of project/package dependencies"
                    formControlLabel.control (
                        Mui.checkbox [
                            checkbox.color.primary
                            checkbox.checked' showingVisualization.current
                            prop.onClick (fun _ -> showingVisualization.update not) ] ) ] ] ]
        if showingVisualization.current then
            Html.div [
                prop.style [ style.paddingTop 20 ]
                prop.children [
                    Html.img [
                        prop.src VISUALIZATION_FILENAME
                        prop.alt "Visualization of project/package dependencies" ] ] ] ]

let app =
    FunctionComponent.Of(fun () ->
        let showingVisualization = RctHB.Hooks.useState false
        Html.div [
            preamble
            // TODO-NMB: More...
            visualization showingVisualization ]
    )
