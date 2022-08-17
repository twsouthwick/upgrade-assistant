// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;

namespace Microsoft.DotNet.UpgradeAssistant.VisualStudio;

public abstract class HostedExtension<T> : Extension
{
    private const string FieldName = "factoryServiceProvider";
    private readonly Lazy<IHost> _host;

    public HostedExtension()
    {
        _host = new(SetupHost, LazyThreadSafetyMode.ExecutionAndPublication);

        var field = typeof(ExtensionCore).GetField(FieldName, BindingFlags.NonPublic | BindingFlags.Instance);

        if (field is null)
        {
            throw new InvalidOperationException($"Could not find {FieldName}");
        }

        var factoryServiceProvider = new Lazy<IServiceProvider>(() => _host.Value.Services);
        field.SetValue(this, factoryServiceProvider);
    }

    public override async Task InitializeCommandsAsync(CommandSetBase commandSet)
    {
        await _host.Value.StartAsync(default);
        await base.InitializeCommandsAsync(commandSet);
    }

    private IHost SetupHost()
    {
        var directory = Path.GetFullPath(Path.GetDirectoryName(typeof(T).Assembly.Location)!);

        var host = Host.CreateDefaultBuilder()
            .UseContentRoot(directory)
            .ConfigureLogging(builder =>
            {
                builder.AddOutputWindowLogging();
            })
            .ConfigureServices(InitializeServices);

        Configure(host);

        return BuildHost(host);
    }

    protected sealed override void InitializeServices(IServiceCollection serviceCollection)
    {
        base.InitializeServices(serviceCollection);
    }

    protected abstract void Configure(IHostBuilder host);

    protected virtual IHost BuildHost(IHostBuilder builder)
        => builder.Build();
}
