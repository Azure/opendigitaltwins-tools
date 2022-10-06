# Topology

## Overview
The Topology project is an example of how to pull the topology from a Mapped Instance and insert a matching topology into an instance of Azure Digital Twins

## Prerequisites

1. An instance of Mapped with a populated graph of objects
2. A Personal Access Token (PAT) for the Mapped instance
3. An Azure Digital Twins Instance
4. An instance of Redis Cache running in the cloud

## Data Flow

1. Topology is pulled from Mapped instance
2. Mapped instances are converted to the appropriate target ontology
3. Graph is loaded into Azure Digital Twins
4. A mapping from the Mapped mappingKey to the ADT Twin ID is added to the Redis Cache for each twin

## Configuring

### Environment Variables

The following environment variables need to be defined in order to run this code:

| Environment Variable Name | Value Source | Example |
| --- | --- | --- |
| KeyVaultEndpoint | The Uri to the KeyVault instance where needed secrets are stored | https://<yourkeyvaultname>.vault.azure.net/ |
| AzureDigitalTwinsEndpoint | The Uri of the ADT Instance to update | https://<youradtname>.api.<yourregion>.digitaltwins.azure.net |
| RedisCacheConnectionString | The endpoint location of the local Redis cluster in the cloud that store the mapping from the MappingKey to the ADT Twin ID | <yourcachename>.redis.cache.windows.net,abortConnect=false,ssl=true,allowAdmin=true,password=<yourpassword> |
| AppInsightsConnectionString | The connection string needed to locate the Application Insights instance for logs and metrics | InstrumentationKey/=<yourinstrumentationkey>;IngestionEndpoint/=https://<checkregion>.in.applicationinsights.azure.com/;LiveEndpoint/=https://<checkregion>.livediagnostics.monitor.azure.com/ |
| MappedRootUrl | The root Url of the Mapped Instance | https://api.mapped.com/graphql |

### KeyVault Secrets

The following secrets must be defined in the KeyVault in order to run this code:

| Secret Name | Value Source | Example |
| --- | --- | --- |
| MappedToken | The Personal Access Token for the Mapped instance | <yourkey> |

## Security

1. Access to the Redis cache is granted via Managed Identity
2. Access to the App Insights instance is granted via a Managed Identity 
3. Access to the KeyVault is granted via a Managed Identity
4. Access to the ADT Instance is granted through a Managed Identity
5. Access to the Storage Account is granted through a Managed Identity

## Logging and Metrics

Logs and Metrics will appear in the Application Insights instance pointed to by the App Insights connection string. For more information on how to configure the Application Insights SDK, please see: https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service
