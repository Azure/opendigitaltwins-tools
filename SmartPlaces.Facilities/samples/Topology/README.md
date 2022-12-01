﻿# Topology

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
| AppInsightsConnectionString | The connection string needed to locate the Application Insights instance for logs and metrics | InstrumentationKey/=<yourinstrumentationkey>;IngestionEndpoint/=https://<checkregion>.in.applicationinsights.azure.com/;LiveEndpoint/=https://<checkregion>.livediagnostics.monitor.azure.com/ |
| MappedRootUrl | The root Url of the Mapped Instance | https://api.mapped.com/graphql |

### KeyVault Secrets

The following secrets must be defined in the KeyVault in order to run this code:

| Secret Name | Value Source | Example |
| --- | --- | --- |
| MappedToken | The Personal Access Token for the Mapped instance | <yourkey> |
| RedisCacheConnectionString | The endpoint location of the local Redis cluster in the cloud that store the mapping from the MappingKey to the ADT Twin ID | <yourcachename>.redis.cache.windows.net,abortConnect=false,ssl=true,allowAdmin=true,password=<yourpassword> |

## Security

1. Access to the Redis cache is granted via Managed Identity
2. Access to the App Insights instance is granted via a Managed Identity 
3. Access to the KeyVault is granted via a Managed Identity
4. Access to the ADT Instance is granted through a Managed Identity

## Logging and Metrics

Logs and Metrics will appear in the Application Insights instance pointed to by the App Insights connection string. For more information on how to configure the Application Insights SDK, please see: https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service

There are three common ways to monitor / debug the topology loading process:

1. If you deployed the [dashboards](../Deploy/deploy.ps1), you should be able to review various graphs to help identify issues. There are 4 types of graphs currently available:

    1. Ingestion Metrics (how many of each type were found in the source graph): 
        1. Number of Sites
        2. Number of Buildings
        3. Number of Twins by Building
        4. Number of Relationships by Building
    2. Twin Metrics
        1. Number of Twin creates succeeded by ModelId
        2. Number of Twin creates failed by ModelId
        3. Number of Throttled Twin creates by ModelId
        4. Number of Twin updates succeeded by ModelId
        5. Number of Twin updates failed by ModelId
        6. Number of Throttled Twin updates by ModelId
        7. Number of Twin updates unchanged by ModelId (due to no differences between source and target)
    3. Relationship Metrics
        1. Number of Relationship creates succeeded by RelationshipType
        2. Number of Relationship creates failed by RelationshipType
        3. Number of Throttled Relationship creates by RelationshipType
        4. Number of Relationship updates succeeded by RelationshipType
        5. Number of Relationship updates failed by RelationshipType
        6. Number of Throttled Relationship updates by RelationshipType
        7. Number of Relationship updates unchanged by RelationshipType (due to no differences between source and target)
    4. Ontology Mapping Metrics
        1. Mapping for Input DTMI not found: 
            - Counts the number of the Incoming DTMIs that do not exist in the Source Ontology file that is loaded. 
            - Entries here indicate that the topology program assemblies are out of sync with the source system's ontology
        2. Output Mapping for Input DTMI not found:
            - Counts the number of the Incoming DTMIs that are not mapped to output DTMIs in the mapping file
            - Entries here indicate that the ontology mapping from source to target is incomplete and should be updated. It is possible that the target ontology is a smaller set than the
              input ontology and some mappings may not be possible (or needed).
        3. Target DTMI not found:
            - An target dtmi entry exists in the mapping file which does not actually exist in the target ontology
        4. InputInterfaceNotFound not found in Model by InterfaceType
            - A source dtmi entry exists in the mapping file which does not actually exist in the source ontology
        5. Relationship Not Found in Model by RelationshipType
            - A relationship was found in the toplogy that is not found in the target ontology
        6. Duplicate property found in Model by PropertyName
            - A invalid model was found with multiple properties with the same name
        7. Invalid Target DTMI Mappings in MappingFile
            - The mapping file has a dtmi that is not in the target ontology
        8. Invalid Output DTMI by OutputDtmi
            - The mapping file has a malformed DTMI

        For any of of these issues which involve changes to the provided mapping files, you will need contribution privileges to the Azure\opendigitaltwins-tool repository to resolve. If you do not have permissions to contribute to this repo, please open an issue [here](https://github.com/Azure/opendigitaltwins-tools/issues)

2. You can look at application logs directly through your Application Insights logs with the following queries:

```

traces
| where customDimensions.CategoryName startswith 'Microsoft.SmartPlaces.Facilities.'

```

```

exceptions
| where customDimensions.CategoryName startswith 'Microsoft.SmartPlaces.Facilities.'

```

Verbosity of these logs can be configured in the [appsettings.json](../Topology/src/appsettings.json) file for the Topology app.

3. If you have deployed this sample code within an Azure Container Instance there are additional methods for accessing the logs.
    - Within the AzureContainerInstance resource in the Azure Portal you can click `Containers` > `Logs` to view the last few minutes of console logs from the Topology process. 
    - If you've connected the AzureContainerInstance to a LogAnalytics workspace you can query the same console logs given the following queries.

```

ContainerInstanceLog_CL
| where ContainerName_s contains("topology")
| order by TimeGenerated asc

```

If you want to see all the entries with exceptions:

```

ContainerInstanceLog_CL
| where ContainerName_s contains("topology")
| where Message contains("exception")
| order by TimeGenerated asc

```

If you want to see all the entries with the lines with the word total in them:

```

ContainerInstanceLog_CL
| where ContainerName_s contains("topology")
| where Message contains("total")
| order by TimeGenerated asc

```

