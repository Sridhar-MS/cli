#!/usr/bin/env bash
#
# Copyright (c) .NET Foundation and contributors. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#

set -e

SOURCE="${BASH_SOURCE[0]}"
while [ -h "$SOURCE" ]; do # resolve $SOURCE until the file is no longer a symlink
  DIR="$( cd -P "$( dirname "$SOURCE" )" && pwd )"
  SOURCE="$(readlink "$SOURCE")"
  [[ "$SOURCE" != /* ]] && SOURCE="$DIR/$SOURCE" # if $SOURCE was a relative symlink, we need to resolve it relative to the path where the symlink file was located
done
DIR="$( cd -P "$( dirname "$SOURCE" )" && pwd )"

CLI_DIR=$DIR/cli
CLI_ROOT=$CLI_DIR/bin

export UNAME=$(uname)

say() {
    printf "%b\n" "dotnet_install_dnx: $1"
}

doInstall=true


if [ "$UNAME" == "Darwin" ]; then
    CLI_URL="https://dotnetcli.blob.core.windows.net/dotnet/beta/Binaries/Latest/dotnet-osx-x64.latest.tar.gz"
elif [ "$UNAME" == "Linux" ]; then
    CLI_URL="https://dotnetcli.blob.core.windows.net/dotnet/beta/Binaries/Latest/dotnet-ubuntu-x64.latest.tar.gz"
else
    error "unknown OS: $UNAME" 1>&2
    exit 1
fi    



say "Preparing to install CLI to $CLI_DIR"

if [ -e "$CLI_ROOT/dotnet" ] ; then    
    say "You already have the requested version."    
    doInstall=false    
else
    say "Local Version: Not Installed"
fi

if [ $doInstall = true ] ; then
    rm -rf $CLI_DIR

    mkdir -p $CLI_DIR
    curl -o $CLI_DIR/dotnet.tar.gz $CLI_URL --silent    
    tar -xzf "$CLI_DIR/dotnet.tar.gz" -C "$CLI_DIR"
fi


echo "Clearing nuget cache"
rm -rf ~/.dnx
rm -rf ~/.local/share/dnu/cache
rm -rf ~/.local/share/NuGet/v3-cache
rm -rf ~/.nuget
rm -rf /tmp/NuGetScratch

# Restore packages
echo "Restoring packages"
"$CLI_ROOT/corehost" "$CLI_ROOT/NuGet.CommandLine.XPlat.dll" restore --runtime "win7-x64" --runtime "osx.10.10-x64" --runtime "ubuntu.14.04-x64" --runtime "centos.7.1-x64" "$DIR/src/Microsoft.DotNet.Cli" 

