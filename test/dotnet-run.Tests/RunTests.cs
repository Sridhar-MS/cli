// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.DotNet.TestFramework;
using Microsoft.DotNet.Tools.Test.Utilities;
using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;

namespace Microsoft.DotNet.Tools.Run.Tests
{
    public class RunTests : TestBase
    {
        private const string PortableAppsTestBase = "PortableTests";
        private const string RunTestsBase = "RunTestsApps";
        private const string KestrelHelloWorldBase = "KestrelHelloWorld";
        private const string KestrelHelloWorldPortable = "KestrelHelloWorldPortable";
        
        [Fact]
        public void ItRunsKestrelPortableFatApp()
        {
            TestInstance instance = TestAssetsManager.CreateTestInstance(KestrelHelloWorldBase)
                                                     .WithLockFiles()
                                                     .WithBuildArtifacts();
            
            var url = NetworkHelper.GetLocalhostUrlWithFreePort();
            var args = $"{url} {Guid.NewGuid().ToString()}";
            var runCommand = new RunCommand(Path.Combine(instance.TestRoot, KestrelHelloWorldPortable));
            
            try
            {                
                runCommand.ExecuteAsync(args);
                NetworkHelper.IsServerUp(url).Should().BeTrue($"Unable to connect to kestrel server - {KestrelHelloWorldPortable} @ {url}");
                TestGetRequest(url, args);
            }
            finally
            {                
                runCommand.Kill(true);
            }
        }

        

        private void CopyProjectToTempDir(string projectDir, TempDirectory tempDir)
        {
            // copy all the files to temp dir
            foreach (var file in Directory.EnumerateFiles(projectDir))
            {
                tempDir.CopyFile(file);
            }
        }

        private string GetProjectPath(TempDirectory projectDir)
        {
            return Path.Combine(projectDir.Path, "project.json");
        }

        private static void TestGetRequest(string url, string expectedResponse)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                
                HttpResponseMessage response = client.GetAsync("").Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseString = response.Content.ReadAsStringAsync().Result;
                    responseString.Should().Contain(expectedResponse);                    
                }
            }
        }
    }
}
