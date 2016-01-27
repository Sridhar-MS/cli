#
# Copyright (c) .NET Foundation and contributors. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#

$DnxDir = "$PSScriptRoot\dnx"
$DnxRoot = "$DnxDir\bin"
$DnxPackage = "dnx-coreclr-win-x64.1.0.0-rc1-update1.nupkg"
$DnxVersion = "1.0.0-rc1-16231"

$doInstall = $true

# check if the required dnx version is already downloaded
if ((Test-Path "$DnxRoot\dnx.exe")) {
    $dnxOut = & "$DnxRoot\dnx.exe" --version

    if ($dnxOut -Match $DnxVersion) {
        Write-Host "Dnx version - $DnxVersion already downloaded."
        
        $doInstall = $false
    }
}

if ($doInstall)
{
    # Download dnx to copy to stage2
    Remove-Item -Recurse -Force -ErrorAction Ignore $DnxDir
    mkdir -Force "$DnxDir" | Out-Null

    Write-Host "Downloading Dnx version - $DnxVersion."
    $DnxUrl="https://api.nuget.org/packages/$DnxPackage"
    Invoke-WebRequest -UseBasicParsing "$DnxUrl" -OutFile "$DnxDir\dnx.zip"

    Add-Type -Assembly System.IO.Compression.FileSystem | Out-Null
    [System.IO.Compression.ZipFile]::ExtractToDirectory("$DnxDir\dnx.zip", "$DnxDir")
}


# Restore packages
Write-Host "Restoring packages"
$sw = [Diagnostics.Stopwatch]::StartNew()
& "$DnxRoot\dnu" restore "$PSScriptRoot\src\Microsoft.DotNet.Cli" --quiet --runtime "win7-x64" --runtime "osx.10.10-x64" --runtime "ubuntu.14.04-x64" --runtime "centos.7.1-x64"
$sw.Stop()
$sw.Elapsed


