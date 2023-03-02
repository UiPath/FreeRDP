using Microsoft.Extensions.Logging;


namespace UiPath.Rdp;

internal static class Logging 
{
    public static string ScopeName = "RunId";
    internal static NativeInterface.LogCallback LogCallbackDelegate = Log;
    private static NativeInterface.RegisterThreadScopeCallback RegisterThreadScopeCallbackDelegate = RegisterThreadScope;

    private static ILoggerFactory? LoggerFactory { get; set; }

    public static HashSet<string> FilterErrorsInCategories { get; private set; } = new[]
    {
        "com.freerdp.core.fastpath",
        "com.freerdp.core.transport",
        "com.freerdp.core",
        "com.freerdp.core.update",
        "com.freerdp.core.rdp",
        "com.freerdp.codec.mppc",
        "com.freerdp.core.surface"
    }.ToHashSet();

    public static string[] FilterNotStartsWith { get; private set; } = new[]
    {
        "Fastpath update UNKNOWN [",//c] failed, status 0",
        "Total size (",
        "Fastpath update Surface Commands [",//4] failed, status -1",
        "Fastpath update Color Pointer [",//9] failed, status 0",
        "unknown cmdType 0x",
    };
    public static HashSet<string> FilterErrorMessages { get; private set; } = new[]
    {
        "fastpath_recv_update_data: Unexpected FASTPATH_FRAGMENT_LAST",
        "fastpath_recv_update_data: Unexpected FASTPATH_FRAGMENT_NEXT",
        "fastpath_recv_update_data: Unexpected FASTPATH_FRAGMENT_FIRST",
        "fastpath_recv_update() - -1",
        "fastpath_recv_update_data() fail",
        "fastpath_recv_update_data: fastpath_recv_update() - -1",
        "Stream_GetRemainingLength() < size",
        "bulk_decompress() failed",
        "Unexpected FASTPATH_FRAGMENT_SINGLE",
        "transport_check_fds: transport->ReceiveCallback() - -4",
        "Decompression failure!",
        "Unsupported bulk compression type 00000003",
        "WARNING: invalid packet signature",
        //"rdp_recv_tpkt_pdu: rdp_read_share_control_header() fail",  //1
        //"transport_check_fds: transport->ReceiveCallback() - -1",   //1
        "history buffer index out of range",
        "history buffer overflow",


        "order flags 01 failed",
        "SECONDARY ORDER [0x05] Cache Bitmap V2 (Compressed) failed",
        "order flags 03 failed",
        "Stream_GetRemainingLength(s)",

        "freerdp_check_fds() failed - 0",
        "Unknown bulk compression type 00000003",
    }.ToHashSet();

    private static void Log(string category, LogLevel logLevel, string message)
    {
        if (LoggerFactory is null)
            return;

        if (!FilterLogs(category, logLevel, message))
            return;

        var log = LoggerFactory.CreateLogger(category);
        log.Log(logLevel, message);
    }

    public static void SetupLogging(ILoggerFactory? loggerFactory)
    {
        var forwardFreeRdpLogs = Environment.GetEnvironmentVariable("WLOG_FILEAPPENDER_OUTPUT_FILE_PATH") is null;
        LoggerFactory = loggerFactory;
        NativeInterface.InitializeLogging(logCallback: LogCallbackDelegate,
                                          registerThreadScopeCallback: RegisterThreadScopeCallbackDelegate,
                                          forwardFreeRdpLogs: forwardFreeRdpLogs);
    }

    private static void RegisterThreadScope(string scope)
    {
        BeginScope(scope);

        static void BeginScope(string scopeValue)
        {
            _ = LoggerFactory?.CreateLogger(nameof(RegisterThreadScope)).BeginScope($"{{{ScopeName}}}", scopeValue);
        }
    }

    private static bool FilterLogs(string category, LogLevel logLevel, string message)
    {
        if (logLevel is LogLevel.Error
            && FilterErrorsInCategories.Contains(category)
            && (FilterErrorMessages.Contains(message) || FilterNotStartsWith.Any(message.StartsWith)))
            return false;

        return true;
    }
}
