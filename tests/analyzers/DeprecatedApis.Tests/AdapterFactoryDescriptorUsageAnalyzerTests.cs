﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Xunit;

using VerifyCS = Microsoft.DotNet.UpgradeAssistant.DeprecatedApisAnalyzer.Test.CSharpCodeFixVerifier<
     Microsoft.DotNet.UpgradeAssistant.DeprecatedApisAnalyzer.AdapterFactoryDescriptorUsageAnalyzer,
     Microsoft.DotNet.UpgradeAssistant.DeprecatedApisAnalyzer.CodeFixes.AdapterRefactorCodeFixer>;

namespace Microsoft.DotNet.UpgradeAssistant.DeprecatedApisAnalyzer.Test
{
    public class AdapterFactoryDescriptorUsageAnalyzerTests : AdapterTestBase
    {
        [Fact]
        public async Task EmptyCode()
        {
            var testFile = string.Empty;

            await VerifyCS.Create().WithSource(testFile).RunAsync();
        }

        [Fact]
        public async Task CorrectlyFormed()
        {
            var testFile = @$"
using System;

[assembly: {WellKnownTypeNames.FactoryDescriptorFullyQualified}(typeof(TestFactory.FactoryClass), nameof(TestFactory.FactoryClass.Create))]
" + @"
namespace TestFactory
{
    public static class FactoryClass
    {
        public static IAbstraction Create(Concrete c) => null;
    }

    public interface IAbstraction
    {
    }

    public class Concrete
    {
    }
}";
            await VerifyCS.Create()
                .WithSource(Attribute)
                .WithSource(testFile)
                .RunAsync();
        }

        [Fact]
        public async Task MethodNotFound()
        {
            var testFile = @$"
using System;

[assembly: {WellKnownTypeNames.FactoryDescriptorFullyQualified}(typeof(TestFactory.FactoryClass), {{|#0:""notreal""|}})]
" + @"
namespace TestFactory
{
    public static class FactoryClass
    {
        public static IAbstraction Create(Concrete c) => null;
    }

    public interface IAbstraction
    {
    }

    public class Concrete
    {
    }
}";

            var diagnostic = VerifyCS.Diagnostic(AdapterFactoryDescriptorUsageAnalyzer.MustExistDiagnosticId).WithLocation(0).WithArguments("notreal", "TestFactory.FactoryClass");

            await VerifyCS.Create()
                .WithSource(Attribute)
                .WithSource(testFile)
                .WithExpectedDiagnostics(diagnostic)
                .RunAsync();
        }

        [InlineData("\"Create\"")]
        [InlineData("nameof(TestFactory.FactoryClass.Create)")]
        [Theory]
        public async Task MustBeStatic(string create)
        {
            var testFile = @$"
using System;

[assembly: {WellKnownTypeNames.FactoryDescriptorFullyQualified}(typeof(TestFactory.FactoryClass), {{|#0:{create}|}})]
" + @"
namespace TestFactory
{
    public class FactoryClass
    {
        public IAbstraction Create(Concrete c) => null;
    }

    public interface IAbstraction
    {
    }

    public class Concrete
    {
    }
}";

            var diagnostic = VerifyCS.Diagnostic(AdapterFactoryDescriptorUsageAnalyzer.MustBeStaticDiagnosticId).WithLocation(0).WithArguments("TestFactory.FactoryClass.Create(TestFactory.Concrete)");

            await VerifyCS.Create()
                .WithSource(Attribute)
                .WithSource(testFile)
                .WithExpectedDiagnostics(diagnostic)
                .RunAsync();
        }
    }
}
