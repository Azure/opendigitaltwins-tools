# IngestionManager

This library, intended to work in conjunction with and depending on the `Microsoft.SmartPlaces.Facilities.OntologyMapper` library, provides interfaces and a partial implementation for ingesting existing building data graphs into Azure Digital Twins, populating the latter with twins and relationships sourced from the former.

## How to use

In order to use this library, you need to create at minimum two classes:

* A subclass of `IngestionProcessorBase` (implementing `IGraphIngestionProcessor`)
* An implementation of `IInputGraphManager`

The `IngestionProcessorBase` subclass must provide an implementation of the abstract method `ProcessSites()`. That method should initiate ingestion of all sites (e.g., campuses/buildings or other suitable starting nodes) from the input graph, traverse them and their child nodes (buildings, floors, rooms, etc), translating them as necessary, and storing them in the output Azure Digital Twins graph. 

In order to aid in this process, the `IngestionProcessorBase` class references implementations of `IInputGraphManager` and `IOutputGraphManager` that you may use to interact with the input and output graphs, respectively. An Azure Digital Twins-specific implementation of the latter is provided for you in `AzureDigitalTwinsGraphManager`, but you will need to create the former, adapted to whatever data source you are ingesting from.

Additionally, in order to reconcile any ontological differences between source and target graph, the `IngestionProcessorBase` provides convenience methods for mapping terms from the input to terms in the output (e.g., DTDL Interfaces, Relationships, etc.). These methods require that an implementation of `IOntologyMappingManager` from the aforementioned `OntologyMapper` library, loaded with a set of mappings between the used ontologies, be passed into to the `IngestionProcessorBase` constructor.

## Example

For an example of an implementation using this library, including the creation of those above-mentioned required implementations, see the `Microsoft.SmartPlaces.Facilities.IngestionManager.Mapped` package, which is built specifically to ingest graphs from the [Mapped](https://app.mapped.com/) API. 

The classes provided by this library are intended to be easily wired up using .Net Dependency Injection, and a `ServiceCollection` extension method `AddIngestionManager()` is provided for that express purpose. We recommend that any implementations that you build extend upon this design pattern, and use your own `ServiceCollection` extension method, which you use to wire up your `IInputGraphManager` and `IGraphIngestionProcessor` before then calling out to our extension method that wires up the `IOutputGraphManager`. This is exactly what the Mapped library does, i.e.:

```csharp
public static IServiceCollection AddMappedIngestionManager(this IServiceCollection services, Action<MappedIngestionManagerOptions> options)
    {
        services.AddOptions<MappedIngestionManagerOptions>()
                .Configure(options)
                .ValidateDataAnnotations()
                .ValidateOnStart();

        services.AddSingleton<IInputGraphManager, MappedGraphManager>();
        services.AddSingleton<IGraphIngestionProcessor, MappedGraphIngestionProcessor<MappedIngestionManagerOptions>>();

        services.AddIngestionManager(options);

        return services;
    }
```

## Deployment configuration

Once you have implemented `IngestionProcessorBase` and `IInputGraphManager`, and a ServiceCollection extension method to wire them up as described above, you are ready to use that extension method and your graph ingestion solution in a real system. 

Start by running that extension method to inject all the required interface implementations. Continuing with the Mapped library example, your configuration might look like this:

```csharp
services.AddLogging();

services.AddSingleton<IOntologyMappingLoader>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<MappedOntologyMappingLoader>>();
    return new MappedOntologyMappingLoader(logger, hostContext.Configuration["ontologyMappingFilename"]);
});

services.AddSingleton<IOntologyMappingManager, OntologyMappingManager>();

services.AddMappedIngestionManager(options =>
{
    // Mapped Specific
    options.MappedToken = hostContext.Configuration["MappedToken"];
    options.MappedRootUrl = hostContext.Configuration["MappedRootUrl"];

    // Ingestion Manager
    options.AzureDigitalTwinsEndpoint = hostContext.Configuration["AzureDigitalTwinsEndpoint"];
});
```

If all is configured correctly, your code referring to `IGraphIngestionProcessor` should inject your custom `IngestionProcessorBase` subclass, on which you can call the inherited method `IngestFromApiAsync()`. That method will initialise the base processor, and then immediately call your customized `ProcessSites()` method, kicking off the rest of the ingestion process.