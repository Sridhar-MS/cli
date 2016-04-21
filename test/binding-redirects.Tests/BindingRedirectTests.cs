﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.Tools.Test.Utilities;
using Microsoft.DotNet.TestFramework;
using Microsoft.Extensions.PlatformAbstractions;
using Xunit;
using FluentAssertions;

namespace Microsoft.DotNet.Tests
{
    public class TestSetupFixture : TestBase
    {
        private const string Framework = "net451";
        private const string Config = "Debug";
        private const string Runtime = "win7-x64";
        private const string AppWithConfig = "AppWithRedirectsAndConfig";
        private const string AppWithoutConfig = "AppWithRedirectsNoConfig";

        private string _desktopProjectsRoot = Path.Combine(RepoRoot, "TestAssets", "DesktopTestProjects");
        private string _buildRelativePath = Path.Combine("bin", Config, Framework, Runtime);
        private string _appWithConfigBuildDir;
        private string _appWithConfigPublishDir;
        private string _appWithoutConfigBuildDir;
        private string _appWithoutConfigPublishDir;
        private TestInstance _testInstance;


        public string AppWithConfigBuildOutput { get; }
        public string AppWithConfigPublishOutput { get; }
        public string AppWithoutConfigBuildOutput { get; }
        public string AppWithoutConfigPublishOutput { get; }

        public TestSetupFixture()
        {
            var testAssetsMgr = new TestAssetsManager(_desktopProjectsRoot);
            _testInstance = testAssetsMgr.CreateTestInstance("BindingRedirectSample")
                                                     .WithLockFiles();

            Setup(AppWithConfig, ref _appWithConfigBuildDir, ref _appWithConfigPublishDir);
            Setup(AppWithoutConfig, ref _appWithoutConfigBuildDir, ref _appWithoutConfigPublishDir);

            AppWithConfigBuildOutput = Path.Combine(_appWithConfigBuildDir, AppWithConfig + ".exe");
            AppWithConfigPublishOutput = Path.Combine(_appWithConfigPublishDir, AppWithConfig + ".exe");
            AppWithoutConfigBuildOutput = Path.Combine(_appWithoutConfigBuildDir, AppWithoutConfig + ".exe");
            AppWithoutConfigPublishOutput = Path.Combine(_appWithoutConfigPublishDir, AppWithoutConfig + ".exe");
        }

        private void Setup(string project, ref string buildDir, ref string publishDir)
        {
            string projectRoot = Path.Combine(_testInstance.TestRoot, project);
            buildDir = Path.Combine(projectRoot, _buildRelativePath);
            publishDir = Path.Combine(projectRoot, "publish");

            var buildCommand = new BuildCommand(projectRoot, framework: Framework, runtime: Runtime);
            buildCommand.Execute().Should().Pass();

            var publishCommand = new PublishCommand(projectRoot, output: publishDir, framework: Framework, runtime: Runtime);
            publishCommand.Execute().Should().Pass();
        }
    }

    public class GivenAnAppWithRedirectsAndExecutableDependency : TestBase, IClassFixture<TestSetupFixture>
    {
        private const string ExecutableDependency = "dotnet-desktop-binding-redirects.exe";
        private TestSetupFixture _testSetup;
        private string _appWithConfigBuildOutput;
        private string _appWithoutConfigBuildOutput;
        private string _appWithConfigPublishOutput;
        private string _appWithoutConfigPublishOutput;
        private string _executableDependencyBuildOutput;
        private string _executableDependencyPublishOutput;

        public GivenAnAppWithRedirectsAndExecutableDependency(TestSetupFixture testSetup)
        {
            _testSetup = testSetup;
            _appWithConfigBuildOutput = _testSetup.AppWithConfigBuildOutput;
            _appWithConfigPublishOutput = _testSetup.AppWithConfigPublishOutput;
            _appWithoutConfigBuildOutput = _testSetup.AppWithoutConfigBuildOutput;
            _appWithoutConfigPublishOutput = _testSetup.AppWithoutConfigPublishOutput;
            _executableDependencyBuildOutput = Path.Combine(Path.GetDirectoryName(_appWithConfigBuildOutput), ExecutableDependency);
            _executableDependencyPublishOutput = Path.Combine(Path.GetDirectoryName(_appWithConfigPublishOutput), ExecutableDependency);
        }

