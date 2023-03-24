<<<<<<< HEAD
# Tools for Open Digital Twins Definition Language (DTDL) based Ontologies

## Motivation and purpose

[Azure Digital Twins (ADT)](https://azure.microsoft.com/en-us/services/digital-twins/), and its underlying [Digital Twins Definition Language (DTDL)](https://github.com/Azure/opendigitaltwins-dtdl), are at the heart of Smart Facilities solutions built on Azure.

DTDL is the language by which developers can define the language of the entities they expect to use in their topologies. Since DTDL is a blank canvas which can model any entity, it is important to accelerate developers' time to results by providing a common domain-specific ontology to bootstrap solution building, as well as seamless integration between DTDL-based solutions from different vendors.

This is a set of open-source ontology tools which one can use to operate on any ontologies, including the [Real Estate Core Ontology](https://github.com/RealEstateCore/rec)

## Uploading models to Azure Digital Twins
You can upload an ontology into your own instance of ADT by using [UploadModels](https://github.com/Azure/opendigitaltwins-tools/tree/main/ADTTools/UploadModels). Follow the [instructions](https://github.com/Azure/opendigitaltwins-tools/tree/main/ADTTools#azure-digital-twins-tools) on Upload to upload all of these models into your own instance. Here is [an article](https://docs.microsoft.com/en-us/azure/digital-twins/how-to-manage-model) on how to manage models, update, retrieve, update, decommission and delete models.

## Deleting models in bulk
You can also delete models that are previously uploaded to an instance of ADT. For this you can use the [DeleteModels](https://github.com/Azure/opendigitaltwins-tools/tree/main/ADTTools/DeleteModels) tool. Instructions of how this can be run are found [here](https://github.com/Azure/opendigitaltwins-tools/tree/main/ADTTools#deletemodels)

## Visualizing the models
Once you have uploaded these models into your Azure Digital Twins instance, you can view the ontology using [Azure Digital Twins Explorer](https://explorer.digitaltwins.azure.net/).

## Validating the models
The DTDL RealEstateCore models in this repo have already been validated. You don't have to validate them with the DTDL parser unless you change them. If you have extended the models or made changes, it's recommended to validate the models as described by this article: [Validate models](https://learn.microsoft.com/en-us/azure/digital-twins/concepts-models#validate-models).

## Converting from one ontology / version to another
The OntologyMapper is an assembly that is used to support conversion from one DTDL ontology to another DTDL ontology. More details can be found [here](SmartPlaces.Facilities/lib/OntologyMapper/README.md)
=======
---
page_type: sample
languages:
- csharp
products:
- azure-digital-twins
- azure-iot-pnp
name: DTDL Validator
description: A code sample for validating DTDL model code
urlFragment: dtdl-validator
---

# Introduction 
This project demonstrates use of the Azure Digital Twins DTDL parser, available [here](https://nuget.org/packages/Microsoft.Azure.DigitalTwins.Parser/) on NuGet. It  is language-agnostic, and can be used as a command line utility to validate a directory tree of DTDL files. It also provides an interactive mode.

The source code shows examples for how to use the parser library, and can validate model documents to make sure the DTDL is valid.

# Getting started
The program is a command line application that can be used in normal or interactive mode.

In normal mode, specify:
* a file extension (-e, default json)
* a directory to search (-d, no default value)
* a recursive option that determines if the file search descends into subdirectories (-r, default false)

Interactive mode is entered with the -i option. Type help for information on interactive commands

# What the code demonstrates
* Basic use of the DTDL parser for validation of DTDL
* Basic use of the object model to access information about DTDL content (see the interactive module, in particular the list and show/showinfo commands)

# Build and test
Build the project and run the application from the command line.

You can also create a self-contained single-file .exe (no other files or installations required):

Run
```bash
dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true
```
in the root folder of the repo.

# Package as Global Tool
Run
``` bash
dotnet publish
dotnet tool install --global --add-source ./DTDLValidator/nupkg DTDLValidator
```

This appends the path of the generated executible to your system's **PATH** variable.
Now, run `dtdl-validator <ARGS>` to use the tool.

>>>>>>> master/master
