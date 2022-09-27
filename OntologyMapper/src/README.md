# Microsoft.SmartPlaces.Facilities.OntologyMapper

This library provides helpers useful for mapping one Digital Twins Definition Language (DTDL) model to a different DTDL model. It defines a set of classes which allow the following types to be mapped:

1. Interfaces
1. Relationships
1. Property Projections
1. Backfilling of Properties

A json file can be created with collections of these types of mappings, and then that can be used in a process which uses one DTDL ontology as an input and outputs a different DTDL ontology.


## File Sections

### Header

The header section is for informational purposes designating the source and destination DTDLs. This is to provide an at-a-glance summary of what DTDLs this file maps.

**Input Ontologies**

A collection of what ontologies make up the input source for the conversion

| Field | Description |
| --- | --- |
| Name | The name of the ontology |
| Version | The version of the ontology |
| DtdlVersion | Which version of the DTDL language is currently in use for this provider |

**Output Ontologies**

A collection of what ontologies make up the output source for the conversion

| Field | Description |
| --- | --- |
| Name | The name of the ontology |
| Version | The version of the ontology |
| DtdlVersion | Which version of the DTDL language is currently in use for this provider |


### InterfaceRemaps

A collection of mappings from one interface name to another interface name. It is suggested that you only need to list mappings where the input DTMI is not the same as the output DTMI. Systems should assume that if a DTMI is not listed in the mappings, that the output DTMI will be the same as the input DTMI.

| Field | Description |
| --- | --- |
| InputDtmi | The name of the input DTMI |
| OutputDtmi | The name of the output DTMI |

### RelationshipRemaps

A collection of mappings from one relationship name to another relationship name. It is suggested that you only need to list mappings where the input relationship is not the same as the output relationship. Systems should assume that if a relationship is not listed in the mappings, that the output relationship will be the same as the input relationship.

| Field | Description |
| --- | --- |
| InputDtmi | The name of the input relationship |
| OutputDtmi | The name of the output relationship |

### PropertyProjections

A collection of mappings from one property name to another property name. It is suggested that you only need to list mappings where the input property is not the same as the output property. Systems should assume that if a property is not listed in the mappings, that the output property will be the same as the input property.

| Field | Description |
| --- | --- |
| OutputDtmiFilter | A regex filter which allows filtering so that the mapping is only applied to output property names which match the filter. ".*" to apply to all output properties |
| OutputPropertyName | The name of the output property to assign the input property to |
| InputPropertyNames | A collection of names of the input property to assign to the output property. Can only specify multiple input properties if the output property is a collection |
| IsOutputPropertyCollection | A flag which indicates whether or not the output property is a collection or not. This allows an input property which is not a collection to be assigned as an element in the output collection. |
| Priority | If there are multiple projections for a single output property based on different DtmiFilters, priority is taken into account in ascending order |

### FillProperties

A collection of mappings from one or more property names to another property name so that if the output property is a required value and the input fields might be empty, that alternative fields can be used to populate the output field. It is suggested that you only need to list mappings where the input property is not the same as the output property. Systems should assume that if a property is not listed in the mappings, that the output property will be the same as the input property. Implementations should respect the order in which the input properties are specified so that the first non-empty valued input property is assigned to the output property

| Field | Description |
| --- | --- |
| OutputDtmiFilter | A regex filter which allows filtering so that the mapping is only applied to output property names which match the filter. ".*" to apply to all output properties |
| OutputPropertyName | The name of the output property to assign the input property to |
| InputPropertyNames | A collection of names of the input property to assign to the output property. Can only specify multiple input properties if the output property is a collection |
| Priority | If there are multiple fillproperties for a single output property based on different DtmiFilters, priority is taken into account in ascending order |

## Sample File

