module Aornota.Duh.Common.ProjectDependencyData

open Aornota.Duh.Common.Domain

// #region Solutions:

let domainSln = { Name = "Domain" ; Repo = AzureDevOps ; RootPath = "source" ; Colour = Grey }
let infrastructureSln = { Name = "Infrastructure" ; Repo = AzureDevOps ; RootPath = "source" ; Colour = SlateGrey }

let sharedSln = { Name = "Shared" ; Repo = Subversion ; RootPath = "Shared" ; Colour = SeaGreen }

// #endregion

// #region Projects:

let commonInterfacesProj = { Name = "Common.Interfaces" ; Solution = domainSln ; ExtraPath = None ; Packaged = true }
let commonModelsProj = { Name = "Common.Models" ; Solution = domainSln ; ExtraPath = None ; Packaged = true }
let commonExtensionsProj = { Name = "Common.Extensions" ; Solution = domainSln ; ExtraPath = None ; Packaged = true }
let productInterfacesProj = { Name = "Product.Interfaces" ; Solution = domainSln ; ExtraPath = None ; Packaged = true }
let productModelsProj = { Name = "Product.Models" ; Solution = domainSln ; ExtraPath = None ; Packaged = true }
let orderInterfacesProj = { Name = "Order.Interfaces" ; Solution = domainSln ; ExtraPath = None ; Packaged = true }
let orderModelsProj = { Name = "Order.Models" ; Solution = domainSln ; ExtraPath = None ; Packaged = true }
let infrastructureInterfacesProj = { Name = "Infrastructure.Interfaces" ; Solution = domainSln ; ExtraPath = None ; Packaged = true }

let repositoriesProj = { Name = "Repositories" ; Solution = infrastructureSln ; ExtraPath = None ; Packaged = true }
let repositoriesTestsProj = { Name = "Repositories.Tests" ; Solution = infrastructureSln ; ExtraPath = None ; Packaged = false }

let toolsProj = { Name = "Tools" ; Solution = sharedSln ; ExtraPath = Some "Non Production" ; Packaged = false }

// #endregion

// #region ProjectDependencies:

let commonInterfacesDeps = {
    Project = commonInterfacesProj
    Dependencies = [] |> Set.ofList }
let commonModelsDeps = {
    Project = commonModelsProj
    Dependencies = [
        PackageReference commonInterfacesProj
    ] |> Set.ofList }
let commonExtensionsDeps = {
    Project = commonExtensionsProj
    Dependencies = [
        PackageReference commonModelsProj
    ] |> Set.ofList }
let productInterfacesDeps = {
    Project = productInterfacesProj
    Dependencies = [
        PackageReference commonInterfacesProj
    ] |> Set.ofList }
let productModelsDeps = {
    Project = productModelsProj
    Dependencies = [
        PackageReference productInterfacesProj
        PackageReference commonModelsProj
    ] |> Set.ofList }
let orderInterfacesDeps = {
    Project = orderInterfacesProj
    Dependencies = [
        PackageReference productInterfacesProj
    ] |> Set.ofList }
let orderModelsDeps = {
    Project = orderModelsProj
    Dependencies = [
        PackageReference orderInterfacesProj
        PackageReference productModelsProj
    ] |> Set.ofList }
let infrastructureInterfacesDeps = {
    Project = infrastructureInterfacesProj
    Dependencies = [
        PackageReference orderInterfacesProj
    ] |> Set.ofList }

let repositoriesDeps = {
    Project = repositoriesProj
    Dependencies = [
        PackageReference infrastructureInterfacesProj
        PackageReference orderModelsProj
    ] |> Set.ofList }
let repositoriesTestsDeps = {
    Project = repositoriesTestsProj
    Dependencies = [
        PackageReference commonExtensionsProj
        ProjectReference repositoriesProj
    ] |> Set.ofList }

let toolsDeps = {
    Project = toolsProj
    Dependencies = [
        PackageReference orderModelsProj
        PackageReference commonExtensionsProj
    ] |> Set.ofList }

let projectsDependencies= [
    commonInterfacesDeps ; commonModelsDeps ; commonExtensionsDeps
    productInterfacesDeps ; productModelsDeps
    orderInterfacesDeps ; orderModelsDeps
    infrastructureInterfacesDeps
    repositoriesDeps ; repositoriesTestsDeps
    toolsDeps ]

let packages = projectsDependencies |> List.map (fun pd -> pd.Project ) |> List.filter (fun p -> p.Packaged )

// #endregion
