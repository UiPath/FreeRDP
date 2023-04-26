using Microsoft.Extensions.Logging;

namespace UiPath.Rdp;

internal sealed class Logging : IDisposable
{
    public static string ScopeName { get; set; } = "RunId";
    internal static Logging? Instance { get; private set; }
    internal readonly NativeInterface.LogCallback LogCallbackDelegate;
    private readonly NativeInterface.RegisterThreadScopeCallback _registerThreadScopeCallbackDelegate;
    private bool _logsForwardingEnabled = false;
    private ILoggerFactory? LoggerFactory { get; set; }

    public string[] FilterRemoveStartsWith { get; set; } = new[]
    {
        // ordered by frequency
        "freerdp_check_fds() failed - 0", //24000+ in 50 sec
        "WARNING: invalid packet signature",
        "transport_check_fds: transport->ReceiveCallback() - -4",
        "fastpath_recv_update_data() fail",
        "Stream_GetRemainingLength() < size",//20000+ in 50 sec
        "fastpath_recv_update_data: fastpath_recv_update() -",
        "Fastpath update Orders [0] failed, status 0",//??
        "Total size (", // 21234214) exceeds MultifragMaxRequestSize (65535) // 2000+ in 50 secs
        "Unexpected FASTPATH_FRAGMENT_SINGLE",//800+ in 50 sec
        "SECONDARY ORDER [0x",//<04/05/...>] Cache Bitmap V2 (Compressed) failed",
        "bulk_decompress() failed",
        "Decompression failure!",
        "Unknown bulk compression type 00000003",
        "Unsupported bulk compression type 00000003",
        "order flags ",
        "history buffer index out of range",//10+
        "history buffer overflow",
        "fastpath_recv_update() - -1",
    };

    public Logging(ILoggerFactory loggerFactory)
    {
        LoggerFactory = loggerFactory;
        LogCallbackDelegate = Log;
        _registerThreadScopeCallbackDelegate = RegisterThreadScope;
        EnableNativeLogsForwarding();
        Instance = this;
    }

    private void Log(string category, LogLevel logLevel, string message)
    {
        if (LoggerFactory is null)
            return;

        if (!FilterLogs(logLevel, message))
            return;

        if (logLevel is LogLevel.Error)
        {
            logLevel = LogLevel.Warning;
        }

        var log = LoggerFactory.CreateLogger(category);
        log.Log(logLevel, message);
    }


    private void RegisterThreadScope(string scope)
    {
        _ = LoggerFactory?.CreateLogger(nameof(RegisterThreadScope)).BeginScope($"{{{ScopeName}}}", scope);
    }

    private bool FilterLogs(LogLevel logLevel, string message)
    {
        if (logLevel is LogLevel.Error
            && FilterRemoveStartsWith.Any(message.StartsWith))
            return false;

        return true;
    }

    private void EnableNativeLogsForwarding()
    {
        var forwardFreeRdpLogs = Environment.GetEnvironmentVariable("WLOG_FILEAPPENDER_OUTPUT_FILE_PATH") is null;
        NativeInterface.InitializeLogging(logCallback: LogCallbackDelegate,
                                          registerThreadScopeCallback: _registerThreadScopeCallbackDelegate,
                                          forwardFreeRdpLogs: forwardFreeRdpLogs);
        _logsForwardingEnabled = true;
    }

    private void DisableNativeLogsForwarding()
    {
        LoggerFactory = null;
        NativeInterface.InitializeLogging(logCallback: null, registerThreadScopeCallback: null, forwardFreeRdpLogs: false);
        _logsForwardingEnabled = false;
    }

    internal void EnsureNativeLogsForwarding()
    {
        if (!_logsForwardingEnabled)
            EnableNativeLogsForwarding();
    }

    public void Dispose()
    {
        DisableNativeLogsForwarding();
    }
}
