// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Resources;
using Autofac.Extensions.DependencyInjection;
using Microsoft.DotNet.UpgradeAssistant.Extensions;
using Microsoft.DotNet.UpgradeAssistant.MSBuild;
using Microsoft.DotNet.UpgradeAssistant.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.Extensibility.Commands;

namespace Microsoft.DotNet.UpgradeAssistant.VisualStudio;

[CommandsPackageLoad("IsCSharp", new string[] { "IsCSharp" }, new[] { "SolutionHasProjectCapability:CSharp" })]
public class UpgradeExtension : HostedExtension<UpgradeExtension>
{
    protected override void Configure(IHostBuilder host) => host
        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
        .ConfigureServices((ctx, services) =>
        {
            services.AddSingleton<ITelemetry, EmptyTelemetry>();
            services.AddUpgradeServices(ctx.Configuration, new VisualStudioOptions());
            services.AddSingleton<ExtensionAwareHost>();
            services.AddSingleton<IUserInput, NoInput>();
            services.AddOptions<ExtensionOptions>()
                .Configure(options =>
                {
                    options.RootPath = ctx.Configuration[HostDefaults.ContentRootKey];
                });

            services.AddSingleton<UpgradeRunner>();

            services.AddSingleton<IUpgradeStateManager, NoState>();

            services.AddMsBuild(msBuildOptions =>
            {
            });

            services.AddReadinessChecks(options =>
            {
                options.IgnoreUnsupportedFeatures = false;
            });
        });

    protected override IHost BuildHost(IHostBuilder builder)
        => base.BuildHost(builder).Services.GetRequiredService<ExtensionAwareHost>();

    protected override ResourceManager? ResourceManager => LocalizedStrings.ResourceManager;

    private class NoState : IUpgradeStateManager
    {
        public Task LoadStateAsync(IUpgradeContext context, CancellationToken token) => Task.CompletedTask;

        public Task SaveStateAsync(IUpgradeContext context, CancellationToken token) => Task.CompletedTask;
    }
    private class NoInput : IUserInput
    {
        public bool IsInteractive => false;

        public Task<string?> AskUserAsync(string prompt) => Task.FromResult(default(string));

        public Task<T> ChooseAsync<T>(string message, IEnumerable<T> commands, CancellationToken token)
            where T : UpgradeCommand
            => Task.FromResult(commands.First());

        public Task<bool> WaitToProceedAsync(CancellationToken token) => Task.FromResult(true);
    }
}
