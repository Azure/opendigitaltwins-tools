Set-StrictMode -Version latest
$ErrorActionPreference = "Stop"
Import-Module "$PSScriptRoot\Common.psm1" -Force
$root = Get-Root

function Test-NugetConfig
{
    param($root)

    $pathToNugetConfig = Join-Path $root\SmartPlaces.Facilities -ChildPath "NuGet.config"
    [xml]$nugetConfigFile = Get-Content $pathToNugetConfig
    if($($nugetConfigFile.configuration.packageSources.add.Count) `
       -and $($nugetConfigFile.configuration.packageSources.add.Count -gt 1))
        {
            throw "Nuget.config must have only 1 package source." 
        }
}

Test-NugetConfig $root

foreach($solution in $(Get-Solutions)) {
    Write-Output "Restoring '$solution' using dotnet command line." 

    push-location $(Split-Path $solution -Parent)
        Show-SDKs

        dotnet restore $solution /bl:"$solution.restore.binlog" "/flp1:errorsOnly;logfile=$solution.Errors.log"

        if (! $?) {
            $rawError = $(Get-Content -Raw "$solution.Errors.log")
            Write-Error "Failed to restore NuGet packages for $solution. Error: $rawError"
            pop-location
            throw
        }
    pop-location
}

Write-Output "Restored Packages: "
$(get-childitem -Path $root\SmartPlaces.Facilities\packages\ -Directory).FullName
