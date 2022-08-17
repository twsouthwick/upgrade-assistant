// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.UpgradeAssistant.Telemetry;
using Microsoft.Extensions.Logging;

namespace Microsoft.DotNet.UpgradeAssistant;

public class UpgradeRunner
{
    private readonly IUpgradeContextFactory _contextFactory;
    private readonly UpgraderManager _upgrader;
    private readonly IUpgradeStateManager _stateManager;
    private readonly ILogger<UpgradeRunner> _logger;

    public UpgradeRunner(
       IUpgradeContextFactory contextFactory,
       UpgraderManager upgrader,
       IUpgradeStateManager stateManager,
       ILogger<UpgradeRunner> logger)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _upgrader = upgrader ?? throw new ArgumentNullException(nameof(upgrader));
        _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task RunAsync(CancellationToken token)
    {
        using var context = await _contextFactory.CreateContext(token);

        await _stateManager.LoadStateAsync(context, token);

        try
        {
            // Cache current steps here as defense-in-depth against the possibility
            // of a bug (or very weird upgrade step behavior) causing the current step
            // to reset state after being initialized by GetNextStepAsync
            var steps = await _upgrader.InitializeAsync(context, token);
            var step = await _upgrader.GetNextStepAsync(context, token);

            while (step is not null)
            {
                await step.ApplyAsync(context, token);

                step = await _upgrader.GetNextStepAsync(context, token);
            }

            _logger.LogInformation("Upgrade has completed. Please review any changes.");
        }
        finally
        {
            // Do not pass the same token as it may have been canceled and we still need to persist this.
            await _stateManager.SaveStateAsync(context, default);
        }
    }
}
