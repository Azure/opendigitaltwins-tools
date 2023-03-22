# Telemetry

## Overview

The Telemetry sample project is an demonstration of how to pull device telemetry off an Event Hub fed from IotHub and insert the telemetry into Azure Digital Twins translating the Mapped protobuf datatypes to fit the configured DTDL models.

## Prerequisites

1. An Azure Digital Twins Instance
2. An instance of Redis Cache running in the cloud
3. An Event Hub for telemetry
4. An IotHub with a Gateway device created on it, and configured to route messages to the Event Hub created above
5. A KeyVault
6. The Topology process must have previously run to ingest the topology from Mapped to Azure Digital Twins
7. (Optional) An App Insights Instance
8. Azure CLI installed on your computer

## Data Flow

1. The Smart Facilities Gateway app pushes telemetry to the IotHub
2. IotHub forwards the message to the Event Hub
3. This app picks up the message and extracts the contents
4. This app gets the mapping key from the contents
5. This app checks the cloud redis cache to get the mapping of the MappingKey to the Azure Digital Twins $dtId and ModelId
6. This app updates ADT with the value for the telemetry

## Configuring

### Environment Variables

The following environment variables need to be defined in order to run this code:

| Environment Variable Name | Required | Value Source | Example |
| --- | --- | --- | --- |
| KeyVaultEndpoint | ✔ | The Uri to the KeyVault instance where needed secrets are stored | https://&lt;yourkeyvaultname&gt;.vault.azure.net/ |
| AzureDigitalTwinsEndpoint | ✔ | The Uri of the ADT Instance to update | https://&lt;youradtname&gt;.api.&lt;yourregion&gt;.digitaltwins.azure.net |
| StorageAccountEndpoint | ✔ | The Uri of the storage account to be used for checkpointing when pulling events from the Event Hub | https://&lt;yourstorageaccountname&gt;.blob.core.windows.net |
| AppInsightsConnectionString |  | The connection string needed to locate the Application Insights instance for logs and metrics | InstrumentationKey=&lt;yourinstrumentationkey&gt;;IngestionEndpoint=https://&lt;checkregion&gt;.in.applicationinsights.azure.com/;LiveEndpoint=https://&lt;checkregion&gt;.livediagnostics.monitor.azure.com/ |


### KeyVault Secrets

The following secrets must be defined in the KeyVault in order to run this code:

| Secret Name | Value Source | Example |
| --- | --- | --- |
| RedisCacheConnectionString | The endpoint location of the local Redis cluster in the cloud that store the mapping from the MappingKey to the ADT Twin ID | &lt;yourcachename&gt;.redis.cache.windows.net,abortConnect=false,ssl=true,allowAdmin=true,password=&lt;yourpassword&gt; |
| {EventHubName}-PrimaryConnectionString | The primary connection string for the event hub | Endpoint=sb://&lt;yourEventHubName&gt;.servicebus.windows.net/;SharedAccessKeyName=ListenRule;SharedAccessKey=&lt;yourSharedAccessKey&gt;=;EntityPath=telemetry |

## Logging and Metrics

Logs and Metrics will appear in the Application Insights instance pointed to by the App Insights connection string
