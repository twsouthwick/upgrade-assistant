﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;

namespace Microsoft.DotNet.UpgradeAssistant.DeprecatedApisAnalyzer.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class AdapterAddMemberCodeFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AdapterRefactorAnalyzer.AddMemberDiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            var semantic = await context.Document.GetSemanticModelAsync().ConfigureAwait(false);

            if (semantic is null)
            {
                return;
            }

            var adapterContext = AdapterContext.Create().FromCompilation(semantic.Compilation);

            if (diagnostic.Properties.TryGetExpectedType(semantic, out var type) && diagnostic.Properties.TryGetMissingMember(semantic, out var member))
            {
                var syntax = type.Locations.FirstOrDefault();

                if (syntax is null)
                {
                    return;
                }

                if (!syntax.IsInSource)
                {
                    return;
                }

                var abstractionDocument = context.Document.Project.Solution.GetDocument(syntax.SourceTree);

                if (abstractionDocument is null)
                {
                    return;
                }

                var root = await abstractionDocument.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

                if (root is null)
                {
                    return;
                }

                var node = root.FindNode(syntax.SourceSpan);

                // Register a code action that will invoke the fix.
                context.RegisterCodeFix(
                    CodeAction.Create(
                        CodeFixResources.AdapterAddMemberTitle,
                        createChangedSolution: async cancellationToken =>
                        {
                            var slnEditor = new SolutionEditor(context.Document.Project.Solution);
                            var editor = await slnEditor.GetDocumentEditorAsync(abstractionDocument.Id, cancellationToken).ConfigureAwait(false);
                            var methodDeclaration = editor.Generator.Declaration(member, adapterContext);

                            var exp = methodDeclaration
                                .WithAdditionalAnnotations(Simplifier.AddImportsAnnotation)
                                .WithAdditionalAnnotations(Simplifier.Annotation);
                            editor.AddMember(node, exp);

                            return slnEditor.GetChangedSolution();
                        },
                        nameof(AdapterAddMemberCodeFixer)),
                    context.Diagnostics);
            }
        }
    }
}
