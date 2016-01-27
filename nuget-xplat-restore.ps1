#
# Copyright (c) .NET Foundation and contributors. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#

$CLIDir = "$PSScriptRoot\cli"
$CLIRoot = "$CLIDir\bin"
$CLIZip = "https://dotnetcli.blob.core.windows.net/dotnet/beta/Binaries/Latest/dotnet-win-x64.latest.zip"


$doInstall = $true

# check if the required dnx version is already downloaded
if ((Test-Path "$CLIRoot\dotnet.exe")) {
        Write-Host "Dotnet CLI already downloaded."        
        $doInstall = $false
}

if ($doInstall)
{
    # Download dnx to copy to stage2
    Remove-Item -Recurse -Force -ErrorAction Ignore $CLIDir
    mkdir -Force "$CLIDir" | Out-Null

    Write-Host "Downloading Dotnet CLI version."
    $Url = "https://dotnetcli.blob.core.windows.net/dotnet/beta/Binaries/Latest/dotnet-win-x64.latest.zip"
    Invoke-WebRequest -UseBasicParsing "$Url" -OutFile "$CLIDir\dotnet.zip"

    Add-Type -Assembly System.IO.Compression.FileSystem | Out-Null
    [System.IO.Compression.ZipFile]::ExtractToDirectory("$CLIDir\dotnet.zip", "$CLIDir")
}


# Restore packages
Write-Host "Restoring packages"
$sw = [Diagnostics.Stopwatch]::StartNew()
$env:DOTNET_HOME="$CLIDir"
& "$CLIRoot\corehost.exe" "$CLIRoot\NuGet.CommandLine.XPlat.dll" restore  --runtime "win7-x64" --runtime "osx.10.10-x64" --runtime "ubuntu.14.04-x64" --runtime "centos.7.1-x64" "$PSScriptRoot\src\Microsoft.DotNet.Cli"
$sw.Stop()
$sw.Elapsed


