// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.DotNet.UpgradeAssistant.VisualStudio;

internal class ExtensionAwareHost : IHost
{
    private readonly IServiceScope _scope;
    private readonly IHost _host;

    public ExtensionAwareHost(IHost other)
    {
        _scope = other.Services.CreateScope();
        _host = other;
    }

    public IServiceProvider Services => _scope.ServiceProvider;

    public void Dispose()
    {
        _host.Dispose();
        _scope.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
        => _host.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken = default)
        => _host.StopAsync(cancellationToken);
}
