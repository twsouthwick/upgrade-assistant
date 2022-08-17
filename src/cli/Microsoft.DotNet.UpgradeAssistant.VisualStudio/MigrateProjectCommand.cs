// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Microsoft.Build.Framework;
using Microsoft.DotNet.UpgradeAssistant.MSBuild;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Definitions;
using Microsoft.VisualStudio.ProjectSystem.Query;
using Microsoft.VisualStudio.ProjectSystem.Query.ProjectModel;

namespace Microsoft.DotNet.UpgradeAssistant.VisualStudio;

[CommandIcon(KnownMonikers.ConvertPartition, IconSettings.IconAndText)]
[Command("Microsoft.DotNet.UpgradeAssistant.VisualStudio.Migrate.Solution", "Migrate Project", containerType: typeof(UpgradeExtension), placement: CommandPlacement.ToolsMenu)]
[CommandEnabledWhen(
    "SolutionLoaded",
    new string[] { "SolutionLoaded" },
    new string[] { "SolutionState:FullyLoaded" })]
internal class MigrateProjectCommand : Command
{
    private readonly IOptions<WorkspaceOptions> _options;
    private readonly UpgradeRunner _manager;
    private readonly ILogger<MigrateProjectCommand> _logger;

    public MigrateProjectCommand(VisualStudioExtensibility extensibility, IOptions<WorkspaceOptions> options, ILogger<MigrateProjectCommand> logger, UpgradeRunner manager, string id)
        : base(extensibility, id)
    {
        _options = options;
        _manager = manager;
        _logger = logger;
    }

    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        var solutionQuery = await this.Extensibility.Workspaces().QuerySolutionAsync(
            query => query.With(sln => sln.Path),
            cancellationToken);
        var solution = solutionQuery.FirstOrDefault();

        if (solution is null)
        {
            _logger.LogInformation("No projects available");
            return;
        }

        UpdateOptions(solution);

        await _manager.RunAsync(cancellationToken);
    }

    private void UpdateOptions(ISolution solution)
    {
        var options = _options.Value;

        options.InputPath = solution.Path;

        if (int.TryParse(Environment.GetEnvironmentVariable("VisualStudioVersion"), out var version))
        {
            options.VisualStudioVersion = version;
        }


    }
}
