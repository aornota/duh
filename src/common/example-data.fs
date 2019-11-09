module Aornota.Duh.Common.ExampleData

open Aornota.Duh.Common.Domain

// #region Solutions

let domainSln = { Name = "Domain" ; Repo = AzureDevOps ; Colour = Grey }
let repositorySln = { Name = "Repository" ; Repo = AzureDevOps ; Colour = Yellow }
let toolsSln = { Name = "Tools" ; Repo = Subversion ; Colour = Pink }

// #endregion

// #region Projects:

let commonInterfacesProj = { Name = "Common.Interfaces" ; Solution = domainSln }
let commonModelsProj = { Name = "Common.Models" ; Solution = domainSln }
let commonExtensionsProj = { Name = "Common.Extensions" ; Solution = domainSln }
let productInterfacesProj = { Name = "Product.Interfaces" ; Solution = domainSln }
let productModelsProj = { Name = "Product.Models" ; Solution = domainSln }
let orderInterfacesProj = { Name = "Order.Interfaces" ; Solution = domainSln }
let orderModelsProj = { Name = "Order.Models" ; Solution = domainSln }
let infrastructureInterfacesProj = { Name = "Infrastructure.Interfaces" ; Solution = domainSln }

let repositoryProj = { Name = "Repository" ; Solution = repositorySln }

let toolsProj = { Name = "Tools" ; Solution = toolsSln }

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

let repositoryPack = Package repositoryProj

let packages = [
    commonInterfacesPack ; commonModelsPack ; commonExtensionsPack
    productInterfacesPack ; productModelsPack
    orderInterfacesPack ; orderModelsPack
    infrastructureInterfacesPack
    repositoryPack ]

// toolsProj is not a Package

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

let repositoryDeps = { ProjectOrPackage = Pack repositoryPack ; PackageReferences = [ infrastructureInterfacesPack ; orderModelsPack ] |> Set.ofList }

let toolsDeps = { ProjectOrPackage = Proj toolsProj ; PackageReferences = [ orderModelsPack ; commonExtensionsPack ] |> Set.ofList }

let projectsDependencies= [
    commonInterfacesDeps ; commonModelsDeps ; commonExtensionsDeps
    productInterfacesDeps ; productModelsDeps
    orderInterfacesDeps ; orderModelsDeps
    infrastructureInterfacesDeps
    repositoryDeps
    toolsDeps ]

// #endregion
