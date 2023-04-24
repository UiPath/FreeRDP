using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Nito.Disposables;
using System.Collections.Concurrent;
using UiPath.Rdp;

namespace UiPath.FreeRdp.Tests;

public class LoggingTests : TestsBase
{
    private readonly ConcurrentDictionary<string, ConcurrentBag<(LogLevel logLevel, string message)>> _loggers = new ();
    private readonly Mock<ILoggerProvider> _loggingProviderMock = new();

    private IFreeRdpClient FreeRdpClient => Host.GetRequiredService<IFreeRdpClient>();
    private ILogger Log => Host.GetRequiredService<ILogger<RdpClientTests>>();

    private async Task<IAsyncDisposable> Connect(RdpConnectionSettings connectionSettings)
    {
        using var logScope = Log.BeginScope($"{Logging.ScopeName}", connectionSettings.ClientName);
        return await FreeRdpClient.Connect(connectionSettings);
    }

    public LoggingTests(ITestOutputHelper output) : base(output)
    {
        _loggingProviderMock.Setup(p => p.CreateLogger(It.IsAny<string>()))
            .Returns((string category) => new FakeLogger(_loggers.GetOrAdd(category, c => new())));

        Host.AddRegistry(s => s.AddLogging(b => b.AddProvider(_loggingProviderMock.Object)));
    }

    private class FakeLogger : ILogger
    {
        private readonly ConcurrentBag<(LogLevel logLevel, string message)> logsBag;

        public FakeLogger(ConcurrentBag<(LogLevel logLevel, string message)> logsBag)
        {
            this.logsBag = logsBag;
        }

        public IDisposable BeginScope<TState>(TState state)
        => NoopDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => logsBag.Add(new(logLevel, formatter(state, exception)));
    }

    [Fact]
    public async Task ShouldProduceLogsFromFreerdp()
    {
        var user = await Host.GivenUser();
        var connectionSettings = new RdpConnectionSettings(
            username: user.UserName.Split("\\")[1],
            password: user.Password,
            domain: user.UserName.Split("\\")[0]
        )
        {
        };

        await using var sut = await Connect(connectionSettings);
        var sessionId = WtsApi.FindFirstSessionByClientName(connectionSettings.ClientName);
        sessionId.HasValue.ShouldBeTrue();

        await sut.DisposeAsync();
        await WaitFor.Predicate(() => WtsApi.FindFirstSessionByClientName(connectionSettings.ClientName) == null);

        var acceptedDebugCategory = "com.freerdp.core.nego";
        var negoLogs = _loggers.Where(kv => kv.Key.StartsWith(acceptedDebugCategory))
            .SelectMany(kv => kv.Value)
            .Where(l => l.logLevel == LogLevel.Debug)
            .ToArray();
        negoLogs.ShouldNotBeEmpty();

        var wrapperCategory = "UiPath.FreeRdpWrapper";
        var wrapperLogs = _loggers.Where(kv => kv.Key.StartsWith(wrapperCategory))
            .SelectMany(kv => kv.Value)
            .ToArray();
        wrapperLogs.ShouldNotBeEmpty();

        var freerdpCategoryPrefix = "com.freerdp";
        var nonDebugFreeRdpLogs = _loggers.Where(kv => kv.Key.StartsWith(freerdpCategoryPrefix) && !kv.Key.StartsWith(acceptedDebugCategory))
            .SelectMany(kv => kv.Value)
            .ToArray();
        nonDebugFreeRdpLogs.ShouldNotBeEmpty();
        nonDebugFreeRdpLogs.Where(l => l.logLevel == LogLevel.Debug).ShouldBeEmpty();
    }
}