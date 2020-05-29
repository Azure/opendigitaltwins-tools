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

# DTDL Validator sample

This project demonstrates use of the Azure Digital Twins DTDL parser. It uses the DTDL parser library to validate DTDL model code, for data description in IoT.

It demonstrates:
* Basic use of the DTDL parser for validation of DTDL
* Basic use of the object model to access information about DTDL content (see the interactive moduel, in particular the list and show/showinfo commands)

## Contents

<!--Outline the file contents of the repository. It helps users navigate the codebase, build configuration and any related assets.-->

| File/folder       | Description                                |
|-------------------|--------------------------------------------|
| `DTDLValidator`   | Sample source code.                        |
| `.gitignore`      | Define what to ignore at commit time.      |
| `CHANGELOG.md`    | List of changes to the sample.             |
| `CONTRIBUTING.md` | Guidelines for contributing to the sample. |
| `README.md`       | This README file.                          |
| `LICENSE`         | The license for the sample.                |

## Prerequisites

<!--Outline the required components and tools that a user might need to have on their machine in order to run the sample. This can be anything from frameworks, SDKs, OS versions or IDE releases.--> 

## Setup

<!--Explain how to prepare the sample once the user clones or downloads the repository. The section should outline every step necessary to install dependencies and set up any settings (for example, API keys and output folders).-->

The program is a command line application that can be used in normal or interactive mode.

In normal mode, specify:
* a file extension (-e, default json)
* a directory to search (-d, default '.')
* a recursive option that determines if the file search descends into subdirectories (-r, default true)

Interactive mode is entered with the -i option. Type help for information on interactive commands.

## Running the sample

Build the project and run the application from the command line.

You can also create a self-contained single-file .exe (no other files or installations required):

Run
```bash
dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true
```
in the root folder of the repo.

## Key concepts

<!--Provide users with more context on the tools and services used in the sample. Explain some of the code that is being used and how services interact with each other.--> 

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
