// -----------------------------------------------------------------------
// <copyright file="TwinMergeHelper.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IngestionManager.AzureDigitalTwins
{
    using System.Text.Json;
    using Azure;
    using Azure.DigitalTwins.Core;
    using IngestionManager.Extensions;

    internal class TwinMergeHelper
    {
        internal static bool TryCreatePatchDocument(BasicDigitalTwin existingDigitalTwin, BasicDigitalTwin newTwin, out JsonPatchDocument jsonPatchDocument)
        {
            jsonPatchDocument = new JsonPatchDocument();

            foreach (var propertyName in newTwin.Contents.Keys)
            {
                var newPropertyJson = JsonSerializer.SerializeToElement(newTwin.Contents[propertyName]);

                // See if the existing twin has the property
                if (existingDigitalTwin.Contents.TryGetValue(propertyName, out var existingPropertyValue))
                {
                    var existingPropertyJson = JsonSerializer.SerializeToElement(existingPropertyValue);

                    GetUpdates(jsonPatchDocument, "/" + propertyName, newPropertyJson, existingPropertyJson);
                }
                else
                {
                    // This is a new property
                    jsonPatchDocument.AppendAdd("/" + propertyName, newPropertyJson);
                }
            }

            return !jsonPatchDocument.IsEmpty();
        }

        private static void GetUpdates(JsonPatchDocument jsonPatchDocument, string propertyName, JsonElement newPropertyJson, JsonElement existingPropertyJson)
        {
            // If the kinds are the same, keep checking to see if there are other equalities
            if (newPropertyJson.ValueKind == existingPropertyJson.ValueKind)
            {
                switch (newPropertyJson.ValueKind)
                {
                    case JsonValueKind.String:
                        {
                            // Verify that the property value has changed
                            if (existingPropertyJson.ToString() != newPropertyJson.ToString())
                            {
                                jsonPatchDocument.AppendReplace(propertyName, newPropertyJson.ToString());
                            }

                            break;
                        }

                    case JsonValueKind.Number:
                        {
                            // Verify that the property value has changed
                            if (existingPropertyJson.ToString() != newPropertyJson.ToString())
                            {
                                // See why we always try decimal first, here: https://stackoverflow.com/questions/72597489/what-is-the-idiomatic-way-to-find-the-underlying-type-of-a-jsonelement-value
                                if (newPropertyJson.TryGetDecimal(out var result))
                                {
                                    jsonPatchDocument.AppendReplace(propertyName, result);
                                }
                                else
                                {
                                    jsonPatchDocument.AppendReplace(propertyName, newPropertyJson.ToString());
                                }
                            }

                            break;
                        }

                    case JsonValueKind.Object:
                        {
                            foreach (var prop in newPropertyJson.EnumerateObject())
                            {
                                // Skip the metadata property
                                if (prop.Name != "$metadata")
                                {
                                    if (existingPropertyJson.TryGetProperty(prop.Name, out var outProp))
                                    {
                                        GetUpdates(jsonPatchDocument, propertyName + "/" + prop.Name, prop.Value, outProp);
                                    }
                                }
                            }
                        }

                        break;

                    case JsonValueKind.Array:
                        {
                            foreach (var prop in newPropertyJson.EnumerateArray())
                            {
                                if (prop.TryGetProperty("name", out var propName))
                                {
                                    // If the property exists in the output array, update.
                                    if (existingPropertyJson.TryGetProperty(propName.ToString(), out var existingProp))
                                    {
                                        GetUpdates(jsonPatchDocument, propertyName + "/" + propName.ToString(), prop, existingProp);
                                    }
                                    else
                                    {
                                        jsonPatchDocument.AppendReplace(propertyName + "/" + propName.ToString(), prop.ToString());
                                    }
                                }
                            }
                        }

                        break;
                }
            }
            else
            {
                // The kind has changed. Do a replace
                jsonPatchDocument.AppendReplace(propertyName + "/" + propertyName, newPropertyJson);
            }
        }
    }
}
