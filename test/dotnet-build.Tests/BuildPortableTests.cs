using System.IO;
using Microsoft.DotNet.Tools.Test.Utilities;
using Xunit;

namespace Microsoft.DotNet.Tools.Builder.Tests
{
    public class BuildPortableTests : TestBase
    {
        public string PortableApp { get; } = "PortableApp";
        public string PortableAppRoot { get; } = Path.Combine("PortableTests", PortableApp);
        public string KestrelPortableApp { get; } = "KestrelHelloWorldPortable";
        public string KestrelPortableAppRoot { get; } = Path.Combine("KestrelHelloWorld", KestrelPortableApp);
        
        [Fact]
        public void BuildingPortableAppProducesExpectedArtifacts()
        {
            var testInstance = TestAssetsManager.CreateTestInstance(PortableAppRoot)
                .WithLockFiles();

            BuildAndTest(testInstance.TestRoot);
        }
        
        [Fact]
        public void BuildingKestrelPortableFatAppProducesExpectedArtifacts()
        {
            var testInstance = TestAssetsManager.CreateTestInstance(KestrelPortableAppRoot)
                .WithLockFiles();

            BuildAndTest(testInstance.TestRoot);
        }
        
        private static void BuildAndTest(string testRoot)
        {
            string appName =  Path.GetFileName(testRoot);
            
            var result = new BuildCommand(
                projectPath: testRoot)
                .ExecuteWithCapturedOutput();

            result.Should().Pass();

            var outputBase = new DirectoryInfo(Path.Combine(testRoot, "bin", "Debug"));

            var netstandardappOutput = outputBase.Sub("netstandard1.5");

            netstandardappOutput.Should()
                .Exist().And
                .OnlyHaveFiles(new[]
                {
                    $"{appName}.deps",
                    $"{appName}.deps.json",
                    $"{appName}.dll",
                    $"{appName}.pdb",
                    $"{appName}.runtimeconfig.json"
                });
            
        }
    }
}
