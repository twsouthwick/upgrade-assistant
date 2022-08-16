// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Resources;
using Autofac.Extensions.DependencyInjection;
using Microsoft.DotNet.UpgradeAssistant.Extensions;
using Microsoft.DotNet.UpgradeAssistant.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.DotNet.UpgradeAssistant.VisualStudio;

internal class UpgradeExtension : HostedExtension<UpgradeExtension>
{
    protected override void Configure(IHostBuilder host) => host
        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
        .ConfigureServices((ctx, services) =>
        {
            services.AddSingleton<ITelemetry, EmptyTelemetry>();
            services.AddUpgradeServices(ctx.Configuration, new VisualStudioOptions());
            services.AddSingleton<ExtensionAwareHost>();
            services.AddOptions<ExtensionOptions>()
                .Configure(options =>
                {
                    options.RootPath = ctx.Configuration[HostDefaults.ContentRootKey];
                });
        });

    protected override IHost BuildHost(IHostBuilder builder)
        => base.BuildHost(builder).Services.GetRequiredService<ExtensionAwareHost>();

    protected override ResourceManager? ResourceManager => LocalizedStrings.ResourceManager;
}
