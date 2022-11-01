# Smart Facilities

This folder contains the code, configuration, and documentation for the SmartPlaces.Facilities Components. This includes several libraries that are also packaged as Nuget Packages, and some sample code.

## Required Software

1. Visual Studio 2022+ or Visual Studio Code

## Libraries

There are two core libraries which have been extended to work with Mapped as a source provider. Other twin source providers are also encouraged to accelerate customer scenarios by partnering through Microsoft into RealEstateCore and other output ontologies built on Azure Digital Twins.

| Name | Description | Read More |
| --- | --- | --- |
| OntologyManager | Defines base classes and helpers which allow mapping from one ontology to another | [Read More](./lib/OntologyMapper/README.md) |
| OntologyManager.Mapped | Defines the mappings from the Mapped ontology to Rec and Willow ontologies | [Read More](./lib/OntologyMapper.Mapped/README.md) |
| IngestioManager | Defines the process flow for copying one topology to another topology and using the Ontology Mapper to convert the source ontology to a target ontology | [Read More](./lib/IngestionManager/README.md) |
| IngestionManager.Mapped | Refines specific functionality needed when using Mapped as an input topology | [Read More](./lib/IngestionManager.Mapped/README.md) |

The nugets can be found on [Nuget.org](https://www.nuget.org/packages?q=Microsoft.SmartPlaces).

### Getting Started with the Lib projects

Each library folder has a stand-alone solution which includes a src and test project.

Dependencies between the projects are taken via Nugets, which can be a little bit painful at times, but we are working to improve that experience.

When updating the src project for any of the libs, please remember to:
1. Update the Assembly Version and Package Version in the csproj as appropriate. The projects use [Semantic Versioning](https://semver.org/), so please follow the semantic versioning guidelines when updating.
2. Add Unit tests for any new features you add or bugs you fix
3. Ensure the documentation for the lib is up to date
4. Upon completion of the pull request, sent a note to [smartplaces@microsoft.com](mailto:smartplaces@microsoft.com) to request that they publish the latest version of the Nuget packages. Response times should be under 2 business days.
5. If you are updating a low level project that one of the other projects depend upon through a nuget reference, you will need to do 2 PRs: one for the low level package, and one for the lib referencing the updated package. Note that the second pull request will not succeed until step 4 above is completed by the Smart Places team. 

### Local Testing

Because of the complexity of the process of getting the official Nuget packages updated, it is always faster to test all the components locally before starting the first pull request. Here are some steps to make testing simpler:

1. Set up a local folder on your machine to put Nuget packages into (i.e. ..\LocalNugets)
2. Update the [Nuget.config](./NuGet.config) and add the following line to it. Never check in this file. It will break the builds.

    ```

    <add key="DELETE_ME" value="..\LocalNugets" />

    ```

3. Update the version number in the lib you have changed
4. Run the following command at a command prompt:

    ```
    
    dotnet nuget pack <path to project file> -o ..\LocalNugets

    ```

5. Copy the nuget package from the bin\(Debug or Release)\ to your nuget folder
6. Update the nuget version reference in your project to reference this new version
7. Repeat this process for any other projects

# Sample Projects

## Deploy

This sample provides scripts which setup key assets in your Azure subscription needed for the use of the other samples. [Read More](./samples/Deploy/README.md)

## Topology

The topology project is an example of a console application which reads in configuration specifying the source topology graph and the target Azure Digital Twins instance, then copies the source graph to the target ADT instance, and converts from the source ontology to the target ontology as configured. [Read More](./samples/Topology/README.md)
