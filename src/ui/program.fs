module Aornota.Duh.Ui.Program

open Aornota.Duh.Ui.App

open Elmish
open Elmish.React
open Elmish.HMR

// Elmish is only being used for "hot-reload" functionality; the real "application" is in adaptive-app.fs.

let initialize () = 0
let transition _message state = state
let render _state _dispatch = app ()

Program.mkSimple initialize transition render
|> Program.withReactBatched "elmish-app" // needs to match id of div in index.html
|> Program.withConsoleTrace
|> Program.run
