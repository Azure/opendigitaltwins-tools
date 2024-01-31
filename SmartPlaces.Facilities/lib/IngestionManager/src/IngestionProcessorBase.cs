//-----------------------------------------------------------------------
// <copyright file="IngestionProcessorBase.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager
{
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using DTDLParser;
    using DTDLParser.Models;
    using global::Azure.DigitalTwins.Core;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Metrics;
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
        private readonly IGraphNamingManager graphNamingManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="IngestionProcessorBase{TOptions}"/> class.
        /// </summary>
        /// <param name="logger">Ingestion processor logger, for local logging.</param>
        /// <param name="inputGraphManager">Manager for the input data graph that this ingestion processor parses.</param>
        /// <param name="ontologyMappingManager">Manager mapping between ontologies used by the input and output graphs.</param>
        /// <param name="outputGraphManager">Manager for the output data graph that this ingestion processor writes to.</param>
        /// <param name="graphNamingManager">Manager for the naming of the elements of the graph.</param>
        /// <param name="telemetryClient">Application Insights telemetry client for remote metrics tracking.</param>
        protected IngestionProcessorBase(ILogger<IngestionProcessorBase<TOptions>> logger,
                                        IInputGraphManager inputGraphManager,
                                        IOntologyMappingManager ontologyMappingManager,
                                        IOutputGraphManager outputGraphManager,
                                        IGraphNamingManager graphNamingManager,
                                        TelemetryClient telemetryClient)
        {
            Logger = logger;
            TelemetryClient = telemetryClient;
            InputGraphManager = inputGraphManager;
            OntologyMappingManager = ontologyMappingManager;
            TargetModelParser = new ModelParser();
            OutputGraphManager = outputGraphManager;
            this.graphNamingManager = graphNamingManager;
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
        protected static JsonElement EmptyComponentElement { get => JsonDocument.Parse("{ \"$metadata\": {} }").RootElement; }

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
        protected abstract Task ProcessSites(CancellationToken cancellationToken);

        /// <summary>
        /// This method is called for each twin in the graph to add properties to the twin based on inputs.
        /// </summary>
        /// <param name="inputDtmi">The DTMI of the input twin.</param>
        /// <returns>A dictionary of strings and objects to add to the contents of the twin.</returns>
        protected virtual IDictionary<string, object> GetTargetSpecificContents(Dtmi inputDtmi)
        {
            return new Dictionary<string, object>();
        }

        /// <inheritdoc/>
        public async Task IngestFromApiAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting ingestion process");

            await Init(cancellationToken);

            await ProcessSites(cancellationToken);

            Logger.LogInformation("Completed ingestion process");
            await TelemetryClient.FlushAsync(CancellationToken.None);
        }

        private async Task Init(CancellationToken cancellationToken)
        {
            var targetModelList = await OutputGraphManager.GetModelAsync(cancellationToken);

            try
            {
                // Load the target model into the Model Parser, to make it possible to write queries against the model
                TargetObjectModel = TargetModelParser.Parse(targetModelList);

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
            catch (ParsingException ex)
            {
                Logger.LogError(ex, "Error parsing models: {errors}", ex.Errors);
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

        /// <summary>
        /// Adds a new digital twin to the input twins collection, named and typed per the
        /// input parameters, by parsing an input source element.
        /// <br/><br/>
        /// Note that this method will return a DTMI if ontology mapping can be carried out,
        /// irrespective of whether a new twin was successfully created or not.
        /// </summary>
        /// <param name="twins">Collection to which the new twin is added.</param>
        /// <param name="sourceElement">Element to be parsed in the input graph.</param>
        /// <param name="targetDtId">dtId for the new twin to add.</param>
        /// <param name="sourceTwinInterface">The interface of the source twin.</param>
        /// <returns>Target DTMI corresponding with <paramref name="sourceTwinInterface"/> after
        /// ontology mapping (or <c>null</c> if no such mapping could be found.</returns>
        protected Dtmi? AddTwin(IDictionary<string, BasicDigitalTwin> twins,
                                JsonElement sourceElement,
                                string targetDtId,
                                string sourceTwinInterface)
        {
            Dtmi? inputDtmi = GetInputInterfaceDtmi(sourceTwinInterface);

            if (inputDtmi != null)
            {
                if (TryGetOutputInterfaceDtmi(inputDtmi, out var outputDtmi) && outputDtmi != null)
                {
                    // Create a basic twin
                    var basicTwin = new BasicDigitalTwin
                    {
                        Id = targetDtId,

                        // model Id of digital twin
                        Metadata = { ModelId = outputDtmi.ToString() },
                    };

                    // Populate the content of the twin
                    var contentDictionary = new Dictionary<string, object>();

                    // Check to see if there are any custom properties that need to be added to the twin based on the input DTMI.
                    var targetSpecificContents = GetTargetSpecificContents(inputDtmi);

                    if (targetSpecificContents != null)
                    {
                        foreach (var content in targetSpecificContents)
                        {
                            contentDictionary.Add(content.Key, JsonSerializer.SerializeToDocument(content.Value).RootElement);
                        }
                    }

                    // Get the model needed
                    if (TargetObjectModel.TryGetValue(outputDtmi, out var model))
                    {
                        // Get a list of the properties of the model
                        foreach (var targetContentEntity in ((DTInterfaceInfo)model).Contents.Values.Where(v => v.EntityKind == DTEntityKind.Property || v.EntityKind == DTEntityKind.Component))
                        {
                            switch (targetContentEntity.EntityKind)
                            {
                                case DTEntityKind.Property:
                                    {
                                        AddProperty(sourceElement, targetDtId, sourceTwinInterface, contentDictionary, targetContentEntity, outputDtmi.ToString());
                                        break;
                                    }

                                case DTEntityKind.Component:
                                    {
                                        AddComponent(sourceElement, contentDictionary, targetContentEntity);
                                        break;
                                    }
                            }
                        }
                    }
                    else
                    {
                        Logger.LogWarning("Target DTMI: '{outputDtmi}' with InterfaceType: '{interfaceType}' not found in target model parser.", targetDtId, sourceTwinInterface);
                        TelemetryClient.GetMetric(targetDtmiNotFoundMetricIdentifier).TrackValue(1, sourceTwinInterface.ToString());
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
                    Logger.LogWarning("Output mapping for input Dtmi: '{inputDtmi}' with InterfaceType: '{interfaceType}' to output Dtmi not found.", targetDtId, sourceTwinInterface);
                    TelemetryClient.GetMetric(outputMappingForInputDtmiNotFoundMetricIdentifier).TrackValue(1, inputDtmi.ToString());
                }

                return outputDtmi;
            }
            else
            {
                Logger.LogWarning("Mapping for input interface: '{inputDtmi}' with InterfaceType: '{interfaceType}' not found.", targetDtId, sourceTwinInterface);
                TelemetryClient.GetMetric(mappingForInputDtmiNotFoundMetricIdentifier).TrackValue(1, sourceTwinInterface.ToString());

                return null;
            }
        }

        /// <summary>
        /// Parses a source JSON element and populates a provided Property declaration in the input content
        /// directory based on the structure of that source element.
        /// </summary>
        /// <param name="sourceElement">Element parsed from source graph.</param>
        /// <param name="basicDtId">dtID of target digital twin that the contents dictionary belongs to (used for logging).</param>
        /// <param name="interfaceType">Interface of the source digital twin (used for logging).</param>
        /// <param name="contentDictionary">The content dictionary to which the generated Property will be addded.</param>
        /// <param name="property">Property declaration on the target twin's Interface.</param>
        /// <param name="outputDtmi">Interface of the target digital twin (that the contents directory belongs to).</param>
        protected void AddProperty(JsonElement sourceElement, string basicDtId, string interfaceType, Dictionary<string, object> contentDictionary, DTContentInfo property, string outputDtmi)
        {
            // Do the transform first to see if there is a special mapping for this property
            var matchResult = OntologyMappingManager.TryGetObjectTransformation(outputDtmi, property.Name, out var objectTransformation);

            if (matchResult == ObjectTransformationMatch.PropertyAndTypeMatch)
            {
                Logger.LogInformation("Found object transformation for property: '{propertyName}' on interface: '{interfaceType}' for target DTMI: '{outputDtmi}'.", property.Name, interfaceType, outputDtmi);
                PerformObjectTransformation(sourceElement, basicDtId, interfaceType, contentDictionary, objectTransformation);
            }
            else if (matchResult == ObjectTransformationMatch.PropertyMatchOnly)
            {
                Logger.LogDebug("No object transformation found for property: '{propertyName}' on interface: '{interfaceType}' for target DTMI: '{outputDtmi}'. Checking ancestors.", property.Name, interfaceType, outputDtmi);

                var oDtmi = new Dtmi(outputDtmi);
                var hashSet = new HashSet<string>();
                var queue = new Queue<Dtmi>();

                GetParentModels(queue, hashSet, oDtmi);

                var dequeueSuccess = queue.TryDequeue(out var parent);

                // Walk the ancestor tree to find the first parent that has a mapping
                while (dequeueSuccess && parent != null)
                {
                    Logger.LogDebug("No object transformation found for property: '{propertyName}' on interface: '{interfaceType}' for target DTMI: '{outputDtmi}'. Checking parent: '{parentDtmi}'.", property.Name, interfaceType, outputDtmi, parent);
                    if (OntologyMappingManager.TryGetObjectTransformation(parent.ToString(), property.Name, out objectTransformation) == ObjectTransformationMatch.PropertyAndTypeMatch)
                    {
                        Logger.LogInformation("Found object transformation for property: '{propertyName}' on interface: '{interfaceType}' for target DTMI: '{outputDtmi}'.", property.Name, interfaceType, outputDtmi);
                        PerformObjectTransformation(sourceElement, basicDtId, interfaceType, contentDictionary, objectTransformation);

                        // Stop at the first match
                        break;
                    }
                    else
                    {
                        GetParentModels(queue, hashSet, parent);
                    }

                    dequeueSuccess = queue.TryDequeue(out parent);
                }
            }

            // Find the property on the input type that matches the propertyName of this property
            if (sourceElement.TryGetProperty(property.Name, out var propertyValue))
            {
                if (propertyValue.ValueKind != JsonValueKind.Null)
                {
                    // If the property already exists, we don't want to overwrite it
                    contentDictionary.TryAdd(property.Name, propertyValue);
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
                            if (sourceElement.TryGetProperty(inputProperty, out var inputValue))
                            {
                                // Take the first one that is not null
                                if (inputValue.ValueKind != JsonValueKind.Null)
                                {
                                    // If the property already exists, we don't want to overwrite it
                                    contentDictionary.TryAdd(property.Name, inputValue);
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
                        if (sourceElement.TryGetProperty(inputProperty, out var inputValue))
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

        private void PerformObjectTransformation(JsonElement sourceElement, string basicDtId, string interfaceType, Dictionary<string, object> contentDictionary, ObjectTransformation? objectTransformation)
        {
            if (objectTransformation != null)
            {
                Logger.LogInformation("Performing object transformation for property: '{outputPropertyName}' with InterfaceType: '{interfaceType}' for DTMI: '{dtId}'.", objectTransformation.OutputPropertyName, interfaceType, basicDtId);

                // Get the value of the input property
                if (sourceElement.TryGetProperty(objectTransformation.InputProperty, out var inputProperty))
                {
                    // Get the value of the input property
                    if (inputProperty.TryGetProperty(objectTransformation.InputPropertyName, out var inputPropertyValue))
                    {
                        if (!contentDictionary.TryAdd(objectTransformation.OutputPropertyName, inputPropertyValue.ToString()))
                        {
                            Logger.LogWarning("Duplicate target property: '{outputPropertyName}' with InterfaceType: '{interfaceType}' for DTMI: '{dtId}'.", objectTransformation.OutputPropertyName, interfaceType, basicDtId);
                            TelemetryClient.GetMetric(duplicateMappingPropertyFoundMetricIdentifier).TrackValue(1, objectTransformation.OutputPropertyName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Parses a source JSON element and populates a provided component declaration in the input content
        /// directory based on the structure of that source element.
        /// </summary>
        /// <param name="sourceElement">Element parsed from source graph.</param>
        /// <param name="contentDictionary">The content dictionary to which the generated component will be added.</param>
        /// <param name="component">Component declaration on the target twin's Interface.</param>
        protected void AddComponent(JsonElement sourceElement, Dictionary<string, object> contentDictionary, DTContentInfo component)
        {
            // Find the property on the input type that matches the propertyName of this component
            if (sourceElement.TryGetProperty(component.Name, out var propertyValue) && propertyValue.ValueKind != JsonValueKind.Null)
            {
                contentDictionary.Add(component.Name, propertyValue);
            }
            else
            {
                // If there is a component field on the Target Model, and there is not input value, create an element with empty $metadata as components are not optional
                contentDictionary.Add(component.Name, EmptyComponentElement);
            }
        }

        /// <summary>
        /// Adds a new relationship to the provided relationships collection, based on the provided  parameters
        /// and employing ontology mapping to translate DTMI of the relationship source/target twins' Interfaces,
        /// relationship name/direction, etc.
        /// </summary>
        /// <param name="relationships">Collection to which the new relationship is added.</param>
        /// <param name="sourceDtId">dtId of the input relationship source.</param>
        /// <param name="inputSourceDtmi">DTMI of the input relationship source's Interface.</param>
        /// <param name="inputRelationshipType">Input relationship name.</param>
        /// <param name="targetDtId">dtId of the input relationship target.</param>
        /// <param name="targetInterfaceType">DTMI of the input relationship target's Interface.</param>
        /// <param name="relationshipProperties">A dictionary of proporties for the relationship.</param>
        protected void AddRelationship(IDictionary<string, BasicRelationship> relationships,
                                      string sourceDtId,
                                      Dtmi? inputSourceDtmi,
                                      string? inputRelationshipType,
                                      string targetDtId,
                                      string targetInterfaceType,
                                      IDictionary<string, object> relationshipProperties)
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
                            var relationshipId = outputRelationship.Item2 ? graphNamingManager.GetRelationshipName(targetDtId, sourceDtId, outputRelationship.Item1, relationshipProperties) :
                                                                            graphNamingManager.GetRelationshipName(sourceDtId, targetDtId, outputRelationship.Item1, relationshipProperties);

                            // Create a basic relationship
                            var basicRelationship = new BasicRelationship
                            {
                                SourceId = outputRelationship.Item2 ? targetDtId : sourceDtId,
                                TargetId = outputRelationship.Item2 ? sourceDtId : targetDtId,
                                Id = relationshipId,
                                Name = outputRelationship.Item1.ToString(),
                            };

                            if (relationshipProperties != null)
                            {
                                foreach (var relationshipProperty in relationshipProperties)
                                {
                                    basicRelationship.Properties.Add(new KeyValuePair<string, object>(relationshipProperty.Key, relationshipProperty.Value));
                                }
                            }

                            relationships.TryAdd(basicRelationship.Id, basicRelationship);
                        }
                        else
                        {
                            Logger.LogWarning("Output relationship '{relationshipType}' not found in Target Model. Source Element Id: '{sourceElementId}', TargetInterfaceType: '{interfaceType}', TargetId: '{targetId}",
                                outputRelationship.Item1 ?? string.Empty,
                                sourceDtId ?? string.Empty,
                                targetInterfaceType,
                                targetDtId);

                            TelemetryClient.GetMetric(relationshipNotFoundInModelmetricIdentifier).TrackValue(1, outputRelationship.Item1 ?? "NotFound");
                        }
                    }
                    else
                    {
                        Logger.LogWarning("No relationship mapping found from input model to output model: Source Element Id: '{sourceElementId}',  RelationshipType: '{relationshipType}', TargetInterfaceType: '{interfaceType}', TargetId: '{targetId}",
                            sourceDtId ?? string.Empty,
                            inputRelationshipType ?? string.Empty,
                            targetInterfaceType,
                            targetDtId);

                        TelemetryClient.GetMetric(relationshipNotFoundInModelmetricIdentifier).TrackValue(1, inputRelationshipType ?? "NotFound");
                    }
                }
            }
        }

        private void GetParentModels(Queue<Dtmi> queue, HashSet<string> hashSet, Dtmi model)
        {
            if (model == null)
            {
                return;
            }

            var dtInterfaceInfo = TargetObjectModel.FirstOrDefault(p => p.Key == model && p.Value.EntityKind == DTEntityKind.Interface);

            if (dtInterfaceInfo.Value != null)
            {
                var p = dtInterfaceInfo.Value as DTInterfaceInfo;

                if (p != null)
                {
                    var extends = p.Extends.ToList();
                    if (extends.Any())
                    {
                        foreach (var dtInterface in extends)
                        {
                            if (!hashSet.Contains(dtInterface.Id.ToString()))
                            {
                                hashSet.Add(dtInterface.Id.ToString());
                                queue.Enqueue(dtInterface.Id);
                            }
                        }
                    }
                }
            }

            return;
        }
    }
}