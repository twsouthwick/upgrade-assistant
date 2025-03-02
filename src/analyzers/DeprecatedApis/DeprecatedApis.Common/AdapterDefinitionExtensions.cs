﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Microsoft.DotNet.UpgradeAssistant.DeprecatedApisAnalyzer
{
    public static class AdapterDefinitionExtensions
    {
        private const string TypeToReplaceKey = nameof(TypeToReplaceKey);

        private static readonly SymbolDisplayFormat RoundtripMethodFormat = SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);

        public static ImmutableDictionary<string, string?> WithTypeToReplace(this ImmutableDictionary<string, string?> properties, ITypeSymbol symbol)
        {
            if (properties is null)
            {
                throw new System.ArgumentNullException(nameof(properties));
            }

            if (symbol is null)
            {
                throw new System.ArgumentNullException(nameof(symbol));
            }

            return properties.Add(TypeToReplaceKey, symbol.ToDisplayString(RoundtripMethodFormat));
        }

        public static bool TryGetTypeToReplace(this ImmutableDictionary<string, string?> dictionary, SemanticModel semantic, [MaybeNullWhen(false)] out INamedTypeSymbol namedType)
        {
            if (dictionary is null)
            {
                throw new System.ArgumentNullException(nameof(dictionary));
            }

            if (semantic is null)
            {
                throw new System.ArgumentNullException(nameof(semantic));
            }

            if (dictionary.TryGetValue(TypeToReplaceKey, out var result) && result is not null)
            {
                if (semantic.Compilation.GetTypeByMetadataName(result) is INamedTypeSymbol symbol)
                {
                    namedType = symbol;
                    return true;
                }
            }

            namedType = default;
            return false;
        }
    }
}
