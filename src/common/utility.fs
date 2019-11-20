module Aornota.Duh.Common.Utility

open System

let concatenate separator = function | [] -> String.Empty | items -> List.reduce (fun item1 item2 -> sprintf "%s%s%s" item1 separator item2) items

let concatenateComma = concatenate ", "
let concatenatePipe = concatenate " | "
