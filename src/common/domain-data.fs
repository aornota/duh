module Aornota.Duh.Common.DomainData

open Aornota.Duh.Common.Domain

let [<Literal>] IS_SCENARIO_TEST_DATA = true

let [<Literal>] PACKAGE_SOURCE__AZURE = "https://{duh}.pkgs.visualstudio.com/_packaging/{duh}/nuget/v3/index.json"
let [<Literal>] PACKAGE_SOURCE__LOCAL = "source/packages"

let solutionMap =
    [
        { Name = "Domain" ; Repo = AzureDevOps ; Colour = Gold ; SortOrder = Some 1 }
        { Name = "Infrastructure" ; Repo = AzureDevOps ; Colour = Tomato ; SortOrder = Some 2 }
        { Name = "Services" ; Repo = AzureDevOps ; Colour = LightSkyBlue ; SortOrder = Some 3 }

        { Name = "Support" ; Repo = Subversion ; Colour = Plum ; SortOrder = None }
    ]
    |> List.map (fun sln -> sln.Name, sln) |> Map

let projectMap =
    [
        // #region Domain solution
        { Name = "Common.Interfaces" ; SolutionName = "Domain" ; ProjectType = Some (Packaged (Some 1, None)) }
        { Name = "Common.Models" ; SolutionName = "Domain" ; ProjectType = Some (Packaged (Some 2, Some "Common.Models.Tests")) }
        { Name = "Common.Models.Tests" ; SolutionName = "Domain" ; ProjectType = Some Tests }
        { Name = "Common.Extensions" ; SolutionName = "Domain" ; ProjectType = Some (Packaged (Some 3, Some "Common.Extensions.Tests")) }
        { Name = "Common.Extensions.Tests" ; SolutionName = "Domain" ; ProjectType = Some Tests }
        { Name = "Product.Interfaces" ; SolutionName = "Domain" ; ProjectType = Some (Packaged (Some 4, None)) }
        { Name = "Product.Models" ; SolutionName = "Domain" ; ProjectType = Some (Packaged (Some 5, Some "Product.Models.Tests")) }
        { Name = "Product.Models.Tests" ; SolutionName = "Domain" ; ProjectType = Some Tests }
        { Name = "Product.Extensions" ; SolutionName = "Domain" ; ProjectType = Some (Packaged (Some 6, Some "Product.Extensions.Tests")) }
        { Name = "Product.Extensions.Tests" ; SolutionName = "Domain" ; ProjectType = Some Tests }
        { Name = "Order.Interfaces" ; SolutionName = "Domain" ; ProjectType = Some (Packaged (Some 7, None)) }
        { Name = "Order.Models" ; SolutionName = "Domain" ; ProjectType = Some (Packaged (Some 8, Some "Order.Models.Tests")) }
        { Name = "Order.Models.Tests" ; SolutionName = "Domain" ; ProjectType = Some Tests }
        { Name = "Order.Extensions" ; SolutionName = "Domain" ; ProjectType = Some (Packaged (Some 9, Some "Order.Extensions.Tests")) }
        { Name = "Order.Extensions.Tests" ; SolutionName = "Domain" ; ProjectType = Some Tests }
        { Name = "Infrastructure.Interfaces" ; SolutionName = "Domain" ; ProjectType = Some (Packaged (Some 10, None)) }
        // #endregion

        // #region Infrastructur solution
        { Name = "Repositories" ; SolutionName = "Infrastructure" ; ProjectType = Some (Packaged (None, Some "Repositories.Tests")) }
        { Name = "Repositories.Tests" ; SolutionName = "Infrastructure" ; ProjectType = Some Tests }
        // #endregion

        // #region Services solution
        { Name = "Services.Interfaces" ; SolutionName = "Services" ; ProjectType = Some (Packaged (Some 1, None)) }
        { Name = "Services.Implementation" ; SolutionName = "Services" ; ProjectType = Some (Packaged (Some 2, Some "Services.Implementation.Tests")) }
        { Name = "Services.Implementation.Tests" ; SolutionName = "Services" ; ProjectType = Some Tests }
        // #endregion

        // #region Support solution
        { Name = "Tools" ; SolutionName = "Support" ; ProjectType = None }
        { Name = "Tools.Extensions" ; SolutionName = "Support" ; ProjectType = None }
        { Name = "Tools.Tests" ; SolutionName = "Support" ; ProjectType = Some Tests }
        // #endregion
    ]
    |> List.map (fun proj -> proj.Name, proj) |> Map

