// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.UpgradeAssistant.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.DotNet.UpgradeAssistant.Cli
{
    public static class UpgradeAssistantHostExtensions
    {
        public static void AddNonInteractive(this IServiceCollection services, Action<NonInteractiveOptions> configure, bool isNonInteractive)
        {
            if (isNonInteractive)
            {
                services.AddTransient<IUserInput, NonInteractiveUserInput>();
                services
                    .AddOptions<NonInteractiveOptions>()
                    .Configure(configure);
            }
        }

        public static void AddKnownExtensionOptions(this IServiceCollection services, KnownExtensionOptionsBuilder options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            services.AddExtensionOption(new
            {
                Backup = new { Skip = options.SkipBackup },
                Solution = new { Entrypoints = options.Entrypoints }
            });
        }
    }
}
