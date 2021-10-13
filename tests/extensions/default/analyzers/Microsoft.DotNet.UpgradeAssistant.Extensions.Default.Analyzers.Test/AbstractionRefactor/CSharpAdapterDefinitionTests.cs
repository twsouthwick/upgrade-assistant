// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

using VerifyCS = Microsoft.DotNet.UpgradeAssistant.Extensions.Default.Analyzers.Test.CSharpCodeFixVerifier<
     Microsoft.DotNet.UpgradeAssistant.Extensions.Default.Analyzers.AdapterRefactorAnalyzer,
     Microsoft.DotNet.UpgradeAssistant.Extensions.Default.CodeFixes.AdapterDefinitionCodeFixer>;

namespace Microsoft.DotNet.UpgradeAssistant.Extensions.Default.Analyzers.Test.AbstractionRefactor
{
    public class CSharpAdapterDefinitionTests : AdapterTestBase
    {
        [Fact]
        public async Task CanGenerateInterfaceStub()
        {
            while (!System.Diagnostics.Debugger.IsAttached)
            {
                System.Console.WriteLine($"Waiting to attach on ${System.Diagnostics.Process.GetCurrentProcess().Id}");
                System.Threading.Thread.Sleep(1000);
            }
            var testFile = @"
[assembly: {|#0:Microsoft.CodeAnalysis.Refactoring.AdapterDescriptor(typeof(RefactorTest.SomeClass))|}]

namespace RefactorTest
{
    public class Test
    {
        public void Run(SomeClass c)
        {
            var isValid = c.IsValid();
        }
    }

    public class SomeClass
    {
       public bool IsValid() => true;
    }
}";

            const string withFix = @"
[assembly: Microsoft.CodeAnalysis.Refactoring.AdapterDescriptor(typeof(RefactorTest.ISomeClass), typeof(global::RefactorTest.SomeClass))]

namespace RefactorTest
{
    public class Test
    {
        public void Run({|#0:SomeClass|} c)
        {
            var isValid = {|#1:c.IsValid()|};
        }
    }

    public class SomeClass
    {
       public bool IsValid() => true;
    }
}";

            const string interfaceDefinition = @"namespace RefactorTest
{
    public interface ISomeClass
    {
    }
}";

            var diagnostic = VerifyCS.Diagnostic(AdapterRefactorAnalyzer.DefinitionDiagnosticId).WithLocation(0).WithArguments("RefactorTest.SomeClass");
            var refactorDiagnostic = VerifyCS.Diagnostic(AdapterRefactorAnalyzer.RefactorDiagnosticId).WithLocation(0).WithArguments("SomeClass", "ISomeClass");
            var addMemberDiagnostic = VerifyCS.Diagnostic(AdapterRefactorAnalyzer.AddMemberDiagnosticId).WithLocation(1).WithArguments("IsValid", "RefactorTest.ISomeClass");
            var test = CreateTest(VerifyCS.Create(), null)
                .WithSource(testFile)
                .WithExpectedDiagnostics(diagnostic)
                .WithFixed(withFix);
                // .WithFixed(interfaceDefinition, "ISomeClass.cs")
                // .WithExpectedDiagnosticsAfter(refactorDiagnostic, addMemberDiagnostic);
            await test.RunAsync();
        }
    }
}
