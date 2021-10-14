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

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            if (root is null)
            {
                return;
            }

            var node = root.FindNode(diagnostic.Location.SourceSpan);

            if (node is null)
            {
                return;
            }

            var adapterContext = AdapterContext.Create().FromCompilation(semantic.Compilation);

            if (diagnostic.Properties.TryGetTypeToReplace(semantic, out var typeToReplace))
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "foo",
                        createChangedSolution: async cancellationToken =>
                        {
                            // Update attribute
                            var editor = await DocumentEditor.CreateAsync(context.Document, cancellationToken).ConfigureAwait(false);
                            var interfaceName = $"I{typeToReplace.Name}";

                            var newArg = editor.Generator.AttributeArgument(
                                editor.Generator.TypeOfExpression(
                                    editor.Generator.QualifiedName(
                                        editor.Generator.NameExpression(typeToReplace.ContainingNamespace),
                                        editor.Generator.IdentifierName(interfaceName))));

                            var newNode = editor.Generator.InsertAttributeArguments(node, 0, new[] { newArg });
                            editor.ReplaceNode(node, newNode);

                            var interfaceDeclaration = editor.Generator.InterfaceDeclaration(interfaceName, accessibility: Accessibility.Public);
                            var namespaceDeclaration = editor.Generator.NamespaceDeclaration(
                                editor.Generator.NameExpression(typeToReplace.ContainingNamespace),
                                interfaceDeclaration);
                            editor.AddMember(root, namespaceDeclaration);

                            return editor.GetChangedDocument().Project.Solution;

                            // Add the interface declaration to the abstractions project

                            //var addedDocument = project.AddDocument("{interfaceName}.cs", editor.Generator.CompilationUnit(namespaceDeclaration));

                            //return addedDocument.Project.Solution;
                        },
                        nameof(AdapterDefinitionCodeFixer)),
                    diagnostic);
            }
        }
    }
}
