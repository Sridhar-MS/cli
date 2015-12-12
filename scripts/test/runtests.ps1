#
# Copyright (c) .NET Foundation and contributors. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#

. "$PSScriptRoot\..\_common.ps1"

$TestProjects = @(
    "E2E",
    "Microsoft.DotNet.Tools.Publish.Tests"
)

# Publish each test project
$TestProjects | ForEach-Object {
    dotnet publish --framework "dnxcore50" --runtime "$Rid" --output "$RepoRoot\artifacts\tests" --configuration "$Configuration" "$RepoRoot\test\$_"
    if (!$?) {
        Write-Host Command failed: dotnet publish --framework "dnxcore50" --runtime "$Rid" --output "$RepoRoot\artifacts\tests" --configuration "$Configuration" "$RepoRoot\test\$_"
        exit 1
    }
}

## Temporary Workaround for Native Compilation
## Need x64 Native Tools Dev Prompt Env Vars
## Tracked Here: https://github.com/dotnet/cli/issues/301
pushd "$env:VS140COMNTOOLS\..\..\VC"
cmd /c "vcvarsall.bat x64&set" |
foreach {
  if ($_ -match "=") {
    $v = $_.split("=", 2); set-item -force -literalpath "ENV:\$($v[0])" -value "$($v[1])"
  }
}
popd

# copy TestProjects folder which is used by the test cases
mkdir -Force "$RepoRoot\artifacts\tests\TestProjects"
cp -rec -Force "$RepoRoot\test\TestProjects\*" "$RepoRoot\artifacts\tests\TestProjects"

$failCount = 0
pushd "$RepoRoot\artifacts\tests"

# Run each test project
$TestProjects | ForEach-Object {	
    & "corerun.exe"  "xunit.console.netcore.exe" "$_.dll" -xml "$_.xml"
	$failCount += $LastExitCode
}

popd

Exit $failCount
