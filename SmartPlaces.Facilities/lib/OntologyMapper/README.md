# Ontology Mapper

## Motivation and purpose

The OntologyMapper allows developers to define conversion processes from one DTDL-based Ontology to a different DTDL-based Ontology.

This may be needed in cases where cloning a topology which uses a propriety ontology, and converting to an open standard ontology such as Brick or RealEstateCore.

These conversion rules are defined in a series of Mappings files here in the repo, which are currently embedded in the assembly to make versioning easier.

The key is that these files describe exceptions only. If the source DTMI and Target DTMI are identical, nothing needs to appear for the mapping between the two. Thus, if an source and target use the same ontology and version, the mapping file will have very little content.

## Mappings Folder

The Mappings folder is a hierarchy which goes from the Input Ontology, format, and version, to the output ontology. A version of the file should exist for each combination of versions of the source to target ontology for each combination actually in use. 

When loading

## Mapping Classes

### OntologyMapping

- Defines the structure of the files stored in the Mappings folder for deserialization. The structure is defined [here] (./src/OntologyMapping.cs)

Includes the following elements
    
#### MappingHeader

    - Information regarding the contents of the file for reference purposes

##### Sample MappingHeader

``` json

  "Header": {
    "InputOntologies": [
      {
        "Name": "mapped",
        "Version": "1.0",
        "DtdlVersion": "v0"
      }
    ],
    "OutputOntologies": [
      {
        "Name": "brick",
        "Version": "1.3",
        "DtdlVersion": "v2"
      },
      {
        "Name": "rec",
        "Version": "4.0",
        "DtdlVersion": "v2"
      }
    ]
  }

``` 

#### InterfaceRemaps

    - Describes the translation from an input DTMI to an output DTMI. Note that only mappings where the input DTMI does not exactly match the output DTMI should be defined here. If the two DTMIs match exactly, they do not need to be added        

##### Sample InterfaceRemaps

``` json

"InterfaceRemaps": [
    {
      "InputDtmi": "dtmi:mapped:core:AblutionsRoom;1",
      "OutputDtmi": "dtmi:org:w3id:rec:ShowerRoom;1"
    },
    {
      "InputDtmi": "dtmi:mapped:core:AbsorptionChiller;1",
      "OutputDtmi": "dtmi:org:brickschema:schema:Brick:Absorption_Chiller;1"
    },
    {
      "InputDtmi": "dtmi:mapped:core:AccelerationTimeSetpoint;1",
      "OutputDtmi": "dtmi:org:brickschema:schema:Brick:Acceleration_Time_Setpoint;1"
    },
    {
      "InputDtmi": "dtmi:mapped:core:AccessActivityStatus;1",
      "OutputDtmi": "dtmi:org:brickschema:schema:Brick:Status;1"
    },
    {
      "InputDtmi": "dtmi:mapped:core:AccessCardReader;1",
      "OutputDtmi": "dtmi:org:brickschema:schema:Brick:Access_Card_Reader;1",
    }
]

```

#### RelationshipRemaps

    - Describes the translation from an input relationship type to an output relationship type

##### Sample RelationshipRemaps

``` json

 "RelationshipRemaps": [
    {
      "InputRelationship": "floors",
      "OutputRelationship": "hasPart"
    },
    {
      "InputRelationship": "hasPart",
      "OutputRelationship": "isLocationOf"
    }
  ]

```

#### PropertyProjections

    - In some cases, a property of the input model needs to be put into a different field or collection in the target model. A declaration can be made to map the input field to the appropriate output field

##### Sample PropertyProjections

``` json

  "PropertyProjections": [
    {
      "OutputDtmiFilter": ".*",
      "OutputPropertyName": "externalIds",
      "InputPropertyName": "mappingKey",
      "IsOutputPropertyCollection": true
      "Priority": 1
    }
  ]

```

#### FillProperties

    - If the target ontology requires that a field always be populated, but the source field may be null and a different field be more reliable, a chain of fields can be set here so that there is a priority list of fields that will backfill the output field if the original input name field is null.

##### Sample FillProperties

``` json

"FillProperties": [
    {
      "OutputDtmiFilter": ".*",
      "OutputPropertyName": "name",
      "InputPropertyNames": [ "name", "description" ]
      "Priority": 1
    }
  ]

```

### OntologyMappingManager

This class defines an implementation of the IOntologyMappingManager, and provides simplified calls for clients to make to use the OntologyMapping

### MappedOntologyMappingLoader

This class enables selection and loading of the Ontology Mappings from the embedded Mapped resource files. When instantiating this class, specify the full path of the resource to be loaded. 
i.e.

``` csharp

var resourceManager = new MappedOntologyMappingLoader(logger, "Mappings.v0.BrickRec.mapped_json_v0_dtdlv2_Brick_1_3-REC_4_0.json");

```

### FileOntologyMappingLoader

This class enables selection and loading of the Ontology Mappings from an external json file. When instantiating this class, specify the full path of the file to be loaded. 
i.e.

``` csharp

var fileLoader = new FileOntologyMappingLoader(logger, "c:\mappings\mapped_json_v0_dtdlv2_Brick_1_3-REC_4_0.json");

```