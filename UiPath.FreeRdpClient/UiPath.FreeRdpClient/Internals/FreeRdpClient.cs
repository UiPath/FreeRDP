using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using Nito.Disposables;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace UiPath.Rdp;

using CallbackMap = ConditionalWeakTable<IAsyncDisposable, Tuple<string, DisconnectCallback?>>;

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
        _disconnectCallback = OnDisconnect;
        NativeInterface.SetDisconnectCallback(_disconnectCallback);
    }

    private readonly ILogger<FreeRdpClient> _log;
    private readonly AsyncLock _initLock = new();
    private readonly CallbackMap _disconnectCallbacks = [];
    private bool _initialized = false;

    internal NativeInterface.DisconnectCallback _disconnectCallback;

    public async Task<IAsyncDisposable> Connect(RdpConnectionSettings connectionSettings)
    {
        ArgumentNullException.ThrowIfNull(connectionSettings.Username);
        ArgumentNullException.ThrowIfNull(connectionSettings.Domain);
        ArgumentNullException.ThrowIfNull(connectionSettings.Password);

        NativeInterface.ConnectOptions connectOptions = new()
        {
            Width = connectionSettings.DesktopWidth,
            Height = connectionSettings.DesktopHeight,
            Depth = connectionSettings.ColorDepth,
            FontSmoothing = connectionSettings.FontSmoothing,
            User = connectionSettings.Username,
            Domain = connectionSettings.Domain,
            Password = connectionSettings.Password,
            ScopeName = connectionSettings.ScopeName,
            ClientName = connectionSettings.ClientName,
            HostName = connectionSettings.HostName,
            Port = connectionSettings.Port ?? default
        };

        using (await _initLock.LockAsync())
        {
            /// Make sure freerdp static initilizers are not run concurrently
            /// when they do they fail with 
            if (!_initialized)
            {
                _log.LogInformation("RdpInitLock acquired.");
                var connection = await DoConnect();
                _initialized = true;
                _log.LogInformation("RdpInitLock released.");
                return connection;
            }
        }

        return await DoConnect();

        Task<AsyncDisposable> DoConnect() => Task.Run(() =>
        {
            NativeInterface.RdpLogon(connectOptions, out var releaseObjectName);
            var connection = new AsyncDisposable(async () =>
            {
                Disconnect(releaseObjectName);
            });

            _disconnectCallbacks.Add(
                connection,
                Tuple.Create(releaseObjectName, connectionSettings.DisconnectCallback));

            return connection;
        });
    }

    private void Disconnect(string releaseObjectName)
    {
        if (releaseObjectName != default)
            NativeInterface.RdpRelease(releaseObjectName);
    }

    internal void OnDisconnect(string releaseObjectName)
    {
        _log.LogInformation("FreeRDP client disconnected"); // TODO just for tests, remove before pushing
        _disconnectCallbacks
            .SingleOrDefault(item => item.Value?.Item1 == releaseObjectName)
            .Value? // tuple
            .Item2?.Invoke(); // callback
    }
}