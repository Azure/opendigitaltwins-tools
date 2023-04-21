# OntologyMapper.Mapped

This library provides helpers useful for mapping the Mapped implementation Digital Twins Definition Language (DTDL) model to various other DTDL topologies. The Mapped mappings are embedded resources within this assembly, making it easy to use these transforms without recreating them from scratch.

This library works in conjunction with and depends on the `Microsoft.SmartPlaces.Facilities.OntologyMapper` library, that defines a generic DTDL ontology mapping format (`OntologyMapping`) and facilities for software to consume such mappings (`IOntologyMappingManager`).

The following mappings have been created and are being maintained:

| Resource Path | Input Ontologies | Input DTDL Version | Output Ontologies | Output DTDL Version | Notes |
| --- | --- | --- | --- | --- | --- |
| Mappings\v0\BrickRec\mapped_json_v0_dtdlv2_Brick_1_3-Rec_4_0.json | Mapped | V0 (not strict DTDL compliance) | Brick 1.3, Rec 4.0 | v2 | Deprecated |
| Mappings\v0\BrickRec\mapped_json_v0_dtdlv3_Brick_1_3-Rec_4_0.json | Mapped | V0 (not strict DTDL compliance) | Brick 1.3, Rec 4.0 | v3 | Deprecated |
| Mappings\v0\Willow\mapped_json_v0_dtdlv2_Willow.json | Mapped | V0 (not strict DTDL compliance) | Willow | v2 | Deprecated |
| Mappings\v1\BrickRec\mapped_v1_dtdlv2_Brick_1_3-Rec_4_0.json | Mapped | V3 | Brick 1.3, Rec 4.0 | v2 | |
| Mappings\v1\Willow\mapped_v1_dtdlv3_Willow.json | Mapped | V3 | Willow | v3 | |

### Usage Instructions

The `MappedOntologyMappingLoader` class enables loading of one of the above listed. An instance of such a loader is passed as input when constructing an `IOntologyMappingManager` instance, from which the mappings can subsequently be read. When instantiating this class, specify the full path of the resource to be loaded in dot notation, i.e.:

``` csharp
var loader = new MappedOntologyMappingLoader(logger, "Mappings.v0.BrickRec.mapped_json_v0_dtdlv2_Brick_1_3-REC_4_0.json");
var mappingManager = new OntologyMappingManager(loader);
var mappingExists = mappingManager.TryGetInterfaceRemapDtmi(someInputDtmi, someOutputDtmi);
```