``` json
  "Header": {
    "InputOntologies": [
      {
        "Name": "input-ontology-name",
        "Version": "1.0",
        "DtdlVersion": "v2"
      }
    ],
    "OutputOntologies": [
      {
        "Name": "output-ontology-name",
        "Version": "2.0",
        "DtdlVersion": "v3"
      }
    ]
  },
  "InterfaceRemaps": [
    {
      "InputDtmi": "dtmi:source-namespace:source-interface-name;1",
      "OutputDtmi": "dtmi:target-namespace:target-interface;1"
    },
  ],
  "RelationshipRemaps": [
    {
      "InputRelationship": "source-relationship-name",
      "OutputRelationship": "target-relationship-name"
    }
  ],
  "PropertyProjections": [
    {
      "OutputDtmiFilter": ".*",
      "OutputPropertyName": "output-property-name",
      "InputPropertyNames": [ "input-property-name1", "input-property-name2" ],
      "IsOutputPropertyCollection": true
    },
    {
      "OutputDtmiFilter": "\w*Space\w*",
      "OutputPropertyName": "output-property-name",
      "InputPropertyNames": [ "input-property-name2" ],
      "IsOutputPropertyCollection": true
    }

  ],
  "FillProperties": [
    {
      "OutputDtmiFilter": ".*",
      "OutputPropertyName": "output-property-name",
      "InputPropertyNames": [ "input-property-name1, input-property-name2" ]
    },
    {
      "OutputDtmiFilter": "\w*Space\w*",
      "OutputPropertyName": "output-property-name",
      "InputPropertyNames": [ "input-property-name2, input-property-name1" ]
    }
  ]
}
```

## Interfaces and Classes
    
### Interface: IOntologyMappingManager

**Method: ValidateTargetOntologyMapping**

*Description*

Validates that all Output DTMIs listed in the Interface Remaps exist in the target object model

*Parameters*

| Name | Description |
| --- | --- |
| targetObjectModel | A dictionary of DTMI to DTEntityInfo mappings which are valid in the target ontology |
| invalidTargets | An output listing of invalid output mappings in the InterfaceRemaps |

*Returns*

`true` if all targets are valid, otherwise `false`</returns>

**Method: TryGetInterfaceRemapDtmi**

*Description*

For a given DTMI from the source ontology, get the DTMI for the target ontology

*Parameters*

| Name | Description |
| --- | --- |
| inputDtmi | The DTMI from the source ontology |
| dtmiRemap | An InterfaceRemap if it exists |

*Returns*

`true` if a remap exists, otherwise `false`</returns>

**Method: TryGetRelationshipRemap**

*Description*

For a given relationship from the source ontology, get the relationship for the target ontology

*Parameters*

| Name | Description |
| --- | --- |
| inputRelationship | The relationship from the source ontology |
| relationshipRemap | A relationshipRemap if it exists |

*Returns*

`true` if a remap exists, otherwise `false`</returns>

**Method: TryGetFillProperty**

*Description*

In some cases, the contents of one input property may need to be copied to multiple other fields in the target ontology. For instance, if the target ontology requires that the name field always be populated, but the source name field may be null and the description field be more reliable, a chain of fields can be set here so that there is a priority list of fields that will backfill the name field if the input name field is null.

*Parameters*

| Name | Description |
| --- | --- |
| outputDtmi | The output dtmi to which this search will be applied |
| outputPropertyName | The name of the output property |
| fillProperty | A fillProperty if it exists |

*Returns*

`true` if a fillProperty exists, otherwise `false`</returns>

**Method: TryGetPropertyProjection**

*Description*

In some cases, a property of the input model needs to be put into a different field or collection in the target model. A declaration can be made to map the input field to the appropriate output field

*Parameters*

| Name | Description |
| --- | --- |
| outputDtmi | The output dtmi to which this search will be applied |
| outputPropertyName | The name of the output property |
| propertyProjection | The property projection for the output property |

*Returns*

`true` if a propertyProjection exists, otherwise `false`</returns>

### Interface: IOntologyMappingLoader

**Method: LoadOntologyMapping**

*Description*

Loads an OntologyMapping into memory

*Parameters*

None

*Returns*

An ontology mapping

### Class: FileOntologyMappingLoader

**Implements**

Microsoft.SmartPlaces.Facilities.OntologyMapper.IOntologyMappingLoader

**Method: LoadOntologyMapping**

*Description*

Loads an OntologyMapping into memory from an input file (specified in the constructor)

*Parameters*

None

*Returns*

An ontology mapping
