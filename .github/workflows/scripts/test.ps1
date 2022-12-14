Set-StrictMode -Version latest
$ErrorActionPreference = "Stop"
Import-Module "$PSScriptRoot/common.psm1" -Force


foreach($solution in $(Get-Solutions)) {
    Write-Output "Testing '$solution' using dotnet command line." 

    push-location $(Split-Path $solution -Parent)
        Show-SDKs

        dotnet test  $solution --no-restore --no-build -c Release /bl:"$solution.test.binlog" "/flp1:errorsOnly;logfile=$solution.Errors.log" --collect:'XPlat Code Coverage'

        if (! $?) {
            $rawError = $(Get-Content -Raw "$solution.Errors.log")
            Write-Error "Failed to test $solution. Error: $rawError"
            pop-location
            throw
        }
    pop-location
}