# Smart Facilities

## Problem Statement and Solution
Facility Digitization Accelerator is designed to unlock facility-related operational and sustainability data at scale. Facilities contain numerous disparate, unnormalized Operational Technology systems (HVAC, lighting, power, etc.) often controlled by a Building Management System (BMS). Onboarding these systems into an IoT cloud solution is an arduous, manual, serialized task taking domain experts 1-3 months to classify and normalize the data. 

We are providing a provisioning tool which converts discovered building data from the BMS into a well-defined digital twin topology of the building using [RealEstateCore](https://github.com/RealEstateCore/REC), an open industry standard data model. By leveraging this tool, customers can now onboard their building and realize business value in 3-5 days or less.

## This project
This project contains the code, configuration, and documentation for the Smart Facilities Components. This includes several libraries that are created as Nuget Packages, and some sample code.

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
3. In order to run it, you must set up the appropriate infrastructure and configuration. For more information about this sample project, see its [readme](samples/Topology/src/README.md)