﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.CodeAnalysis;

namespace Microsoft.DotNet.UpgradeAssistant.DeprecatedApisAnalyzer.Test
{
    public record AdapterDescriptorFactory(string Namespace, string Original, string Destination)
    {
        public string FullDestination => $"{Namespace}.{Destination}";

        public string FullOriginal => $"{Namespace}.{Original}";

        public string CreateAttributeString(string languageName)
            => languageName switch
            {
                LanguageNames.VisualBasic => CreateVBAttributeString(),
                LanguageNames.CSharp => CreateCSharpAttributeString(),
                _ => throw new NotSupportedException(),
            };

        private string CreateVBAttributeString() => Destination is null
            ? $"<Assembly: {WellKnownTypeNames.AdapterDescriptorFullyQualified}(GetType({FullOriginal}))>"
            : $"<Assembly: {WellKnownTypeNames.AdapterDescriptorFullyQualified}(GetType({FullOriginal}), GetType({FullDestination}))>";

        private string CreateCSharpAttributeString() => Destination is null
            ? $"[assembly: {WellKnownTypeNames.AdapterDescriptorFullyQualified}(typeof({FullOriginal}))]"
            : $"[assembly: {WellKnownTypeNames.AdapterDescriptorFullyQualified}(typeof({FullOriginal}), typeof({FullDestination}))]";
    }
}
