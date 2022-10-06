# Smart Facilities

This folder contains the code, configuration, and documentation for the Smart Facilities Solution. This solution includes a few assemblies which provide basic functionality for loading a DTDL-based graph from one provider into another.

### Required Software

1. Visual Studio 2022+
2. Visual Studio Code

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

1. Clone the repo
2. Load the SmartPlaces\SmartFacilities.slm file into Visual Studio
3. Compile the Solution.

# Sample Projects

## Topology

The topology project is an example of a console application which reads in configuration specifying the source graph and the target Azure Digital Twins instance, then calls methods on the injected instances of IngestionProcessor to execute the load.

For more information about this sample project, see its [readme](.\products\topology\src\readme.md)