param (
    $subscription,
    $resourceGroup,
    $applicationInsightsName,
    $dashboardName = 'Microsoft SmartPlaces Facilities Topology',
    [Switch]$deployTopology = $false
)
Set-StrictMode -Version latest
$ErrorActionPreference = 'Stop'
$root = Split-Path $(Split-Path $PSScriptRoot -Parent) -Parent

#####
# Login
#####
Write-Output "Make sure we are logged in to Azure"
try {
    $userPrincipalId = az ad signed-in-user show --query id --output tsv --only-show-errors

    if (!$userPrincipalId) {
        Write-Output "User not logged in. Logging in"
        az login --only-show-errors
        $userPrincipalId = az ad signed-in-user show --query id --output tsv --only-show-errors
    }
    else {
        Write-Output "User already logged in."
    }
}
catch {
    Write-Output "Error getting user id. Trying to log in again."
    az login --only-show-errors
    $userPrincipalId = az ad signed-in-user show --query id --output tsv --only-show-errors
}

#####
# Getting preexisitng resources
#####
$context = (az account show) | ConvertFrom-Json
if ($context.name -ne $subscription -and $context.id -ne $subscription) {
    Write-Output "Setting context to $subscription"
    $null = az account set --subscription $subscription
    $context = (az account show) | ConvertFrom-Json
}
Write-Output "Context: $($context.name)"

$rg = (az group show --name $resourceGroup) | ConvertFrom-Json
Write-Output "ResourceGroup: $($rg.name)"

#####
# Deploying bits relevant to the Topology Sample
#####
if($deployTopology) {
    Write-Output "Deploying topology resources"
    $topology = (az deployment group create --template-file "$root\samples\Deploy\sampleProjects\topology.bicep" --resource-group $resourceGroup `
                                            --parameters location=$($rg.location) `
                                                         appInsightsName=$($applicationInsightsName) `
                                                         dashboardName=$dashboardName) | ConvertFrom-Json
    Write-Output "-> $($topology.properties.outputs.dashboardName.value)"
}

write-host "Complete!"
