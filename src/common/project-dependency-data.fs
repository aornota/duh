module Aornota.Duh.Common.ProjectDependencyData

open Aornota.Duh.Common.Domain

// #region Solutions

let domainSln = { Name = "Domain" ; Repo = AzureDevOps ; RootPath = "source" ; Colour = Grey }
let infrastructureSln = { Name = "Infrastructure" ; Repo = AzureDevOps ; RootPath = "source" ; Colour = SlateGrey }

let sharedSln = { Name = "Shared" ; Repo = Subversion ; RootPath = "Shared" ; Colour = SeaGreen }

// #endregion

// #region Projects:

let commonInterfacesProj = { Name = "Common.Interfaces" ; Solution = domainSln ; ExtraPath = None }
let commonModelsProj = { Name = "Common.Models" ; Solution = domainSln ; ExtraPath = None }
let commonExtensionsProj = { Name = "Common.Extensions" ; Solution = domainSln ; ExtraPath = None }
let productInterfacesProj = { Name = "Product.Interfaces" ; Solution = domainSln ; ExtraPath = None }
let productModelsProj = { Name = "Product.Models" ; Solution = domainSln ; ExtraPath = None }
let orderInterfacesProj = { Name = "Order.Interfaces" ; Solution = domainSln ; ExtraPath = None }
let orderModelsProj = { Name = "Order.Models" ; Solution = domainSln ; ExtraPath = None }
let infrastructureInterfacesProj = { Name = "Infrastructure.Interfaces" ; Solution = domainSln ; ExtraPath = None }

let repositoriesProj = { Name = "Repositories" ; Solution = infrastructureSln ; ExtraPath = None }

let toolsProj = { Name = "Tools" ; Solution = sharedSln ; ExtraPath = Some "Non Production" }

// #endregion

// #region Packages:

let commonInterfacesPack = Package commonInterfacesProj
let commonModelsPack = Package commonModelsProj
let commonExtensionsPack = Package commonExtensionsProj
let productInterfacesPack = Package productInterfacesProj
let productModelsPack = Package productModelsProj
let orderInterfacesPack = Package orderInterfacesProj
let orderModelsPack = Package orderModelsProj
let infrastructureInterfacesPack = Package infrastructureInterfacesProj

let repositoriesPack = Package repositoriesProj

// toolsProj is not a Package

let packages = [
    commonInterfacesPack ; commonModelsPack ; commonExtensionsPack
    productInterfacesPack ; productModelsPack
    orderInterfacesPack ; orderModelsPack
    infrastructureInterfacesPack
    repositoriesPack ]

// #endregion

// #region ProjectDependencies:

let commonInterfacesDeps = { ProjectOrPackage = Pack commonInterfacesPack ; PackageReferences = [] |> Set.ofList }
let commonModelsDeps = { ProjectOrPackage = Pack commonModelsPack ; PackageReferences = [ commonInterfacesPack ] |> Set.ofList }
let commonExtensionsDeps = { ProjectOrPackage = Pack commonExtensionsPack ; PackageReferences = [ commonModelsPack ] |> Set.ofList }
let productInterfacesDeps = { ProjectOrPackage = Pack productInterfacesPack ; PackageReferences = [ commonInterfacesPack ] |> Set.ofList }
let productModelsDeps = { ProjectOrPackage = Pack productModelsPack ; PackageReferences = [ productInterfacesPack ; commonModelsPack ] |> Set.ofList }
let orderInterfacesDeps = { ProjectOrPackage = Pack orderInterfacesPack ; PackageReferences = [ productInterfacesPack ] |> Set.ofList }
let orderModelsDeps = { ProjectOrPackage = Pack orderModelsPack ; PackageReferences = [ orderInterfacesPack ; productModelsPack ] |> Set.ofList }
let infrastructureInterfacesDeps = { ProjectOrPackage = Pack infrastructureInterfacesPack ; PackageReferences = [ orderInterfacesPack ] |> Set.ofList }

let repositoriesDeps = { ProjectOrPackage = Pack repositoriesPack ; PackageReferences = [ infrastructureInterfacesPack ; orderModelsPack ] |> Set.ofList }

let toolsDeps = { ProjectOrPackage = Proj toolsProj ; PackageReferences = [ orderModelsPack ; commonExtensionsPack ] |> Set.ofList }

let projectsDependencies= [
    commonInterfacesDeps ; commonModelsDeps ; commonExtensionsDeps
    productInterfacesDeps ; productModelsDeps
    orderInterfacesDeps ; orderModelsDeps
    infrastructureInterfacesDeps
    repositoriesDeps
    toolsDeps ]

// #endregion
