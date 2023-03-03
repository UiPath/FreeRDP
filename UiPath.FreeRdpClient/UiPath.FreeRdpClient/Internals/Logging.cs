using Microsoft.Extensions.Logging;


namespace UiPath.Rdp;

internal static class Logging 
{
    public static string ScopeName = "RunId";
    internal static NativeInterface.LogCallback LogCallbackDelegate = Log;
    private static NativeInterface.RegisterThreadScopeCallback RegisterThreadScopeCallbackDelegate = RegisterThreadScope;

    private static ILoggerFactory? LoggerFactory { get; set; }

    public static string[] FilterNotStartsWith { get; private set; } = new[]
    {
        // ordered by frequency
        "freerdp_check_fds() failed - 0", //24000+ in 50 sec
        "WARNING: invalid packet signature",
        "transport_check_fds: transport->ReceiveCallback() - -4",
        "fastpath_recv_update_data() fail",
        "Stream_GetRemainingLength() < size",//20000+ in 50 sec
        "Fastpath update Orders [0] failed, status 0",//??
        "Total size (", // 21234214) exceeds MultifragMaxRequestSize (65535) // 2000+ in 50 secs
        "Unexpected FASTPATH_FRAGMENT_SINGLE",//800+ in 50 sec
        "SECONDARY ORDER [0x",//<04/05/...>] Cache Bitmap V2 (Compressed) failed",
        "bulk_decompress() failed",
        "Decompression failure!",
        "Unknown bulk compression type 00000003",
        "Unsupported bulk compression type 00000003",
        "history buffer index out of range",//10+
        "history buffer overflow",
/*        
        // appeared once in last run
        "fastpath_recv_update() - -1",
        "rdp_recv_tpkt_pdu: rdp_read_share_control_header() fail",
        "transport_check_fds: transport->ReceiveCallback() - -1",

        // did not appear in last run
        "fastpath_recv_update_data: fastpath_recv_update() - -1",
        "fastpath_recv_update_data: Unexpected FASTPATH_FRAGMENT_LAST",
        "fastpath_recv_update_data: Unexpected FASTPATH_FRAGMENT_NEXT",
        "fastpath_recv_update_data: Unexpected FASTPATH_FRAGMENT_FIRST",
        "order flags 03 failed",
        "order flags 01 failed",
        "SECONDARY ORDER [0x05] Cache Bitmap V2 (Compressed) failed",
        "Stream_GetRemainingLength(s)",
        "Fastpath update UNKNOWN [",//c] failed, status 0",
        "Fastpath update Surface Commands [",//4] failed, status -1",
        "Fastpath update Color Pointer [",//9] failed, status 0",
        "unknown cmdType 0x",
*/
    };

    private static void Log(string category, LogLevel logLevel, string message)
    {
        if (LoggerFactory is null)
            return;

        if (!FilterLogs(logLevel, message))
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

    private static bool FilterLogs(LogLevel logLevel, string message)
    {
        if (logLevel is LogLevel.Error
            && FilterNotStartsWith.Any(message.StartsWith))
            return false;

        return true;
    }
}
