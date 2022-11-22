# Smart Facilities Gateway

## Overview
The Edge Gateway sample project is a demonstration of how to consume telemetry on an edge device from a Redis cache based feed from Mapped and pass it to an IoTHub for 
forwarding into Azure Digital Twins.

## Prerequisites

1. A Mapped Instance
2. An instance of Redis Cache running on the Edge
3. An IotHub with a Gateway device created on it
4. A KeyVault
5. Azure Active Directory App Identity
6. (Optional) An App Insights Instance

## Data Flow

1. The Mapped Gateway pushes telemetry to the Redis Cache
2. This app subscribes to changes to the contents of the Redis Cache
3. When a message is received, the content is converted from Protobuf to JSON
4. The message is forwarded to an IotHub for ingestion into Azure Digital Twins

## Configuring

The following environment variables need to be defined in order to run this code:

### Environment Variables

| Environment Variable Name | Required | Value Source | Example |
| --- | --- | --- | --- |
| KeyVaultEndpoint | ✔ | The Uri to the KeyVault instance where needed secrets are stored | https://&lt;yourkeyvaultname&gt;.vault.azure.net/ |
| RedisEndpoint | ✔ | The endpoint location of the local Redis cluster on the Edge device that receives the telemetry from the Mapped Gateway | cache:6379 |
| AppInsightsConnectionString |  | The connection string needed to locate the Application Insights instance for logs and metrics | InstrumentationKey=&lt;yourinstrumentationkey&gt;;IngestionEndpoint=https://&lt;checkregion&gt;.in.applicationinsights.azure.com/;LiveEndpoint=https://&lt;checkregion&gt;.livediagnostics.monitor.azure.com/ |

### KeyVault Secrets

The following secrets must be defined in the KeyVault in order to run this code:

| Secret Name | Value Source | Example |
| --- | --- | --- |
| DeviceConnectionString | The connection string for the device set up in the IotHub | HostName=&lt;youriothubname&gt;.azure-devices.net;DeviceId=&lt;yourdevicename&gt;;SharedAccessKey=&lt;yoursharedaccesskey&gt; |

## Logging and Metrics

Logs and Metrics will appear in the Application Insights instance pointed to by the App Insights connection string
