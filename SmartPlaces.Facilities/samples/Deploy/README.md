# Deploy Samples

This sample is dedicated to demonstrating easy deployment of the sample projects provided. You're free to use these as a starting point, or to package the code in whatever manner works best for your solution. This is just a suggestion.

Each sample should have a flag dedicated to deploying it. Just append the relevant `-deploy<Thing>` flag and it will take care of the rest.

Open powershell and run the following command

```powershell
.\SmartPlaces.Facilities\samples\Deploy\deploy.ps1 -Subscription <name or id> -ResourceGroup <name> [deploy flags you'd like deployed]
```

## Topology

Appending `-deployTopology -appInsightsName <name> [-dashboardName <name>]` will add a dashboard which displays metrics emitted by the Topology sample to a connected ApplicationInsights instance.
