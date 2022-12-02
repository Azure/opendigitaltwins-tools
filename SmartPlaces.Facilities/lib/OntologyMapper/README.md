# Ontology Mapper

## Motivation and purpose

The OntologyMapper allows developers to define conversion processes from one DTDL-based Ontology to a different DTDL-based Ontology.

This may be needed in cases where cloning a topology which uses a propriety ontology, and converting to an open standard ontology such as RealEstateCore.

These conversion rules are defined in a series of Mappings files here in the repo which are embedded in the assembly to make versioning easier.

The key is that these files describe exceptions only. If the source DTMI and Target DTMI are identical, nothing needs to appear for the mapping between the two. Thus, if an source and target use the same ontology and version, the mapping file will have very little content.

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


### NamespaceRemaps

A collection of mappings from one namespace name to another namespace name. This is used after the explicit InterfaceRemaps to execute a simple search and replace of a string in
an input model to convert it to an output model. That is, in those cases where the input model interface name is exactly the same as the output model interface name, but the namespace is different,
then the input model''s namespace will be replaced by the NamespaceRemap's output namespace.

| Field | Description |
| --- | --- |
| InputNamespace | The string to search for in the input dtmi |
| OutputNamespace | The replacement string |

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
| ReverseRelationshipDirection | The relationship direction can be reversed so that if the original relationship is from source to target, the new relationship will be from target to source |

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
  "NamespaceRemaps": [
    {
      "InputDtmi": "dtmi:source-namespace:",
      "OutputDtmi": "dtmi:target-namespace:"
    },
  ],
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
      "ReverseRelationshipDirection": true | false
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