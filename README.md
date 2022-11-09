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

