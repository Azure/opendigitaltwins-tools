# Smart Facilities

This folder contains the code, configuration, and documentation for the Smart Facilities Components. This includes several libraries that are created as Nuget Packages, and some sample code.

### Required Software

1. Visual Studio 2022+
2. Visual Studio Code

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

1. Clone the repo
3. Compile the Solution.

# Sample Projects

## Topology

The topology project is an example of a console application which reads in configuration specifying the source graph and the target Azure Digital Twins instance, then calls methods on the injected instances of IngestionProcessor to execute the load.

### Getting Started

1. Load the SmartPlaces.Facilities\samples\topology\topology.sln file into Visual Studio
2. Compile the solution
3. In order to run it, you must set up the appropriate infrastructure and configuration. For more information about this sample project, see its [readme](.\products\topology\src\readme.md)