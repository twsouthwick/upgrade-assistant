// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Definitions;

namespace Microsoft.DotNet.UpgradeAssistant.VisualStudio
{
    [CommandIcon(KnownMonikers.Extension, IconSettings.IconAndText)]
    [Command("Microsoft.DotNet.UpgradeAssistant.VisualStudio.Command1", "Sample Remote Command", placement: CommandPlacement.ToolsMenu)]
    internal class Command1 : Command
    {
        private TraceSource traceSource;

        public Command1(VisualStudioExtensibility extensibility, TraceSource traceSource, string id)
            : base(extensibility, id)
        {
            this.traceSource = Requires.NotNull(traceSource, nameof(traceSource));
        }

        public override Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
        {
            traceSource.TraceInformation($"Hello from another process!");
            return Task.CompletedTask;
        }
    }
}