        private static List<string> BindingsAppNoConfig
        {
            get
            {
                List<string> bindings = new List<string>()
                {
                    @"<dependentAssembly xmlns=""urn:schemas-microsoft-com:asm.v1"">
                        <assemblyIdentity name=""Newtonsoft.Json"" publicKeyToken=""30ad4fe6b2a6aeed"" culture=""neutral"" />
                        <bindingRedirect oldVersion=""4.5.0.0"" newVersion=""8.0.0.0"" />
                        <bindingRedirect oldVersion=""6.0.0.0"" newVersion=""8.0.0.0"" />
                      </dependentAssembly>",
                    @"<dependentAssembly xmlns=""urn:schemas-microsoft-com:asm.v1"">
                        <assemblyIdentity name=""System.Web.Mvc"" publicKeyToken=""31bf3856ad364e35"" culture=""neutral"" />
                        <bindingRedirect oldVersion=""4.0.0.0"" newVersion=""3.0.0.1"" />
                      </dependentAssembly>"
                };

                return bindings;
            }
        }

        private static List<string> BindingsAppWithConfig
        {
            get
            {
                List<string> bindings = new List<string>()
                {
                    @"<dependentAssembly xmlns=""urn:schemas-microsoft-com:asm.v1"">
                        <assemblyIdentity name=""Newtonsoft.Json"" publicKeyToken=""30ad4fe6b2a6aeed"" culture=""neutral"" />
                        <bindingRedirect oldVersion=""3.5.0.0"" newVersion=""8.0.0.0"" />
                        <bindingRedirect oldVersion=""4.5.0.0"" newVersion=""8.0.0.0"" />
                        <bindingRedirect oldVersion=""6.0.0.0"" newVersion=""8.0.0.0"" />
                      </dependentAssembly>",
                    @"<dependentAssembly xmlns=""urn:schemas-microsoft-com:asm.v1"">
                        <assemblyIdentity name=""Some.Foo.Assembly"" publicKeyToken=""814f48568d36eed5"" culture=""neutral"" />
                        <bindingRedirect oldVersion=""3.0.0.0"" newVersion=""5.5.5.1"" />
                      </dependentAssembly>",
                    @"<dependentAssembly xmlns=""urn:schemas-microsoft-com:asm.v1"">
                        <assemblyIdentity name=""System.Web.Mvc"" publicKeyToken=""31bf3856ad364e35"" culture=""neutral"" />
                        <bindingRedirect oldVersion=""4.0.0.0"" newVersion=""3.0.0.1"" />
                      </dependentAssembly>"
                };

                return bindings;
            }
        }

        private static List<XElement> ExpectedBindingsAppNoConfig
        {
            get
            {
                List<XElement> bindingElements = new List<XElement>();

                foreach (var binding in BindingsAppNoConfig)
                {
                    bindingElements.Add(XElement.Parse(binding));
                }

                return bindingElements;
            }
        }

        private static List<XElement> ExpectedBindingsAppWithConfig
        {
            get
            {
                List<XElement> bindingElements = new List<XElement>();

                foreach (var binding in BindingsAppWithConfig)
                {
                    bindingElements.Add(XElement.Parse(binding));
                }

                return bindingElements;
            }
        }

        private static Dictionary<string, string> ExpectedAppSettings
        {
            get
            {
                Dictionary<string, string> appSettings = new Dictionary<string, string>()
                {
                    {"Setting1", "Hello"},
                    {"Setting2", "World"}
                };

                return appSettings;
            }
        }

