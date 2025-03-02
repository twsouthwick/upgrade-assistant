﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

#pragma warning disable CA1062 // Validate arguments of public methods

namespace Microsoft.DotNet.UpgradeAssistant.DeprecatedApisAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class AdapterRefactorAnalyzer : DiagnosticAnalyzer
    {
        public const string RefactorDiagnosticId = "UA0100";
        public const string AddMemberDiagnosticId = "UA0101";
        public const string CallFactoryDiagnosticId = "UA0102";
        public const string StaticMemberDiagnosticId = "UA0103";

        private const string Category = "Refactor";

        private static readonly LocalizableString RefactorTitle = new LocalizableResourceString(nameof(Resources.AdapterRefactorTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString RefactorMessageFormat = new LocalizableResourceString(nameof(Resources.AdapterRefactorMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString RefactorDescription = new LocalizableResourceString(nameof(Resources.AdapterRefactorDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString StaticMemberTitle = new LocalizableResourceString(nameof(Resources.AdapterStaticMemberTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString StaticMemberMessageFormat = new LocalizableResourceString(nameof(Resources.AdapterStaticMemberMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString StaticMemberDescription = new LocalizableResourceString(nameof(Resources.AdapterStaticMemberDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString AddMemberTitle = new LocalizableResourceString(nameof(Resources.AdapterAddMemberTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString AddMemberMessageFormat = new LocalizableResourceString(nameof(Resources.AdapterAddMemberMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString AddMemberDescription = new LocalizableResourceString(nameof(Resources.AdapterAddMemberDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString CallFactoryTitle = new LocalizableResourceString(nameof(Resources.AdapterCallFactoryTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString CallFactoryMessageFormat = new LocalizableResourceString(nameof(Resources.AdapterCallFactoryMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString CallFactoryDescription = new LocalizableResourceString(nameof(Resources.AdapterCallFactoryDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly DiagnosticDescriptor RefactorRule = new(RefactorDiagnosticId, RefactorTitle, RefactorMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: RefactorDescription, helpLinkUri: HelpLink.Create(RefactorDiagnosticId));
        private static readonly DiagnosticDescriptor StaticMemberRule = new(StaticMemberDiagnosticId, StaticMemberTitle, StaticMemberMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: StaticMemberDescription, helpLinkUri: HelpLink.Create(StaticMemberDiagnosticId));
        private static readonly DiagnosticDescriptor AddMemberRule = new(AddMemberDiagnosticId, AddMemberTitle, AddMemberMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: AddMemberDescription, helpLinkUri: HelpLink.Create(AddMemberDiagnosticId));
        private static readonly DiagnosticDescriptor CallFactoryRule = new(CallFactoryDiagnosticId, CallFactoryTitle, CallFactoryMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: CallFactoryDescription, helpLinkUri: HelpLink.Create(CallFactoryDiagnosticId));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(RefactorRule, AddMemberRule, CallFactoryRule, StaticMemberRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(context =>
            {
                var adapterContext = AdapterContext.Create().FromCompilation(context.Compilation);

                RegisterAddMemberActions(adapterContext, context);
                RegisterTypeAction(adapterContext, context);
                RegisterCallFactoryActions(adapterContext, context);
                RegisterStaticMemberActions(adapterContext, context);
            });
        }

        private static void RegisterTypeAction(AdapterContext adapterContext, CompilationStartAnalysisContext context)
        {
            if (adapterContext.Types.IsEmpty)
            {
                return;
            }

            context.RegisterTypeAdapterActions(adapterContext, context =>
            {
                foreach (var adapter in adapterContext.Types)
                {
                    if (SymbolEqualityComparer.Default.Equals(adapter.Original, context.symbol))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(RefactorRule, context.node.GetLocation(), properties: adapter.Properties, adapter.OriginalMessage, adapter.DestinationMessage));
                    }
                }
            });
        }

        private static void RegisterStaticMemberActions(AdapterContext adapterContext, CompilationStartAnalysisContext context)
        {
            if (adapterContext.StaticMembers.IsEmpty)
            {
                return;
            }

            context.RegisterOperationAction(context =>
            {
                if (context.Operation.Parent.Kind == OperationKind.NameOf)
                {
                    return;
                }

                var operation = (IPropertyReferenceOperation)context.Operation;
                var descriptor = adapterContext.GetStaticMemberDescriptor(operation.Property);

                if (descriptor is not null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(StaticMemberRule, context.Operation.Syntax.GetLocation(), properties: descriptor.Properties, descriptor.Original.ToDisplayString(), descriptor.Destination.ToDisplayString()));
                }
            }, OperationKind.PropertyReference);
        }

        private static void RegisterAddMemberActions(AdapterContext adapterContext, CompilationStartAnalysisContext context)
        {
            if (adapterContext.Types.IsEmpty)
            {
                return;
            }

            context.RegisterOperationAction(context =>
            {
                if (context.Operation.GetEnclosingMethod() is IMethodSymbol enclosing && adapterContext.IsFactoryMethod(enclosing))
                {
                    return;
                }

                var member = context.Operation switch
                {
                    IInvocationOperation invocation => (ISymbol)invocation.TargetMethod,
                    IPropertyReferenceOperation property => property.Property,
                    _ => throw new NotImplementedException(),
                };

                if (member.IsStatic)
                {
                    return;
                }

                foreach (var adapter in adapterContext.Types)
                {
                    if (SymbolEqualityComparer.Default.Equals(member.ContainingType, adapter.Original))
                    {
                        // TODO: this could be better by matching if it actually binds
                        if (adapter.Destination.GetMembers(member.Name).Length == 0)
                        {
                            var properties = adapter.Properties
                                .WithSymbol(member);

                            context.ReportDiagnostic(Diagnostic.Create(AddMemberRule, context.Operation.Syntax.GetLocation(), properties: properties, member.Name, adapter.Destination));
                        }
                    }
                }
            }, OperationKind.Invocation, OperationKind.PropertyReference);
        }

        private static void RegisterCallFactoryActions(AdapterContext adapterContext, CompilationStartAnalysisContext context)
        {
            if (adapterContext.Factories.Length == 0)
            {
                return;
            }

            context.RegisterOperationBlockStartAction(context =>
            {
                if (context.OwningSymbol is IMethodSymbol enclosing &&
                    !adapterContext.IsFactoryMethod(enclosing) &&
                    adapterContext.GetDescriptorForDestination(enclosing.ReturnType) is ReplacementDescriptor<ITypeSymbol> descriptor)
                {
                    context.RegisterOperationAction(context =>
                    {
                        var @return = (IReturnOperation)context.Operation;

                        foreach (var child in @return.ReturnedValue.Children)
                        {
                            if (SymbolEqualityComparer.Default.Equals(child.Type, descriptor.Original))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(CallFactoryRule, child.Syntax.GetLocation(), properties: descriptor.Properties, descriptor.OriginalMessage, descriptor.DestinationMessage));
                            }
                        }
                    }, OperationKind.Return);
                }
            });

            context.RegisterOperationAction(context =>
            {
                var operation = (IInvalidOperation)context.Operation;
                var symbolInfo = context.Operation.SemanticModel.GetSymbolInfo(operation.Syntax);

                foreach (var child in operation.Children)
                {
                    foreach (var descriptor in adapterContext.Types)
                    {
                        if (SymbolEqualityComparer.Default.Equals(child.Type, descriptor.Original))
                        {
                            // TODO: should match arguments, but seems non-trivial
                            foreach (var proposedMethod in symbolInfo.CandidateSymbols)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(CallFactoryRule, child.Syntax.GetLocation(), properties: descriptor.Properties, descriptor.OriginalMessage, descriptor.DestinationMessage));
                            }
                        }
                    }
                }
            }, OperationKind.Invalid);
        }
    }
}
