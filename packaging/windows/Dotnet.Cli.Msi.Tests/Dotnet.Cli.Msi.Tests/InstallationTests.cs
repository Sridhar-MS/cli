using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Dotnet.Cli.Msi.Tests
{
    public class UnitTests
    {
        // all the tests assume that the msi to be tested is available via environment variable %CLI_MSI%
        [Theory]
        [InlineData("")]
        [InlineData(@"%SystemDrive%\dotnet")]
        public void InstallTest(string installLocation)
        {
            string msiFile = Environment.GetEnvironmentVariable("CLI_MSI");
            installLocation = Environment.ExpandEnvironmentVariables(installLocation);
            string expectedInstallLocation = string.IsNullOrEmpty(installLocation) ?
                Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\dotnet") :
                installLocation;

            var msiMgr = new MsiManager(msiFile);

            // make sure that the msi is not already installed, if so the machine is in a bad state
            Assert.False(msiMgr.IsInstalled, "The dotnet CLI msi is already installed");

            msiMgr.Install(installLocation);
            Assert.True(msiMgr.IsInstalled);
            Assert.Equal(expectedInstallLocation, msiMgr.InstallLocation);
            Assert.True(Directory.Exists(expectedInstallLocation));

            msiMgr.UnInstall();
            Assert.False(msiMgr.IsInstalled);
            Assert.False(Directory.Exists(expectedInstallLocation));
        }
    }
}
