// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.DotNet.UpgradeAssistant.VisualStudio;

internal class VisualStudioOptions : IUpgradeAssistantOptions
{
    public bool IsVerbose => false;

    public string? Format => null;

    public UpgradeTarget TargetTfmSupport => UpgradeTarget.LTS;
}
