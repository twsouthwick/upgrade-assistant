// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Extensibility;

namespace Microsoft.DotNet.UpgradeAssistant.VisualStudio;

internal static class VisualStudioLoggerExtensions
{
    public static void AddOutputWindowLogging(this ILoggingBuilder builder)
    {
        builder.Services.AddTransient<ILoggerFactory, VisualStudioLoggerFactory>();
        builder.Services.AddSingleton<VisualStudioLoggingChannelProvider>();
        builder.Services.AddTransient<IOutputWindowWriter>(ctx => ctx.GetRequiredService<VisualStudioLoggingChannelProvider>());
    }

    internal class VisualStudioLoggingChannelProvider : IOutputWindowWriter
    {
        private readonly string _id = Guid.NewGuid().ToString();
        private readonly Channel<LogMessage> _channel;

        public ChannelWriter<LogMessage> Messages => _channel.Writer;

        public VisualStudioLoggingChannelProvider()
        {
            _channel = Channel.CreateBounded<LogMessage>(new BoundedChannelOptions(200)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false,
            });
        }

        public async Task DrainAsync(VisualStudioExtensibility vs, CancellationToken token)
        {
            var output = await vs.Views().Output.GetChannelAsync(_id, nameof(LocalizedStrings.OutputWindowName), token);

            try
            {
                await foreach (var item in _channel.Reader.ReadAllAsync(token))
                {
                    await output.Writer.WriteAsync('[');
                    await output.Writer.WriteAsync(item.Timestamp.ToString().AsMemory(), token);
                    await output.Writer.WriteAsync("] ".AsMemory(), token);
                    await output.Writer.WriteAsync(item.level.ToString().AsMemory(), token);
                    await output.Writer.WriteAsync(" ".AsMemory(), token);
                    await output.Writer.WriteAsync(item.message.AsMemory(), token);
                    await output.Writer.WriteAsync(Environment.NewLine);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
            }
        }
    }

    internal class VisualStudioLoggerFactory : ILoggerFactory
    {
        private readonly ConcurrentDictionary<string, ILogger> _loggers = new ConcurrentDictionary<string, ILogger>();
        private readonly ChannelWriter<LogMessage> _writer;

        public VisualStudioLoggerFactory(VisualStudioLoggingChannelProvider channel)
        {
            _writer = channel.Messages;
        }

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName)
            => _loggers.GetOrAdd(categoryName, (categoryName, writer) => new Logger(categoryName, writer), _writer);

        public void Dispose()
        {
        }

        private sealed class Logger : ILogger, IDisposable
        {
            private readonly ChannelWriter<LogMessage> _writer;
            private readonly string _name;

            public Logger(string name, ChannelWriter<LogMessage> writer)
            {
                _writer = writer;
                _name = name;
            }

            public IDisposable BeginScope<TState>(TState state) => this;

            public void Dispose()
            {
            }

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                while (!_writer.TryWrite(new(logLevel, _name, formatter(state, exception))))
                {
                }
            }
        }
    }

    internal readonly record struct LogMessage(LogLevel level, string name, string message)
    {
        public DateTimeOffset Timestamp { get; } = DateTimeOffset.Now;
    }
}
