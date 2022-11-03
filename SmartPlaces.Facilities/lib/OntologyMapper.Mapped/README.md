# OntologyMapper.Mapped

This library provides helpers useful for mapping the Mapped implementation Digital Twins Definition Language (DTDL) model to various other DTDL topologies. The Mapped mappings are embedded resources within this assembly, making it easy to use these transforms without recreating them from scratch.

The following mappings have been created and are being maintained:

| Resource Path | Input Ontologies | Input DTDL Version | Output Ontologies | Output DTDL Version | Notes |
| --- | --- | --- | --- | --- |
| Mappings\v0\BrickRec\mapped_json_v0_dtdlv2_Brick_1_3-Rec_4_0.json | Mapped | V0 (not strict DTDL compliance) | Brick 1.3, Rec 4.0 | v2 | Deprecated |
| Mappings\v0\BrickRec\mapped_json_v0_dtdlv3_Brick_1_3-Rec_4_0.json | Mapped | V0 (not strict DTDL compliance) | Brick 1.3, Rec 4.0 | v3 | Deprecated |
| Mappings\v0\Willow\mapped_json_v0_dtdlv2_Willow.json | Mapped | V0 (not strict DTDL compliance) | Willow | v2 | Deprecated |
| Mappings\v1\BrickRec\mapped_v1_dtdlv2_Brick_1_3-Rec_4_0.json | Mapped | V3 | Brick 1.3, Rec 4.0 | v2 | |
| Mappings\v1\Willow\mapped_v1_dtdlv2_Willow.json | Mapped | V3 | Willow | v2 | |

### MappedOntologyMappingLoader

This class enables selection and loading of the Ontology Mappings from the embedded Mapped resource files. When instantiating this class, specify the full path of the resource to be loaded. 
i.e.

``` csharp

var resourceManager = new MappedOntologyMappingLoader(logger, "Mappings.v0.BrickRec.mapped_json_v0_dtdlv2_Brick_1_3-REC_4_0.json");

```
**Implements**

OntologyMapper.IOntologyMappingLoader

**Method: LoadOntologyMapping**

*Description*

Loads an OntologyMapping into memory from an embedded resource file (specified in the constructor)

*Parameters*

None

*Returns*

An ontology mapping

