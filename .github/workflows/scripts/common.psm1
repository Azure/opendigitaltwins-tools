Set-StrictMode -Version latest
$ErrorActionPreference = "Stop"
$root = Split-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) -Parent

function Get-Root {
    return $root
}

function Get-Solutions {
    $list = @(Get-ChildItem $root/SmartPlaces.Facilities -File -Recurse | Where-Object {($_.FullName -like "*.sln")})
    return $list
}

function Show-SDKs {
    Write-Output "Global.json contents:"
    Get-Content $root/SmartPlaces.Facilities/global.json
    Write-Output "Installed SDK versions:"
    dotnet --list-sdks
    Write-Output "Active SDK Version:"
    dotnet --version
    Write-Output "Active MSBuild Version:"
    dotnet msbuild -version
    Write-Output "Active Powershell Version:"
    Write-Output $PSVersionTable
}
