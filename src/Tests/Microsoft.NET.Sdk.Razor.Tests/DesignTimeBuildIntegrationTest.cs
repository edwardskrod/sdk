// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.NET.TestFramework;
using Microsoft.NET.TestFramework.Assertions;
using Microsoft.NET.TestFramework.Commands;
using Microsoft.NET.TestFramework.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.Razor.Design.IntegrationTests
{
    public class DesignTimeBuildIntegrationTest : SdkTest
    {
        public DesignTimeBuildIntegrationTest(ITestOutputHelper log) : base(log) {}

        [Fact]
        public void DesignTimeBuild_DoesNotRunRazorTargets()
        {
            var testAsset = "SimpleMvc";
            var projectDirectory = _testAssetsManager
                .CopyTestAsset(testAsset)
                .WithSource();

            // Using Compile here instead of CompileDesignTime because the latter is only defined when using
            // the VS targets. This is a close enough simulation for an SDK project
            var command = new MSBuildCommand(Log, "Compile", projectDirectory.Path);
            command.Execute("/clp:PerformanceSummary /p:DesignTimeBuild=true")
                .Should()
                .Pass()
                // .And.HaveStdOutContaining("_GenerateRazorAssemblyInfo")
                .And.NotHaveStdOutContaining("RazorCoreGenerate")
                .And.NotHaveStdOutContaining("RazorCoreCompile");

            var outputPath = command.GetOutputDirectory("net5.0", "Debug").ToString();

            new FileInfo(Path.Combine(outputPath, "SimpleMvc.dll")).Should().NotExist();
            new FileInfo(Path.Combine(outputPath, "SimpleMvc.pdb")).Should().NotExist();
            new FileInfo(Path.Combine(outputPath, "SimpleMvc.Views.dll")).Should().NotExist();
            new FileInfo(Path.Combine(outputPath, "SimpleMvc.Views.pdb")).Should().NotExist();
        }

        [Fact]
        public void RazorGenerateDesignTime_ReturnsRazorGenerateWithTargetPath()
        {
            var testAsset = "SimpleMvc";
            var projectDirectory = _testAssetsManager
                .CopyTestAsset(testAsset)
                .WithSource();

            var command = new MSBuildCommand(Log, "RazorGenerateDesignTime;_IntrospectRazorGenerateWithTargetPath", projectDirectory.Path);
            var result = command.Execute();
            result.Should().Pass();

            var filePaths = new string[]
            {
                Path.Combine("Views", "Home", "About.cshtml"),
                Path.Combine("Views", "Home", "Contact.cshtml"),
                Path.Combine("Views", "Home", "Index.cshtml"),
                Path.Combine("Views", "Shared", "Error.cshtml"),
                Path.Combine("Views", "Shared", "_Layout.cshtml"),
                Path.Combine("Views", "Shared", "_ValidationScriptsPartial.cshtml"),
                Path.Combine("Views", "_ViewImports.cshtml"),
                Path.Combine("Views", "_ViewStart.cshtml"),
            };

            var razorIntermediateOutputPath = Path.Combine(
                command.GetBaseIntermediateDirectory().ToString(), "Razor");

            foreach (var filePath in filePaths)
            {
                result.Should().HaveStdOutContaining(
                    $@"RazorGenerateWithTargetPath: {filePath} {filePath} {Path.Combine("obj", "Debug", "net5.0", "Razor", filePath + ".g.cs")}");
            }
        }

        [Fact]
        public void RazorGenerateComponentDesignTime_ReturnsRazorComponentWithTargetPath()
        {
            var testAsset = "ComponentLibrary";
            var projectDirectory = _testAssetsManager
                .CopyTestAsset(testAsset)
                .WithSource();

            var command = new MSBuildCommand(Log, "RazorGenerateComponentDesignTime;_IntrospectRazorComponentWithTargetPath", projectDirectory.Path);
            var result = command.Execute();
            result.Should().Pass();

            var filePaths = new string[]
            {
                Path.Combine("GenericComponent.razor"),
                Path.Combine("MyComponent.razor"),
            };

            foreach (var filePath in filePaths)
            {
                result.Should().HaveStdOutContaining(
                    $@"RazorComponentWithTargetPath: {filePath} {filePath} {Path.Combine("obj", "Debug", "netstandard2.0", "Razor", filePath + ".g.cs")} {Path.Combine("obj", "Debug", "netstandard2.0", "RazorDeclaration", filePath + ".g.cs")}");
            }
        }
    }
}
