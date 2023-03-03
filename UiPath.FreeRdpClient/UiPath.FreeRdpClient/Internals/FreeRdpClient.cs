using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using Nito.Disposables;
using System.Net.Sockets;

namespace UiPath.Rdp;

internal class FreeRdpClient : IFreeRdpClient
{
    static FreeRdpClient()
    {
        //Make sure winsock is initialized
        using var _ = new TcpClient();
    }

    public FreeRdpClient(ILogger<FreeRdpClient> logger)
    {
        _log = logger;
    }

    private readonly ILogger<FreeRdpClient> _log;
    private static AsyncLock? InitLock = new();

    private async Task<IDisposable?> TryAcquireInitLock()
    {
        /// Make sure freerdp static initilizers are not run concurrently
        /// when they do they fail with 
        if (InitLock is null)
            return null;

        var releaseLock = await InitLock.LockAsync();
        if (InitLock is null)
            return null;

        _log.LogInformation("RdpInitLock acquired.");
        return Disposable.Create(() =>
        {
            InitLock = null;
            releaseLock.Dispose();
            _log.LogInformation("RdpInitLock released.");
        });
    }

    public async Task<IAsyncDisposable> Connect(RdpConnectionSettings connectionSettings)
    {
        NativeInterface.ConnectOptions connectOptions = new()
        {
            Width = connectionSettings.DesktopWidth,
            Height = connectionSettings.DesktopHeight,
            Depth = connectionSettings.ColorDepth,
            FontSmoothing = connectionSettings.FontSmoothing,
            User = connectionSettings.Username,
            Domain = connectionSettings.Domain,
            Password = connectionSettings.Password,
            ClientName = connectionSettings.ClientName,
            HostName = connectionSettings.HostName,
            Port = connectionSettings.Port ?? default
        };
        using var releaseInitLock = await TryAcquireInitLock();
        return await Task.Run(async () =>
        {
            NativeInterface.RdpLogon(connectOptions, out var releaseObjectName);
            return new AsyncDisposable(() =>
            {
                Disconnect(releaseObjectName);
                return ValueTask.CompletedTask;
            });
        });
    }

    private void Disconnect(string releaseObjectName)
    {
        if (releaseObjectName != default)
            NativeInterface.RdpRelease(releaseObjectName);
    }
}