# Microsoft Smart Places & Energy

This repo contains the code, configuration, and documentation for the Microsoft Smart Places & Energy ASPEN Project. 

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Required Software

1. Visual Studio 2022+
2. Visual Studio Code
3. Docker for Windows

### Recommended Software

1. Check out the recommended workspace VSCode extensions
1. [IotHubExplorer](https://github.com/Azure/azure-iot-explorer/releases)
1. [BeyondCompare](https://microsoft.service-now.com/sp?id=sc_cat_item&sys_id=63372f60db042414b720f3376896196e)

### Prerequisites

1. You must be a member of the `TM-SmartPlaces` Security group in MyAccess
2. Set up access to the `kv-mapped` keyvault
    1. Go to the [Azure portal](https://ms.portal.azure.com/#@microsoft.onmicrosoft.com/resource/subscriptions/736f9579-2beb-4266-89a7-e25e8277185f/resourceGroups/Mapped/providers/Microsoft.KeyVault/vaults/kv-mapped/access_policies)
    2. Navigate to the `SPE-Dev-Common` subscription
    3. Go to the `Mapped` resource group
    4. Select the `kv-mapped` keyvault
    5. Click on `Access Policies`
    6. Click `Add Access Policy`
    7. Select the `Key, Secret, & Certificate Management` Template
    8. Select your principal id.
    9. Click `Add`
    10. Click `Save`
3. Clone the following repo to your machine: [ASPEN](https://dev.azure.com/dynamicscrm/Solutions/_git/ASPEN)
4. Clone the following repo to your machine: [RealEstateCore/BRECK-DTDL](https://github.com/RealEstateCore/BRECK-DTDL)

> [!Important]
> Until DigitalTwins has completed the migration to DTDLv3 use commit `737a1c8b6395e5a707361e0c1a4d72b404927651`
> (No current trackable workitem from DT to link, but support should be privatePreview 7/15 - Charlie Lagervik is the PM)

5. Check out a subscription from the subscription library here: [Subscription Library](https://subscriptionlibrary.azure.net/#/). Make sure to **regularly** extend your subscription checkout to the maximum number of days.
6. Note the subscription id you checked out for later use.

### Installing

#### Create the Demo Environment

1. Open the ASPEN clone on your machine in Visual Studio code
2. Open a Powershell terminal window in VS Code
3. Run `cd infra/demo`
4. Run the following command:

```
  .\deployInfra.ps1 --environment "<env>" --location "<loc>" --subscriptionId "<subId>" --tenantId "<tenId>" --instanceName "<instName>" --ontologyRoot "<ontRoot>" *> out.log

  where:
    env: is a 2 letter code (i.e. the initials of your first and last name)
    loc: is an Azure region (i.e. eastus). Note that this must correspond to a region where Azure Digital Twins can be provisioned
    subId: is the subscription id you copied from the subscription library earlier
    tenId: is the Azure Active Directory tenant you are using. For Microsoft, this is '72f988bf-86f1-41af-91ab-2d7cd011db47'
    instName: is the name of the Smart Facilities instance you are creating. For now, keep this short (less than 8 characters) to avoid issues with names of sub-resources being too long
    ontRoot: is the path to the folder for the RealEstateCore/BRECK repo clone you created on your machine previously

  i.e.
  .\deployInfra.ps1 --environment "jb" --location "eastus" --subscriptionId "552fc293-0442-4311-8a96-ded956eb8369" --tenantId "72f988bf-86f1-41af-91ab-2d7cd011db47" --instanceName "sf01" --ontologyRoot "D:\repos\breck-dtdl" *> logs\tyangell\$((Get-date).tostring('yyMMdd.HHmm')).log

```

#### Deploy the Azure Functions

1. Open the `SmartFacilities.sln` in Visual Studio
2. Compile the solution
3. Right click on the `products\TwinIngestion` Project
4. Click `Publish`
5. Create new publish profile specific to your environment
  1. Select `Azure`
  1. Click `Next`
  1. Select `Azure Function App (Windows)`
  1. Click `Next`
  1. Login and navigate to your function app
  1. Click `Next`
  1. Select `Publish (generates pubxml file)`
  1. Click `Finish`
6. Click `Publish`

#### Running the Docker Containers Locally

1. Define Identity for container (Only need to create the App first time, only need to create a new client secret first time and every time the identity expires (default: 6 months))
  1. Go to the Azure portal
  1. Go to `Azure Active Directory`
  1. Click `Add`
  1. Select `App Registration`
  1. Define a `Name` for the app. ex: `<alias>-aspen`
  1. Click `Register`
  1. Make note of the `Application (client) ID` you will need this later
  1. Click `Certificates & Secrets`
> [Note]
> You make have to wait a few minutes for the identity to propagate if you're met with a `Not Found` summary
  1. Click `New client secret`
  1. Define a `Description` for the secret. ex: `My first day on Aspen!`
  1. Click `Add`
  1. Copy the `Value` to your clipboard - This value wont be visible after navigating away from this page
  1. Open the repo in VSCode 
  1. Navigate to `docker-compose.yml`
  1. Populate the secret wherever it says `AZURE_CLIENT_SECRET`
  1. Populate the application client id wherever it says `AZURE_CLIENT_ID`
  1. Update the `VaultUri` to match the one deployed by deployInfra.ps1
1. Run `.\.pipelines\build.ps1` or `dotnet build SmartFacilities.sln`
1. Right click on `.\docker-compose.yml`
1. Select `Compose Up`

## Running the Console Programs Locally

There are 3 Console Programs that are part of the Smart Facilities solution that allow a developer to run components locally more easily than trying to run the Azure Functions locally. They are in the **products** folder of the 
repo.

### SmartFacilitiesGateway.Console

This is the code that runs on the Edge Gateway to pull telemetry messages from a Redis Cache and push to an IotHub. `infra\demo\deployInfra.ps1` will handle setting the following dotnet user-secrets:

| User Secret Name | Value |
| --- | --- |
| RedisEndpoint | The connection string to the local redis cluster. Usually https://localhost |
| DeviceConnectionString | The connection string to the IotHub for the device created to represent the Gateway |

When you start this app, it will connect to the Redis Cache and wait for messages until you stop the program.

### TelemetryProcessor.Console

This app replicates the functionality deployed to an Azure function in Production. It pulls telemetry messages from the Ingress Event Hub and passes them to the Azure Digital Twins instance. `infra\demo\deployInfra.ps1` will handle setting the following dotnet user-secrets:

| User Secret Name | Value |
| --- | --- |
| AzureDigitalTwinsEndpoint | The url of the ADT Instance |
| TelemetryEventHubConnectionString | The connection string to the Ingress Event Hub Namespace |
| TelemetryEventHubName | The name of the Ingress Event Hub |
| StorageAccountName | The storage account which is used to track which messages have been processed by the event hub |

When you start this app, it will connect to the EventHub and wait for messages until you stop the program.

### TopologyProcessor.Console

This app replicates the functionality deployed to an Azure function in Production. It queries the Mapped source graph and creates or updates all the twins and relationships in the Azure Digital Twins instance. `infra\demo\deployInfra.ps1` will handle setting the following dotnet user-secrets:

| User Secret Name | Value |
| --- | --- |
| MappedToken | The auth token needed to connect to the Mapped instance |

When you start this app, it will connect to Mapped, query the entire twin graph, then update ADT. Depending on the size of the graph, the update process can take many hours to complete as the ADT twin update is limited to 50 requests per second.

## Viewing Azure Functions Logs

1. Go to the Azure Portal
2. Go to your resource group
3. Go to the Azure Functions Instance
4. Click `Functions` in the sidebar
5. Click on the name of the function you want to review logs for
6. Click `Monitor`
7. Click `Logs`

## Known Issues

1. Azure Digital Twins only allows a model to be loaded once. When the deployment script is run more than once, there may be a lot of failures at the end of the script saying that the models already exists. This can be safely ignored as long as you weren't trying to update existing models
2. If new model classifications are added to the Ontology, the deployInfra.ps1 script will need to be updated
3. The TwinIngestionHost Azure Function only runs at midnight. If you have deployed the function and don't see any logs right away, wait for midnight to pass

## End-to-end tests

None (yet)

## Unit tests

None (yet)

## Production Deployment

TBD

## Built With

Visual Studio 2022
Visual Studio Code

