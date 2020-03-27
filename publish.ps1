#!/usr/bin/env pwsh

    [CmdletBinding()]
Param(
    [Parameter(Mandatory = $true)]
    [String]$apiKey,

    [Parameter(Mandatory = $true)]
    [String]$packageVersion
)

$currentPath = (Get-Location -PSProvider FileSystem).ProviderPath

$packages = New-Object System.Collections.ArrayList
$packages.Add("DryIoc.Facilities.AutoTx") > $null
$packages.Add("DryIoc.Facilities.NHibernate") > $null
$packages.Add("DryIoc.Transactions") > $null

if ($packageVersion[0] -eq 'v')
{
    $packageVersion = $packageVersion.Substring(1)
}

# `dotnet build` must run before `dotnet publish` because GeneratePackageOnBuild in csproj forces not to build when running `dotnet publish` command
# https://github.com/dotnet/core/issues/1778
dotnet build -c Release "/property:Version=${packageVersion}"

dotnet publish --no-restore -c Release --no-build

Set-Location $currentPath
Set-Location build

foreach ($package in $packages)
{
	$packageFullName = "${package}.${packageVersion}.nupkg"

    Write-Host "Publishing ${packageFullName}"
    dotnet nuget push -k "${apiKey}" "${packageFullName}" --source "http://proget.scalesoft.cz/nuget/Nuget/"
}

Set-Location $currentPath
