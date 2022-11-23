//-----------------------------------------------------------------------
// <copyright file="IngestionProcessorBase.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager
{
    using System.Text.Json;
    using System.Threading.Tasks;
    using global::Azure.DigitalTwins.Core;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Metrics;
    using Microsoft.Azure.DigitalTwins.Parser;
    using Microsoft.Azure.DigitalTwins.Parser.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces;
    using Microsoft.SmartPlaces.Facilities.OntologyMapper;

    /// <summary>
    /// Abstract base class for loading a site graph from input source to output target.
    /// </summary>
    /// <typeparam name="TOptions">Anything that inherits from the base class of IngestionManagerOptions.</typeparam>
    public abstract class IngestionProcessorBase<TOptions> : IGraphIngestionProcessor
        where TOptions : IngestionManagerOptions
    {
        private readonly MetricIdentifier relationshipNotFoundInModelmetricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "RelationshipNotFoundInModel", Metrics.RelationshipTypeDimensionName);
        private readonly MetricIdentifier duplicateMappingPropertyFoundMetricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "DuplicateMappingPropertyFound", "PropertyName");
        private readonly MetricIdentifier inputInterfaceNotFoundMetricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "InputInterfaceNotFound", Metrics.InterfaceTypeDimensionName);
        private readonly MetricIdentifier invalidTargetDtmisMetricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "InvalidTargetDtmis");
        private readonly MetricIdentifier invalidOutputDtmiMetricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "InvalidOutputDtmi", Metrics.OutputDtmiTypeDimensionName);
        private readonly MetricIdentifier targetDtmiNotFoundMetricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "TargetDtmiNotFound", Metrics.InterfaceTypeDimensionName);
        private readonly MetricIdentifier outputMappingForInputDtmiNotFoundMetricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "OutputMappingForInputDtmiNotFound", Metrics.OutputDtmiTypeDimensionName);
        private readonly MetricIdentifier mappingForInputDtmiNotFoundMetricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "MappingForInputDtmiNotFound", Metrics.InterfaceTypeDimensionName);

        /// <summary>
        /// Initializes a new instance of the <see cref="IngestionProcessorBase{TOptions}"/> class.
        /// </summary>
        /// <param name="logger">Ingestion processor logger, for local logging.</param>
        /// <param name="inputGraphManager">Manager for the input data graph that this ingestion processor parses.</param>
        /// <param name="ontologyMappingManager">Manager mapping between ontologies used by the input and output graphs.</param>
        /// <param name="outputGraphManager">Manager for the output data graph that this ingestion processor writes to.</param>
        /// <param name="telemetryClient">Application Insights telemetry client for remote metrics tracking.</param>
        protected IngestionProcessorBase(ILogger<IngestionProcessorBase<TOptions>> logger,
                                        IInputGraphManager inputGraphManager,
                                        IOntologyMappingManager ontologyMappingManager,
                                        IOutputGraphManager outputGraphManager,
                                        TelemetryClient telemetryClient)
        {
            Logger = logger;
            TelemetryClient = telemetryClient;
            InputGraphManager = inputGraphManager;
            OntologyMappingManager = ontologyMappingManager;
            var doc = JsonDocument.Parse("{ \"$metadata\": {} }");
            EmptyComponentElement = doc.RootElement;
            TargetModelParser = new ModelParser();
            OutputGraphManager = outputGraphManager;
        }

        /// <summary>
        /// Gets ingestion processor local logger.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets Application Insights telemetry client.
        /// </summary>
        protected TelemetryClient TelemetryClient { get; }

        /// <summary>
        /// Gets ontology mapping manager.
        /// </summary>
        protected IOntologyMappingManager OntologyMappingManager { get; }

        /// <summary>
        /// Gets input graph manager.
        /// </summary>
        protected IInputGraphManager InputGraphManager { get; }

        /// <summary>
        /// Gets a JSON object with only an empty <c>$metadata</c> field, used to scaffold an empty DTDL Component in target twins.
        /// </summary>
        protected JsonElement EmptyComponentElement { get; }

        /// <summary>
        ///  Gets target model parser, used to read target ontology into memory.
        /// </summary>
        protected ModelParser TargetModelParser { get; }

        /// <summary>
        /// Gets output graph manager.
        /// </summary>
        protected IOutputGraphManager OutputGraphManager { get; }

        /// <summary>
        /// Gets in-memory representation of target ontology model (such that it can be
        /// queried for mapping validations).
        /// <br /><br />
        /// Because this value is determined in an async call, it cannot be called in the constructor,
        /// so we use the null-forgiving operator (null!) to tell the compiler that this is set later
        /// (in the Init method).
        /// </summary>
        protected IReadOnlyDictionary<Dtmi, DTEntityInfo> TargetObjectModel { get; private set; } = null!;

        /// <summary>
        /// This method initiates ingestion of all sites in the input graph.
        /// To be implemented by solutions utilizing this library.
        /// </summary>
        /// <param name="cancellationToken">Cancellation propagation token for interrupting the ingestion process.</param>
        /// <returns>An awaitable task.</returns>
        protected abstract Task GetSites(CancellationToken cancellationToken);

        /// <inheritdoc/>
        public async Task IngestFromApiAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting ingestion process");

            await Init(cancellationToken);

            await GetSites(cancellationToken);

            Logger.LogInformation("Completed ingestion process");
            await TelemetryClient.FlushAsync(CancellationToken.None);
        }

        private async Task Init(CancellationToken cancellationToken)
        {
            var targetModelList = await OutputGraphManager.GetModelAsync(cancellationToken);

            // Load the target model into the Model Parser, to make it possible to write queries against the model
            TargetObjectModel = await TargetModelParser.ParseAsync(targetModelList);

            // Validate the target map. Don't need to stop processing if there is an error, but results will show up in the logs
            if (!OntologyMappingManager.ValidateTargetOntologyMapping(TargetObjectModel, out var invalidTargets) && invalidTargets != null)
            {
                TelemetryClient.GetMetric(invalidTargetDtmisMetricIdentifier).TrackValue(invalidTargets.Count);

                foreach (var invalidTarget in invalidTargets)
                {
                    Logger.LogWarning("Invalid Target DTMI found: {invalidTarget}", invalidTarget);
                }
            }
        }

        /// <summary>
        /// Returns a DTMI from the input graph ontology, corresponding to a string representation, if one exists.
        /// </summary>
        /// <param name="interfaceType">Sought interface name.</param>
        /// <returns><c>DTMI</c> representation of said interface, if it exists; else null.</returns>
        protected Dtmi? GetInputInterfaceDtmi(string interfaceType)
        {
            Dtmi? dtmi = null;

            if (InputGraphManager.TryGetDtmi(interfaceType.ToString(), out var dtmiVal))
            {
                dtmi = new Dtmi(dtmiVal);
            }
            else
            {
                Logger.LogWarning("Mapping for interfaceType '{interfaceType}' not found in DTDL", interfaceType);
                TelemetryClient.GetMetric(inputInterfaceNotFoundMetricIdentifier).TrackValue(1, interfaceType);
            }

            return dtmi;
        }

        /// <summary>
        /// Get an output Relationship name and direction, after ontology mapping, corresponding to an input Relationship name.
        /// If no mapping is found, simply returns the input Relationship.
        /// </summary>
        /// <param name="inputRelationshipType">The sought input Relationship.</param>
        /// <returns>A <c>string,bool</c> tuple, where the <c>string</c> indicates output Relationship name, and the <c>bool</c> indicates whether or not the relationship direction is reversed after mapping compared to the input direction.</returns>
        protected Tuple<string, bool> GetOutputRelationshipType(string inputRelationshipType)
        {
            // If there is a remapping, use that. If not, assume the input and output mapping are the same
            if (OntologyMappingManager.TryGetRelationshipRemap(inputRelationshipType, out var outputRelationship) && outputRelationship != null)
            {
                return new Tuple<string, bool>(outputRelationship.OutputRelationship, outputRelationship.ReverseRelationshipDirection);
            }

            return new Tuple<string, bool>(inputRelationshipType, false);
        }

        /// <summary>
        /// Try to get the output DTMI, after ontology mapping, corresponding to an input DTMI.
        /// </summary>
        /// <param name="inputDtmi">The sought input DTMI.</param>
        /// <param name="outputDtmi">The corresponding output DTMI.</param>
        /// <returns><c>true</c> if a mapping could be found, in which case <paramref name="outputDtmi"/> will hold a result, else <c>false</c>, in which case <paramref name="outputDtmi"/> will be null.</returns>
        protected bool TryGetOutputInterfaceDtmi(Dtmi inputDtmi, out Dtmi? outputDtmi)
        {
            // Try to get the input DTMI from the output DTDL
            if (TargetObjectModel.TryGetValue(inputDtmi, out var dTEntityInfo))
            {
                outputDtmi = dTEntityInfo.Id;
                return true;
            }
            else
            {
                outputDtmi = null;
                DtmiRemap? dtmiRemap = null;
                try
                {
                    if (OntologyMappingManager.TryGetInterfaceRemapDtmi(inputDtmi, out dtmiRemap) && dtmiRemap != null)
                    {
                        outputDtmi = new Dtmi(dtmiRemap.OutputDtmi);
                        return true;
                    }
                }
                catch (ParsingException ex)
                {
                    if (dtmiRemap != null)
                    {
                        Logger.LogWarning(ex, "Output DTMI cannot be parsed: {invalidTarget}.", dtmiRemap.OutputDtmi);
                        TelemetryClient.GetMetric(invalidOutputDtmiMetricIdentifier).TrackValue(1, dtmiRemap.OutputDtmi);
                    }
                    else
                    {
                        Logger.LogWarning(ex, "Output DTMI is null for inputDtmi: {invalidTarget}.", inputDtmi);
                    }

                    return false;
                }
            }

            return false;
        }

        protected Dtmi? GetTwin(IDictionary<string, BasicDigitalTwin> twins,
                                JsonElement targetElement,
                                string basicDtId,
                                string interfaceType)
        {
            Dtmi? inputDtmi = GetInputInterfaceDtmi(interfaceType.ToString());

            if (inputDtmi != null)
            {
                if (TryGetOutputInterfaceDtmi(inputDtmi, out var outputDtmi) && outputDtmi != null)
                {
                    // Create a basic twin
                    var basicTwin = new BasicDigitalTwin
                    {
                        Id = basicDtId,

                        // model Id of digital twin
                        Metadata = { ModelId = outputDtmi.ToString() },
                    };

                    // Populate the content of the twin
                    var contentDictionary = new Dictionary<string, object>();

                    // Get the model needed
                    if (TargetObjectModel.TryGetValue(outputDtmi, out var model))
                    {
                        // Get a list of the properties of the model
                        foreach (var property in ((DTInterfaceInfo)model).Contents.Values.Where(v => v.EntityKind == DTEntityKind.Property || v.EntityKind == DTEntityKind.Component))
                        {
                            switch (property.EntityKind)
                            {
                                case DTEntityKind.Property:
                                    {
                                        AddProperty(targetElement, basicDtId, interfaceType, contentDictionary, property, outputDtmi.ToString());
                                        break;
                                    }

                                case DTEntityKind.Component:
                                    {
                                        AddComponent(targetElement, contentDictionary, property);
                                        break;
                                    }
                            }
                        }
                    }
                    else
                    {
                        Logger.LogWarning("Target DTMI: '{outputDtmi}' with InterfaceType: '{interfaceType}' not found in target model parser.", basicDtId, interfaceType);
                        TelemetryClient.GetMetric(targetDtmiNotFoundMetricIdentifier).TrackValue(1, interfaceType.ToString());
                    }

                    // Twins are required to have a name
                    if (!contentDictionary.TryGetValue("name", out var name))
                    {
                        contentDictionary.Add("name", "None");
                    }

                    basicTwin.Contents = contentDictionary;

                    twins.TryAdd(basicTwin.Id, basicTwin);
                }
                else
                {
                    Logger.LogWarning("Output mapping for input Dtmi: '{inputDtmi}' with InterfaceType: '{interfaceType}' to output Dtmi not found.", basicDtId, interfaceType);
                    TelemetryClient.GetMetric(outputMappingForInputDtmiNotFoundMetricIdentifier).TrackValue(1, inputDtmi.ToString());
                }

                return outputDtmi;
            }
            else
            {
                Logger.LogWarning("Mapping for input interface: '{inputDtmi}' with InterfaceType: '{interfaceType}' not found.", basicDtId, interfaceType);
                TelemetryClient.GetMetric(mappingForInputDtmiNotFoundMetricIdentifier).TrackValue(1, interfaceType.ToString());

                return null;
            }
        }

        protected void AddProperty(JsonElement targetElement, string basicDtId, string interfaceType, Dictionary<string, object> contentDictionary, DTContentInfo property, string outputDtmi)
        {
            // Find the property on the input type that matches the propertyName of this property
            if (targetElement.TryGetProperty(property.Name, out var propertyValue))
            {
                if (propertyValue.ValueKind != JsonValueKind.Null)
                {
                    contentDictionary.Add(property.Name, propertyValue);
                }
                else
                {
                    // Check to see if there are fields we should use to fill the output property with if the input property is null
                    if (OntologyMappingManager.TryGetFillProperty(outputDtmi, property.Name, out var fillProperty) && fillProperty != null)
                    {
                        // Loop through the list
                        foreach (var inputProperty in fillProperty.InputPropertyNames)
                        {
                            // See if the input element has a value for that property
                            if (targetElement.TryGetProperty(inputProperty, out var inputValue))
                            {
                                // Take the first one that is not null
                                if (inputValue.ValueKind != JsonValueKind.Null)
                                {
                                    contentDictionary.Add(property.Name, inputValue);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // See if there are any projections we need to make for the properties
            if (OntologyMappingManager.TryGetPropertyProjection(outputDtmi, property.Name, out var propertyProjection))
            {
                if (propertyProjection != null)
                {
                    foreach (var inputProperty in propertyProjection.InputPropertyNames)
                    {
                        // Get the value of the input property
                        if (targetElement.TryGetProperty(inputProperty, out var inputValue))
                        {
                            // If the output target is a collection, add the value to the target collection
                            if (propertyProjection.IsOutputPropertyCollection)
                            {
                                if (!contentDictionary.TryGetValue(propertyProjection.OutputPropertyName, out var outputProperty))
                                {
                                    var newProperty = new Dictionary<string, string>() { { inputProperty, inputValue.ToString() } };
                                    contentDictionary.Add(propertyProjection.OutputPropertyName, newProperty);
                                }
                                else
                                {
                                    if (outputProperty is Dictionary<string, string> coll)
                                    {
                                        if (!coll.TryAdd(inputProperty, inputValue.ToString()))
                                        {
                                            Logger.LogWarning("Duplicate target property in collection: '{outputPropertyName}' with InterfaceType: '{interfaceType}' for DTMI: '{dtId}'.", propertyProjection.OutputPropertyName, interfaceType, basicDtId);
                                            TelemetryClient.GetMetric(duplicateMappingPropertyFoundMetricIdentifier).TrackValue(1, propertyProjection.OutputPropertyName);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // If the output target is not a collection, add the value to the target
                                if (!contentDictionary.TryAdd(propertyProjection.OutputPropertyName, inputValue.ToString()))
                                {
                                    Logger.LogWarning("Duplicate target property: '{outputPropertyName}' with InterfaceType: '{interfaceType}' for DTMI: '{dtId}'.", propertyProjection.OutputPropertyName, interfaceType, basicDtId);
                                    TelemetryClient.GetMetric(duplicateMappingPropertyFoundMetricIdentifier).TrackValue(1, propertyProjection.OutputPropertyName);
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void AddComponent(JsonElement targetElement, Dictionary<string, object> contentDictionary, DTContentInfo property)
        {
            // Find the property on the input type that matches the propertyName of this component
            if (targetElement.TryGetProperty(property.Name, out var propertyValue) && propertyValue.ValueKind != JsonValueKind.Null)
            {
                contentDictionary.Add(property.Name, propertyValue);
            }
            else
            {
                // If there is a component field on the Target Model, and there is not input value, create an element with empty $metadata as components are not optional
                contentDictionary.Add(property.Name, EmptyComponentElement);
            }
        }

        protected void GetRelationship(IDictionary<string, BasicRelationship> relationships,
                                      string? sourceElementId,
                                      Dtmi? inputSourceDtmi,
                                      string? inputRelationshipType,
                                      string targetDtId,
                                      string targetInterfaceType)
        {
            // Get the Dtmi for the input Target entity
            Dtmi? targetInputDtmi = GetInputInterfaceDtmi(targetInterfaceType);

            Dtmi? outputSourceDtmi = null;

            if (inputSourceDtmi != null)
            {
                TryGetOutputInterfaceDtmi(inputSourceDtmi, out outputSourceDtmi);
            }

            if (targetInputDtmi != null)
            {
                // Now try to get the matching outputDtmi for the Target entity
                if (TryGetOutputInterfaceDtmi(targetInputDtmi, out var targetOutputDtmi) && targetOutputDtmi != null)
                {
                    if (!string.IsNullOrEmpty(inputRelationshipType))
                    {
                        // Get Output relationship
                        var outputRelationship = GetOutputRelationshipType(inputRelationshipType);

                        if (outputSourceDtmi != null && TargetObjectModel.TryGetValue(outputSourceDtmi, out var model))
                        {
                            var relationship = ((DTInterfaceInfo)model).Contents.FirstOrDefault(p => p.Value.EntityKind == DTEntityKind.Relationship && p.Value.Name == outputRelationship.Item1);
                            var relationshipId = outputRelationship.Item2 ? $"{targetDtId}-{sourceElementId}-{outputRelationship.Item1}" :
                                                                            $"{sourceElementId}-{targetDtId}-{outputRelationship.Item1}";

                            // Create a basic relationship
                            var basicRelationship = new BasicRelationship
                            {
                                SourceId = outputRelationship.Item2 ? targetDtId : sourceElementId,
                                TargetId = outputRelationship.Item2 ? sourceElementId : targetDtId,
                                Id = relationshipId,
                                Name = outputRelationship.Item1.ToString(),
                            };

                            relationships.TryAdd(basicRelationship.Id, basicRelationship);
                        }
                        else
                        {
                            Logger.LogWarning("Output relationship '{relationshipType}' not found in Target Model. Source Element Id: '{sourceElementId}', TargetInterfaceType: '{interfaceType}', TargetId: '{targetId}",
                                outputRelationship.Item1 ?? string.Empty,
                                sourceElementId ?? string.Empty,
                                targetInterfaceType,
                                targetDtId);

                            TelemetryClient.GetMetric(relationshipNotFoundInModelmetricIdentifier).TrackValue(1, outputRelationship.Item1 ?? "NotFound");
                        }
                    }
                    else
                    {
                        Logger.LogWarning("No relationship mapping found from input model to output model: Source Element Id: '{sourceElementId}',  RelationshipType: '{relationshipType}', TargetInterfaceType: '{interfaceType}', TargetId: '{targetId}",
                            sourceElementId ?? string.Empty,
                            inputRelationshipType ?? string.Empty,
                            targetInterfaceType,
                            targetDtId);

                        TelemetryClient.GetMetric(relationshipNotFoundInModelmetricIdentifier).TrackValue(1, inputRelationshipType ?? "NotFound");
                    }
                }
            }
        }
    }
}