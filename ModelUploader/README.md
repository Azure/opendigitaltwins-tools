# NOTICE! THIS TOOL WILL BE DEPRECATED ON AUGUST 1st 2021
# Please refer to [ADTTools](https://github.com/Azure/opendigitaltwins-tools/tree/main/ADTTools) for a replacement set of tools


# The ADT Model Uploader

**Author:** [Karl Hammar](https://karlhammar.com), [Akshay Johar](https://github.com/Azure/opendigitaltwins-building-tools/commits?author=akshayj-MSFT)

This tool simplifies batch uploading of DTDL models, such as the [RealEstateCore DTDL models](https://github.com/Azure/opendigitaltwins-building), into an Azure Digital Twins instance.

The code borrows extensively from the [Azure Digital Twins end-to-end samples](https://docs.microsoft.com/sv-se/samples/azure-samples/digital-twins-samples/digital-twins-samples/)

## How it works

Modeluploader simply traverses the contents of a directory tree via a breadth-first search (using the .NET [Directory.EnumerateFiles](https://docs.microsoft.com/en-us/dotnet/api/system.io.directory.enumeratefiles?view=netcore-3.1) method) to select files for upload, and then uploads them one at a time using the ADT REST API. Provided that the DTDL models are structured in such a manner that super-interfaces are always placed higher in the hierarchy than sub-interfaces, this should always work. 

**Note:** Be mindful of multiple inheritance situations when generating or organizing your models. Ensure that interface with multiple parents are placed as far down as possible in the filesystem hierarchy, to guarantee that all of their parent interfaces are uploaded before they are.

## ADT Configuration

In order for this tool to operate, the user must:

1. Update the [serviceConfig.json](serviceConfig.json) file to include connection details for their ADT Instance. The `tenantId` field is filled with the GUID of the Microsoft 365 tenant's Active Directory. The value of `clientId` is the GUID given by the App Registration for ADT. The value of `instanceUrl` is the DNS name of the ADT instance, prepended by `https://`. if the `tenantId` field is empty, the [default Azure credentials](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme) will be used.

2. Ensure that `serviceConfig.json` is copied to the output directory from which the ModelUploader is executed. This is set under [ModelUploader.csproj](ModelUploader.csproj) and should be activated by default.

## Example usage

`./ModelUploader -p /Users/karl/DTDLModels/`

## Example usage with Deleting All Models First

The "-d" option recursively deletes ALL Models from ADT Instance. You cannot use the -d option alone in this version (a -p option must be specified)

`./ModelUploader -p /Users/karl/DTDLModels/` -d

## Example usage with Uploading more than one set of models

In order for this to work, the user must separate the models  into multiple different directories:

`./ModelUploader -p /Users/karl/DTDLModels/REC`

`./ModelUploader -p /Users/karl/DTDLModels/Willow` 

`./ModelUploader -p /Users/karl/DTDLModels/FoaF` 

...etc
