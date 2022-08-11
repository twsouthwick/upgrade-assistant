// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.DotNet.UpgradeAssistant.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.DotNet.UpgradeAssistant.VisualStudio;

internal class ExtensionAwareHost : IHost
{
    private readonly AutofacServiceProvider _scope;
    private readonly IHost _host;
    private readonly ILogger<ExtensionAwareHost> _logger;

    public ExtensionAwareHost(IHost other, ILogger<ExtensionAwareHost> logger)
    {
        _scope = Build(other.Services);
        _host = other;
        _logger = logger;
    }

    public IServiceProvider Services => _scope;

    public void Dispose()
    {
        _host.Dispose();
        _scope.Dispose();
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
        => _host.StopAsync(cancellationToken);

    private static AutofacServiceProvider Build(IServiceProvider services)
    {
        var scope = services.GetAutofacRoot().BeginLifetimeScope(builder =>
        {
            foreach (var extension in services.GetRequiredService<IExtensionProvider>().Instances)
            {
                var services = new ServiceCollection();
                extension.AddServices(services);
                builder.Populate(services);
            }
        });

        return new AutofacServiceProvider(scope);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This method should not throw any exceptions.")]
    public async Task StartAsync(CancellationToken token)
    {
        _logger.LogDebug("Configuration loaded from context base directory: {BaseDirectory}", AppContext.BaseDirectory);

        await RunStartupAsync(Services.GetRequiredService<IEnumerable<IUpgradeStartup>>(), token);
        await _host.StartAsync(token);
    }

    private static async Task RunStartupAsync(IEnumerable<IUpgradeStartup> startups, CancellationToken token)
    {
        foreach (var startup in startups)
        {
            if (!await startup.StartupAsync(token))
            {
                throw new UpgradeException($"Failure running start up action {startup.GetType().FullName}");
            }
        }
    }
}