        private IEnumerable<XElement> GetRedirects(string exePath)
        {
            var configFile = exePath + ".config";
            File.Exists(configFile).Should().BeTrue($"Config file not found - {configFile}");
            var config = ConfigurationManager.OpenExeConfiguration(exePath);
            var runtimeSectionXml = config.Sections["runtime"].SectionInformation.GetRawXml();
            var runtimeSectionElement = XElement.Parse(runtimeSectionXml);
            var redirects = runtimeSectionElement.Elements()
                                .Where(e => e.Name.LocalName == "assemblyBinding").Elements()
                                .Where(e => e.Name.LocalName == "dependentAssembly");
            return redirects;
        }

        private void VerifyRedirects(IEnumerable<XElement> redirects, IEnumerable<XElement> generatedBindings)
        {
            foreach (var binding in generatedBindings)
            {
                var redirect = redirects.SingleOrDefault(r => /*XNode.DeepEquals(r, binding)*/ r.ToString() == binding.ToString());

                redirect.Should().NotBeNull($"Binding not found in runtime section : {Environment.NewLine}{binding}");
            }
        }

        private void VerifyAppSettings(string exePath)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(exePath);
            foreach (var appSetting in ExpectedAppSettings)
            {
                var value = configFile.AppSettings.Settings[appSetting.Key];
                value.Should().NotBeNull($"AppSetting with key '{appSetting.Key}' not found in config file.");
                value.Value.Should().Be(appSetting.Value, $"For AppSetting '{appSetting.Key}' - Expected Value '{appSetting.Value}', Actual '{ value.Value}'");
            }
        }

        [Fact]
        public void Build_Generates_Redirects_For_App_Without_Config()
        {
            var redirects = GetRedirects(_appWithoutConfigBuildOutput);
            VerifyRedirects(redirects, ExpectedBindingsAppNoConfig);

            var commandResult = new TestCommand(_appWithoutConfigBuildOutput)
                                    .Execute();
            commandResult.Should().Pass();
        }

        [Fact]
        public void Publish_Generates_Redirects_For_App_Without_Config()
        {
            var redirects = GetRedirects(_appWithoutConfigPublishOutput);
            VerifyRedirects(redirects, ExpectedBindingsAppNoConfig);

            var commandResult = new TestCommand(_appWithoutConfigPublishOutput)
                                    .Execute();
            commandResult.Should().Pass();
        }

        [Fact]
        public void Build_Generates_Redirects_For_Executable_Dependency()
        {
            var redirects = GetRedirects(_executableDependencyBuildOutput);
            VerifyRedirects(redirects, ExpectedBindingsAppNoConfig);

            var commandResult = new TestCommand(_executableDependencyBuildOutput)
                                    .Execute();
            commandResult.Should().Pass();
        }

        [Fact]
        public void Publish_Generates_Redirects_For_Executable_Dependency()
        {
            var redirects = GetRedirects(_executableDependencyPublishOutput);
            VerifyRedirects(redirects, ExpectedBindingsAppNoConfig);

            var commandResult = new TestCommand(_executableDependencyPublishOutput)
                                    .Execute();
            commandResult.Should().Pass();
        }

        [Fact]
        public void Build_Generates_Redirects_For_App_With_Config()
        {
            var redirects = GetRedirects(_appWithConfigBuildOutput);
            VerifyRedirects(redirects, ExpectedBindingsAppWithConfig);
            VerifyAppSettings(_appWithConfigBuildOutput);

            var commandResult = new TestCommand(_appWithConfigBuildOutput)
                                    .Execute();
            commandResult.Should().Pass();
        }

        [Fact]
        public void Publish_Generates_Redirects_For_App_With_Config()
        {
            var redirects = GetRedirects(_appWithConfigPublishOutput);
            VerifyRedirects(redirects, ExpectedBindingsAppWithConfig);
            VerifyAppSettings(_appWithConfigPublishOutput);

            var commandResult = new TestCommand(_appWithConfigPublishOutput)
                                    .Execute();
            commandResult.Should().Pass();
        }
    }
}
