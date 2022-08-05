# Ontology Mapper

## Motivation and purpose

The OntologyMapper allows developers to define conversion processes from one DTDL-based Ontology to a different DTDL-based Ontology.

This may be needed in cases where cloning a topology which uses a propriety ontology, and converting to an open standard ontology such as Brick or RealEstateCore.

These conversion rules are defined in a series of Mappings files here in the repo, which are currently embedded in the assembly to make versioning easier.

The key is that these files describe exceptions only. If the source DTMI and Target DTMI are identical, nothing needs to appear for the mapping between the two. Thus, if an source and target use the same ontology and version, the mapping file will have very little content.

## Mappings Folder

The Mappings folder is a hierarchy which goes from the Input Ontology, format, and version, to the output ontology. A version of the file should exist for each combination of versions of the source to target ontology for each combination actually in use. 

The name of the file is currently important, as it is used as part of the start up configuration for processes using these files, so changing the name (or having duplicate names anywhere in the assembly), will cause runtime errors.

## Mapping Classes

### OntologyMapping

This class defines the structure of the files stored in the Mappings folder for deserialization. The structure is defined [here] (./src/OntologyMapping.cs)

### OntologyMappingManager

This class defines an implementation of the IOntologyMappingManager, and provides simplified calls for clients to make to use the OntologyMapping