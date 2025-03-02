﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

#pragma warning disable CA1062 // Validate arguments of public methods

namespace Microsoft.DotNet.UpgradeAssistant.DeprecatedApisAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class AdapterDefinitionAnalyzer : DiagnosticAnalyzer
    {
        public const string DefinitionDiagnosticId = "UA0110";

        private const string Category = "Refactor";

        private static readonly LocalizableString DefinitionTitle = new LocalizableResourceString(nameof(Resources.AdapterDefinitionTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString DefinitionMessageFormat = new LocalizableResourceString(nameof(Resources.AdapterDefinitionMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString DefinitionDescription = new LocalizableResourceString(nameof(Resources.AdapterDefinitionDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly DiagnosticDescriptor DefinitionRule = new(DefinitionDiagnosticId, DefinitionTitle, DefinitionMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: DefinitionDescription, helpLinkUri: HelpLink.Create(DefinitionDiagnosticId));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(DefinitionRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterOperationAction(context =>
            {
                if (WellKnownTypes.From(context.Operation).AdapterDescriptor is not null)
                {
                    if (context.Operation.Children.Count() == 1 &&
                     context.Operation.Children.First() is ITypeOfOperation typeOf &&
                     typeOf.TypeOperand is ITypeSymbol typeToReplace)
                    {
                        var definition = new ApiDescriptor(typeToReplace);
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                DefinitionRule,
                                context.Operation.Syntax.GetLocation(),
                                properties: definition.Properties,
                                definition.TypeToReplace));
                    }
                }
            }, OperationKind.None);
        }
    }
}
