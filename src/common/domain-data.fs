module Aornota.Duh.Common.DomainData

open Aornota.Duh.Common.Domain

let solutionMap =
    [
        { Name = "Domain" ; Repo = AzureDevOps ; RootPath = "source" ; Colour = Grey ; SortOrder = Some 1 }
        { Name = "Infrastructure" ; Repo = AzureDevOps ; RootPath = "source" ; Colour = Cyan ; SortOrder = Some 2 }

        { Name = "Shared" ; Repo = Subversion ; RootPath = "Shared" ; Colour = Salmon ; SortOrder = None }
    ]
    |> List.map (fun sln -> sln.Name, sln) |> Map

let projectMap =
    [
        { Name = "Common.Interfaces" ; SolutionName = "Domain" ; ExtraPath = None ; Packaged = true }
        { Name = "Common.Models" ; SolutionName = "Domain" ; ExtraPath = None ; Packaged = true }
        { Name = "Common.Extensions" ; SolutionName = "Domain" ; ExtraPath = None ; Packaged = true }
        { Name = "Product.Interfaces" ; SolutionName = "Domain" ; ExtraPath = None ; Packaged = true }
        { Name = "Product.Models" ; SolutionName = "Domain" ; ExtraPath = None ; Packaged = true }
        { Name = "Order.Interfaces" ; SolutionName = "Domain" ; ExtraPath = None ; Packaged = true }
        { Name = "Order.Models" ; SolutionName = "Domain" ; ExtraPath = None ; Packaged = true }
        { Name = "Infrastructure.Interfaces" ; SolutionName = "Domain" ; ExtraPath = None ; Packaged = true }

        { Name = "Repositories" ; SolutionName = "Infrastructure" ; ExtraPath = None ; Packaged = true }
        { Name = "Repositories.Tests" ; SolutionName = "Infrastructure" ; ExtraPath = None ; Packaged = false }

        { Name = "Tools" ; SolutionName = "Shared" ; ExtraPath = Some "Non Production" ; Packaged = false }
        { Name = "Tools.Tests" ; SolutionName = "Shared" ; ExtraPath = Some "Non Production" ; Packaged = false }
    ]
    |> List.map (fun proj -> proj.Name, proj) |> Map

let projectsDependencies =
    [
        {
            ProjectName = "Common.Interfaces"
            Dependencies = [] |> Set.ofList
        }
        {
            ProjectName = "Common.Models"
            Dependencies = [
                PackageReference "Common.Interfaces"
            ] |> Set.ofList
        }
        {
            ProjectName = "Common.Extensions"
            Dependencies = [
                PackageReference "Common.Models"
            ] |> Set.ofList
        }
        {
            ProjectName = "Product.Interfaces"
            Dependencies = [
                PackageReference "Common.Interfaces"
            ] |> Set.ofList
        }
        {
            ProjectName = "Product.Models"
            Dependencies = [
                PackageReference "Product.Interfaces"
                PackageReference "Common.Models"
            ] |> Set.ofList
        }
        {
            ProjectName = "Order.Interfaces"
            Dependencies = [
                PackageReference "Product.Interfaces"
            ] |> Set.ofList
        }
        {
            ProjectName = "Order.Models"
            Dependencies = [
                PackageReference "Order.Interfaces"
                PackageReference "Product.Models"
            ] |> Set.ofList
        }
        {
            ProjectName = "Infrastructure.Interfaces"
            Dependencies = [
                PackageReference "Order.Interfaces"
            ] |> Set.ofList
        }

        {
            ProjectName = "Repositories"
            Dependencies = [
                PackageReference "Infrastructure.Interfaces"
                PackageReference "Order.Models"
            ] |> Set.ofList
        }
        {
            ProjectName = "Repositories.Tests"
            Dependencies = [
                PackageReference "Common.Extensions"
                ProjectReference "Repositories"
            ] |> Set.ofList
        }

        {
            ProjectName = "Tools"
            Dependencies = [
                PackageReference "Repositories"
                PackageReference "Common.Extensions"
            ] |> Set.ofList
        }
        {
            ProjectName = "Tools.Tests"
            Dependencies = [
                ProjectReference "Tools"
            ] |> Set.ofList
        }
    ]
