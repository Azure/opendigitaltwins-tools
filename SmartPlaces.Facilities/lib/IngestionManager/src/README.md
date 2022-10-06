# IngestionManager

This library provides a way to load an Azure Digital Twins instances with twins and relationships from another DTDL-based graph of twins.


## Interfaces and Classes
    
### Interface: IGraphIngestionProcessor

*Description*
Methods for ingesting a graph from a source graph and inserting into a target graph

**Method: IngestionFromApiAsync**

*Description*

Starts the Ingestion Process from a source to a target graph. The implementation of the derived class is responsible for exposing its configuration through its constructor.

*Parameters*

None

*Returns*

An awaitable task.

### Interface: IInputGraphManager

*Description*
Methods for accessing an input graph source

**Method: TryGetDtmi**

*Description*

Get a DTMI for an interfaceType

*Parameters*

| Name | Description |
| --- | --- |
| interfaceType | The name of the interface |
| dtmi | The found dtmi |

*Returns*

`true` if the DTMi is found, otherwise `false`

**Method: GetTwinGraphAsync**

*Description*

Loads a twin graph from a source based on a passed in graph query

*Parameters*

| Name | Description |
| --- | --- |
| query | A well-formed graph query |

*Returns*

A JsonDocument containing the results of the query

**Method: GetOrganizationQuery**

*Description*

Gets a graph query to return an organization

*Parameters*

None

*Returns*

A graph query string which can be passed to GetTwinsGraphAsync

**Method: GetBuildingsForSiteQuery**

*Description*

Gets a graph query to return all the buildings on a site

*Parameters*

None

*Returns*

A graph query string which can be passed to GetTwinsGraphAsync

**Method: GetFloorQuery**

*Description*

Gets a graph query to return all the floors for a building

*Parameters*

None

*Returns*

A graph query string which can be passed to GetTwinsGraphAsync

**Method: GetBuildingsThingsQuery**

*Description*

Gets a graph query to return all the things in a building

*Parameters*

None

*Returns*

A graph query string which can be passed to GetTwinsGraphAsync

**Method: GetPointsForThingQuery**

*Description*

Gets a graph query to return all the points for a thing

*Parameters*

None

*Returns*

A graph query string which can be passed to GetTwinsGraphAsync

### Interface: IInputGraphManagerOptions

*Description*
Configuration interface for the InputGraphManager

### Interface: IOutputGraphManager

*Description*

Methods for working with an output graph

**Method: GetModelAsync**

*Description*

Loads the model for a graph

*Parameters*

None

*Returns*

A collection of strings which describe the model used by the output graph

**Method: UploadGraphAsync**

*Description*

Loads the twins and relationships into an output graph

*Parameters*

None

*Returns*

None

### Interface: ITelemetryIngestionProcessor

*Description*

Methods for ingesting event data telemetry to a target

**Method: IngestFromEventHubAsync**

*Description*

Ingests the passed in data from Event Hub into a target

*Parameters*

| Name | Description |
| --- | --- |
| telemetryData | An instance of event hub event data |

*Returns*

None

### Interface: ITwinMappingIndexer

*Description*

Methods for working with a cache store of sourceIDs to Digital Twin Ids

**Method: UpsertTwinIndexAsync**

*Description*

Add or update a mapping to the cache

*Parameters*

| Name | Description |
| --- | --- |
| sourceId | The source device key from the source graph |
| twinId | The target twin id for the target graph |

*Returns*

None

**Method: GetTwinIndexAsync**

*Description*

Get a mapping from the cache for a passed in sourceId

*Parameters*

| Name | Description |
| --- | --- |
| sourceId | The source device key from the source graph |

*Returns*

The twinId for the target graph

### Class: IngestionManagerOptions

*Description*

An implementation of IIngestionManagerOptions which allows the consumer to specify the connection and configuration information needed to connect to the target Azure Digital Twins instance

*Properties*
| Name | Description |
| --- | --- |
| AzureDigitalTwinsEndpoint | The Url for the Azure Digital Twins instance to target. |
| StorageAccountEndpoint | The storage account to be used during the ingestion process. |
| MaxRetryAttempts | The number of times to retry create twin attempts per twin/relationship. |
| RetryDelayInMs | The delay in milliseconds between retry create twin attempts per twin/relationship. Defaults to 50ms. |
| AdtResource | Gets or sets the resource to be used when generating a Token for accessing Azure Digital Twins. This needs to be changed when working with non-public clouds. Defaults to https://digitaltwins.azure.net/.default. |
| MaxDegreeOfParallelism | Gets or sets the maximum number parallel threads to use when uploading to Azure Digital Twins. Defaults to 10 threads. |


### Class: IngestionProcessorBase

*Description*

Abstract Base class for loading a site graph from input source to output target. Implements IGraphIngestionProcessor

