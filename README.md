# Tools for Open Digital Twins Definition Language (DTDL) based Ontologies

## Motivation and purpose

[Azure Digital Twins (ADT)](https://azure.microsoft.com/en-us/services/digital-twins/), and its underlying [Digital Twins Definition Language (DTDL)](https://github.com/Azure/opendigitaltwins-dtdl), are at the heart of Smart Building solutions built on Azure.

DTDL is the language by which developers can define the language of the entities they expect to use in their topologies. Since DTDL is a blank canvas which can model any entity, it is important to accelerate developers' time to results by providing a common domain-specific ontology to bootstrap solution building, as well as seamless integration between DTDL-based solutions from different vendors.

This is a set of open-source ontology tools which one can use to operate on any ontologies, including the [Real Estate Core Ontology](https://github.com/Azure/opendigitaltwins-building)

## Uploading models to Azure Digital Twins
You can upload this ontology into your own instance of ADT by using [UploadModels](https://github.com/Azure/opendigitaltwins-tools/tree/main/ADTTools/UploadModels). Follow the [instructions](https://github.com/Azure/opendigitaltwins-tools/tree/main/ADTTools#azure-digital-twins-tools) on Upload to upload all of these models into your own instance. Here is [an article](https://docs.microsoft.com/en-us/azure/digital-twins/how-to-manage-model) on how to manage models, update, retrieve, update, decommission and delete models.

## Deleting models in bulk
You can also delete models that are previously uploaded to an instance of ADT. For this you can use the [DeleteModels](https://github.com/Azure/opendigitaltwins-tools/tree/main/ADTTools/DeleteModels) tool. Instructions of how this can be run are found [here](https://github.com/Azure/opendigitaltwins-tools/tree/main/ADTTools#deletemodels)

## Visualizing the models
Once you have uploaded these models into your Azure Digital Twins instance, you can view the ontology using [ADT Model Visualizer](AdtModelVisualizer). This tool is a draft version (read-only visualizer, no edits) and we invite you to contribute to it to make it better.

## Validating the models
The DTDL RealEstateCore models in this repo have already been validated. You don't have to validate them with the DTDL parser unless you change them. If you have extended the models or made changes, it's recommended to validate the models as described by this article: [Validate models](https://docs.microsoft.com/en-us/azure/digital-twins/concepts-convert-models#validate-and-upload-dtdl-models).

## Converting from one ontology / version to another
The OntologyMapper is an assembly that is used to support conversion from one DTDL ontology to another DTDL ontology. More details can be found [here]: (./OntologyMapper/README.md) 