# IngestionManager.Mapped

This library works in conjunction with and depends on the `Microsoft.SmartPlaces.Facilities.IngestionManager` library, that defines a interfaces and a generic implementation for ingesting building data graphs into Azure Digital Twins. 

## Setup

This library provides `IInputGraphManager` and `IGraphIngestionProcessor` implementations and a subclass of `IngestionManagerOptions` that together allow solutions to ingest graphs from the [Mapped](https://app.mapped.com/) API. 

Those implementations are wired up using the .Net dependency injection framework, as per the common design pattern:

```csharp
services.AddMappedIngestionManager(options =>
    {
        // Mapped Specific
        options.MappedToken = hostContext.Configuration["MappedToken"];
        options.MappedRootUrl = hostContext.Configuration["MappedRootUrl"];

        // Ingestion Manager
        options.AzureDigitalTwinsEndpoint = hostContext.Configuration["AzureDigitalTwinsEndpoint"];
    });
```

The above sets up singleton implementations such that any code depending on `IInputGraphManager` will call `MappedGraphManager`, and code depending on `IGraphIngestionProcessor` will call `MappedGraphIngestionProcessor`. 

Note that for ontology mappings to be resolved correctly, you may also want to wire up an ontology mapper instance:

```csharp
services.AddLogging();

services.AddSingleton<IOntologyMappingLoader>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<MappedOntologyMappingLoader>>();
    return new MappedOntologyMappingLoader(logger, hostContext.Configuration["ontologyMappingFilename"]);
});

services.AddSingleton<IOntologyMappingManager, OntologyMappingManager>();
```

## Usage

The entry point to using these libraries is the base class `IngestionProcessorBase.IngestFromApiAsync()` method; that method in turn calls the `MappedGraphIngestionProcessor.GetSites()` method, which initiates ingestion of all sites (e.g., campuses/buildings or other suitable starting nodes) from the input graph.

`GetSites()` calls out to an input graph manager to actually query the source graph for those starting nodes, and iterates over child nodes returned by said graph manager. As wired up above, that input graph manager will be our own `MappedGraphManager`, which knows how to talk to the Mapped API.

The `MappedGraphManager` is configured (e.g., for access credentials) using the options passed in to the `services.AddMappedIngestionManager()` call shown under Setup above.

So, assuming the Dependency Injection setup as given above, ingestion from Mapped to ADT is achieved in a class where the `MappedGraphIngestionProcessor` has been injected, as follows:

```csharp
logger.LogInformation("Starting to ingest topology");
await mappedProcessor.IngestFromApiAsync(cancellationToken);
logger.LogInformation("Topology ingestion completed");
```