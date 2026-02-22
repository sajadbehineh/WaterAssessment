param(
    [ValidateSet("win-x64", "win-x86", "win-arm64")]
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$projectPath = Join-Path $PSScriptRoot "..\WaterAssessment\WaterAssessment.csproj"
$outputPath = Join-Path $PSScriptRoot "..\artifacts\$Runtime"

Write-Host "Publishing WaterAssessment for $Runtime (non-single-file, unpackaged, Windows App SDK self-contained) ..."

dotnet publish $projectPath `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    /p:WindowsAppSDKSelfContained=true `
    /p:DebugType=None `
    /p:DebugSymbols=false `
    -o $outputPath

Write-Host "Done. Output: $outputPath"
Write-Host "Zip this folder and send it to client."