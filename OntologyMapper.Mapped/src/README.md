# Microsoft.SmartPlaces.Facilities.OntologyMapper.Mapped

This library provides helpers useful for mapping the Mapped implementation Digital Twins Definition Language (DTDL) model to various other DTDL topologies. The Mapped mappings are embedded resources within this assembly, making it easy to use these transforms without recreating them from scratch.

The following mappings have been created and are being maintained:

| Resource Path | Input Ontologies | Input DTDL Version | Output Ontologies | Output DTDL Version |
| --- | --- | --- |
| Mappings\v0\BrickRec\mapped_json_v0_dtdlv2_Brick_1_3-Rec_4_0.json | Mapped | V0 (not strict DTDL compliance) | Brick 1.3, Rec 4.0 | v2 |
| Mappings\v0\BrickRec\mapped_json_v0_dtdlv3_Brick_1_3-Rec_4_0.json | Mapped | V0 (not strict DTDL compliance) | Brick 1.3, Rec 4.0 | v3 |
| Mappings\v0\Willow\mapped_json_v0_dtdlv2_Willow.json | Mapped | V0 (not strict DTDL compliance) | Willow | v2 |

### Class: MappedOntologyMappingLoader 

#### Implements

Microsoft.SmartPlaces.Facilities.OntologyMapper.IOntologyMappingLoader

#### Method: LoadOntologyMapping

##### Description

Loads an OntologyMapping into memory from aon of the embedded resource file (specified in the constructor)

##### Parameters

None

##### Returns

An ontology mapping

