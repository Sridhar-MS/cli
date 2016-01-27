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

DNX_DIR=$DIR/dnx
DNX_ROOT=$DNX_DIR/bin

export UNAME=$(uname)

say() {
    printf "%b\n" "dotnet_install_dnx: $1"
}

doInstall=true

DNX_FEED="https://api.nuget.org/packages"
DNX_PACKAGE_VERSION="1.0.0-rc1-update1"
DNX_VERSION="1.0.0-rc1-16231"

if [ "$UNAME" == "Darwin" ]; then
    DNX_FLAVOR="dnx-coreclr-darwin-x64"
elif [ "$UNAME" == "Linux" ]; then
    DNX_FLAVOR="dnx-coreclr-linux-x64"
else
    error "unknown OS: $UNAME" 1>&2
    exit 1
fi    

DNX_URL="$DNX_FEED/$DNX_FLAVOR.$DNX_PACKAGE_VERSION.nupkg"

say "Preparing to install DNX to $DNX_DIR"
say "Requested Version: $DNX_VERSION"

if [ -e "$DNX_ROOT/dnx" ] ; then
    dnxOut=`$DNX_ROOT/dnx --version | grep '^ Version: ' | awk '{ print $2; }'`
    
    say "Local Version: $dnxOut"
    
    if [ $dnxOut =  $DNX_VERSION ] ; then
        say "You already have the requested version."
        
        doInstall=false
    fi
else
    say "Local Version: Not Installed"
fi

if [ $doInstall = true ] ; then
    rm -rf $DNX_DIR

    mkdir -p $DNX_DIR
    curl -o $DNX_DIR/dnx.zip $DNX_URL --silent
    unzip -qq $DNX_DIR/dnx.zip -d $DNX_DIR
    chmod a+x $DNX_ROOT/dnu $DNX_ROOT/dnx   
fi

echo "Clearing dnx cache at - ~/.dnx"
rm -rf ~/.dnx

# Restore packages
echo "Restoring packages"

"$DNX_ROOT/dnu" restore "$DIR/src/Microsoft.DotNet.Cli" --quiet --runtime "win7-x64" --runtime "osx.10.10-x64" --runtime "ubuntu.14.04-x64" --runtime "centos.7.1-x64"


