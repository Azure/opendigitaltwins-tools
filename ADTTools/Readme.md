# Azure Digital Twins tools

This set of tools is designed to make it easy to manage DTDL ontologies with the Azure Digital Twins service.

## UploadModels

UploadModels is used to upload an ontology (set of models) into an Azure Digital Twins service instance. The tool accepts a list of models (including wildcard and glob support), validates the models using the digital twins parser, orders the models so that "root" models are uploaded first, and then uploads models in batches for fast uploading.

### Usage

`UploadModels [options] <fileList>`

### Options

Upload
- `-t` or `--tenantId`: The id (GUID) of your tenant or directory in Azure.
- `-c` or `--clientId`: The id (GUID) of an Azure Active Directory app registration. See [Create an app registration to use with Azure Digital Twins](https://docs.microsoft.com/en-us/azure/digital-twins/how-to-create-app-registration) for more information.
- `-h` or `--hostName`: The host name of your Azure Digital Twins service instance (no https prefix needed).

Test
- `-w` or `--whatIf`: When set, without the upload options, displays an ordered list of the models that would be uploaded.

### File list

- fileList: A list of model file names that supports wildcards and globs.

### Example usage

- Upload one model: `UploadModels -t 42a9ff5e-dd3a-4a89-8d92-2a7eaadbfcaa -c 198783fb-4301-4983-a12f-4eefb696282d -h briancr-ms.api.wcus.digitaltwins.azure.net .\MyModel.json`
- Upload an ontology: `UploadModels -t 42a9ff5e-dd3a-4a89-8d92-2a7eaadbfcaa -c 198783fb-4301-4983-a12f-4eefb696282d -h briancr-ms.api.wcus.digitaltwins.azure.net \Ontology\**\*.json`
- Display an ordered list of models that would be uploaded: `UploadModels -w \Ontology\**\*.json`

## DeleteModels

DeleteModels is used to delete all the models in an Azure Digital Twins service instance. The tool deletes the models recursively so that "leaf" models are deleted first and "root" models are deleted last.

### Usage

`DeleteModels [options]`

### Options

- `-t` or `--tenantId`: The id (GUID) of your tenant or directory in Azure.
- `-c` or `--clientId`: The id (GUID) of an Azure Active Directory app registration. See [Create an app registration to use with Azure Digital Twins](https://docs.microsoft.com/en-us/azure/digital-twins/how-to-create-app-registration) for more information.
- `-h` or `--hostName`: The host name of your Azure Digital Twins service instance (no https prefix needed).

### Example usage

- Delete all models: `DeleteModels -t 42a9ff5e-dd3a-4a89-8d92-2a7eaadbfcaa -c 198783fb-4301-4983-a12f-4eefb696282d -h briancr-ms.api.wcus.digitaltwins.azure.net`
