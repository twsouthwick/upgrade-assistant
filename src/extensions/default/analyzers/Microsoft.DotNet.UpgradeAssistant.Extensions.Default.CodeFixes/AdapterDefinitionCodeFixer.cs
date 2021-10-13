// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using System.Linq;
using Microsoft.DotNet.UpgradeAssistant.Extensions.Default.Analyzers;

namespace Microsoft.DotNet.UpgradeAssistant.Extensions.Default.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class AdapterDefinitionCodeFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AdapterRefactorAnalyzer.DefinitionDiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var semantic = await context.Document.GetSemanticModelAsync();

            if (semantic is null)
            {
                return;
            }

            if (semantic.Compilation.GetTypeByMetadataName("RefactorTest.ISomeClass") is not null)
            {
                return;
            }

            if (diagnostic.Properties.TryGetTypeToReplace(semantic, out var typeToReplace))
            {   
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "foo",
                        createChangedSolution: async cancellationToken =>
                        {
                            if (semantic.Compilation.GetTypeByMetadataName("RefactorTest.ISomeClass") is not null)
                            {
                                return context.Document.Project.Solution;
                            }

                            // Todo: Replace SyntaxFactory with SyntaxGenerator for more language support
                            var slnEditor = new SolutionEditor(context.Document.Project.Solution);
                            var editor = await slnEditor.GetDocumentEditorAsync(context.Document.Id, cancellationToken);
                            var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
                            var interfaceName = $"I{typeToReplace.Name}";

                            // Add the interface declaration to the abstractions project
                            var interfaceDeclaration = editor.Generator.InterfaceDeclaration(interfaceName, accessibility: Accessibility.Public);
                            var namespaceDeclaration = editor.Generator.NamespaceDeclaration(
                                editor.Generator.NameExpression(typeToReplace.ContainingNamespace),
                                interfaceDeclaration
                            ).NormalizeWhitespace(eol: System.Environment.NewLine);

                            // Update the definition attribute with the descriptor attribute
                            var definitionAttribute = root.FindNode(diagnostic.Location.SourceSpan);
                            var descriptorAttribute = SyntaxFactory.Attribute(
                                SyntaxFactory.ParseName("Microsoft.CodeAnalysis.AdapterDescriptor"),
                                SyntaxFactory.AttributeArgumentList().AddArguments(
                                    SyntaxFactory.AttributeArgument(SyntaxFactory.TypeOfExpression(SyntaxFactory.ParseTypeName("RefactorTest.ISomeClass"))),
                                    SyntaxFactory.AttributeArgument(SyntaxFactory.TypeOfExpression(SyntaxFactory.ParseTypeName(typeToReplace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))))
                            ));
                            editor.ReplaceNode(definitionAttribute, descriptorAttribute);

                            return slnEditor.GetChangedSolution();
                        },
                        nameof(AdapterDefinitionCodeFixer)),
                    diagnostic);
            }
        }
    }
}