let projectsDependencies =
    [
        // #region Domain solution
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
            ProjectName = "Common.Models.Tests"
            Dependencies = [
                ProjectReference "Common.Models"
            ] |> Set.ofList
        }
        {
            ProjectName = "Common.Extensions"
            Dependencies = [
                PackageReference "Common.Interfaces"
            ] |> Set.ofList
        }
        {
            ProjectName = "Common.Extensions.Tests"
            Dependencies = [
                PackageReference "Common.Models"
                ProjectReference "Common.Extensions"
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
                PackageReference "Common.Models"
                PackageReference "Product.Interfaces"
            ] |> Set.ofList
        }
        {
            ProjectName = "Product.Models.Tests"
            Dependencies = [
                ProjectReference "Product.Models"
            ] |> Set.ofList
        }
        {
            ProjectName = "Product.Extensions"
            Dependencies = [
                PackageReference "Common.Extensions"
                PackageReference "Product.Interfaces"
            ] |> Set.ofList
        }
        {
            ProjectName = "Product.Extensions.Tests"
            Dependencies = [
                PackageReference "Product.Models"
                ProjectReference "Product.Extensions"
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
            ProjectName = "Order.Models.Tests"
            Dependencies = [
                ProjectReference "Order.Models"
            ] |> Set.ofList
        }
        {
            ProjectName = "Order.Extensions"
            Dependencies = [
                PackageReference "Common.Extensions"
                PackageReference "Order.Interfaces"
            ] |> Set.ofList
        }
        {
            ProjectName = "Order.Extensions.Tests"
            Dependencies = [
                PackageReference "Order.Models"
                ProjectReference "Order.Extensions"
            ] |> Set.ofList
        }
        {
            ProjectName = "Infrastructure.Interfaces"
            Dependencies = [
                PackageReference "Order.Interfaces"
            ] |> Set.ofList
        }
        // #endregion

        // #region Infrastructure solution
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
                PackageReference "Order.Extensions"
                PackageReference "Product.Extensions"
                ProjectReference "Repositories"
            ] |> Set.ofList
        }
        // #endregion

        // #region Services solution
        {
            ProjectName = "Services.Interfaces"
            Dependencies = [
                PackageReference "Infrastructure.Interfaces"
            ] |> Set.ofList
        }
        {
            ProjectName = "Services.Implementation"
            Dependencies = [
                PackageReference "Order.Extensions"
                PackageReference "Product.Extensions"
                PackageReference "Services.Interfaces"
            ] |> Set.ofList
        }
        {
            ProjectName = "Services.Implementation.Tests"
            Dependencies = [
                PackageReference "Repositories"
                ProjectReference "Services.Implementation"
            ] |> Set.ofList
        }
        // #endregion

        // #region Support solution
        {
            ProjectName = "Tools"
            Dependencies = [
                PackageReference "Repositories"
                PackageReference "Services.Implementation"
            ] |> Set.ofList
        }
        {
            ProjectName = "Tools.Extensions"
            Dependencies = [
                ProjectReference "Tools"
            ] |> Set.ofList
        }
        {
            ProjectName = "Tools.Tests"
            Dependencies = [
                ProjectReference "Tools.Extensions"
            ] |> Set.ofList
        }
        // #endregion
    ]
