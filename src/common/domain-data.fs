module Aornota.Duh.Common.DomainData

open Aornota.Duh.Common.Domain

let [<Literal>] IS_SCENARIO_TEST_DATA = true

let [<Literal>] PACKAGE_SOURCE__AZURE = "https://{duh}.pkgs.visualstudio.com/_packaging/{duh}/nuget/v3/index.json"
let [<Literal>] PACKAGE_SOURCE__LOCAL = "source/packages"

let solutionMap =
    [
        { Name = "Domain" ; Repo = AzureDevOps ; Colour = Gold ; SortOrder = Some 1 }
        { Name = "Infrastructure" ; Repo = AzureDevOps ; Colour = Tomato ; SortOrder = Some 2 }

        { Name = "Support" ; Repo = Subversion ; Colour = Plum ; SortOrder = None }
    ]
    |> List.map (fun sln -> sln.Name, sln) |> Map

let projectMap =
    [
        { Name = "Common.Interfaces" ; SolutionName = "Domain" ; Packaged = true }
        { Name = "Common.Models" ; SolutionName = "Domain" ; Packaged = true }
        { Name = "Common.Extensions" ; SolutionName = "Domain" ; Packaged = true }
        { Name = "Product.Interfaces" ; SolutionName = "Domain" ; Packaged = true }
        { Name = "Product.Models" ; SolutionName = "Domain" ; Packaged = true }
        { Name = "Order.Interfaces" ; SolutionName = "Domain" ; Packaged = true }
        { Name = "Order.Models" ; SolutionName = "Domain" ; Packaged = true }
        { Name = "Order.Extensions" ; SolutionName = "Domain" ; Packaged = true }
        { Name = "Infrastructure.Interfaces" ; SolutionName = "Domain" ; Packaged = true }

        { Name = "Repositories" ; SolutionName = "Infrastructure" ; Packaged = true }
        { Name = "Repositories.Tests" ; SolutionName = "Infrastructure" ; Packaged = false }

        { Name = "Tools" ; SolutionName = "Support" ; Packaged = false }
        { Name = "Tools.Extensions" ; SolutionName = "Support" ; Packaged = false }
        { Name = "Tools.Tests" ; SolutionName = "Support" ; Packaged = false }
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
            ProjectName = "Order.Extensions"
            Dependencies = [
                PackageReference "Order.Models"
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
            ] |> Set.ofList
        }
        {
            ProjectName = "Tools.Extensions"
            Dependencies = [
                PackageReference "Order.Extensions"
                ProjectReference "Tools"
            ] |> Set.ofList
        }
        {
            ProjectName = "Tools.Tests"
            Dependencies = [
                PackageReference "Common.Extensions"
                ProjectReference "Tools"
                ProjectReference "Tools.Extensions"
            ] |> Set.ofList
        }
    ]
