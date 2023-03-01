using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Nito.Disposables;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices;


namespace UiPath.Rdp;

public static class FreeRdpClient
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ConnectOptions
    {
        public int Width;
        public int Height;
        public int Depth;
        public bool FontSmoothing;
        [MarshalAs(UnmanagedType.BStr)]
        public string User;
        [MarshalAs(UnmanagedType.BStr)]
        public string Domain;
        [MarshalAs(UnmanagedType.BStr)]
        public string Password;
        [MarshalAs(UnmanagedType.BStr)]
        public string ClientName;
        [MarshalAs(UnmanagedType.BStr)]
        public string HostName;
        public int Port;
    }

    const string FreeRdpClientDll = "UiPath.FreeRdpWrapper.dll";
    private static string ScopeName = "RunId";

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    private delegate void LogCallback([MarshalAs(UnmanagedType.LPStr)] string category, [MarshalAs(UnmanagedType.I4)] LogLevel logLevel, [MarshalAs(UnmanagedType.LPWStr)] string message);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private delegate void RegisterThreadScopeCallback([MarshalAs(UnmanagedType.LPStr)] string category);

    private static LogCallback LogCallbackDelegate = Log;
    private static RegisterThreadScopeCallback RegisterThreadScopeCallbackDelegate = RegisterThreadScope;

    private static ILoggerFactory? LoggerFactory { get; set; }
    public static string[] DefaultFilterCategories { get; private set; } = new[]
    {
        "com.freerdp.core.fastpath",
        "com.freerdp.core.transport",
        "com.freerdp.core",
        "com.freerdp.core.update",
        "com.freerdp.core.rdp"
    };
    public static string[] DefaultFilterNotContains { get; private set; } = new[]
    {
        "Fastpath update Orders [0] failed, status 0",
        "fastpath_recv_update() - -1",
        "fastpath_recv_update_data() fail",
        "Stream_GetRemainingLength() < size",
        "fastpath_recv_update_data: Unexpected FASTPATH_FRAGMENT_LAST",  
        "fastpath_recv_update_data: Unexpected FASTPATH_FRAGMENT_NEXT",  
        "fastpath_recv_update_data: Unexpected FASTPATH_FRAGMENT_FIRST",
        "bulk_decompress() failed",
        "Fastpath update UNKNOWN [c] failed, status 0",
        "Fastpath update UNKNOWN [e] failed, status 0",
        "exceeds MultifragMaxRequestSize (65535)",
        "Unexpected FASTPATH_FRAGMENT_SINGLE",


        "transport_check_fds: transport->ReceiveCallback() - -4",

        "freerdp_check_fds() failed - 0",
        "Unknown bulk compression type",
        "Decompression failure!",


        "order flags 01 failed",
        "SECONDARY ORDER [0x05] Cache Bitmap V2 (Compressed) failed",
        "order flags 03 failed",
        "Stream_GetRemainingLength(s)",



        "WARNING: invalid packet signature",
        "rdp_recv_tpkt_pdu: rdp_read_share_control_header() fail",  //1
        "transport_check_fds: transport->ReceiveCallback() - -1",   //1
    };

    public static string[] FilterCategories { get; private set; }
    public static string[] FilterNotContains { get; private set; }

    [DllImport(FreeRdpClientDll, PreserveSig = false, CharSet = CharSet.Unicode)]
    private extern static uint InitializeLogging([MarshalAs(UnmanagedType.FunctionPtr)] LogCallback logCallback, [MarshalAs(UnmanagedType.FunctionPtr)]  RegisterThreadScopeCallback registerThreadScopeCallback);

    [DllImport(FreeRdpClientDll, PreserveSig = false, CharSet = CharSet.Unicode)]
    private extern static uint RdpLogon([In] ConnectOptions rdpOptions,  [MarshalAs(UnmanagedType.BStr)]out string releaseObjectName);

    [DllImport(FreeRdpClientDll, PreserveSig = false, CharSet = CharSet.Unicode)]
    private extern static uint RdpRelease(string releaseObjectName);

    static FreeRdpClient()
    {
        //Make sure winsock is initialized
        using var _ = new TcpClient();
    }
    private readonly static ConcurrentDictionary<string, Activity> ActivitiesByClientName = new();

    public static async Task<IAsyncDisposable> Connect(RdpConnectionSettings connectionSettings)
    {
        ConnectOptions connectOptions = new()
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

        return await Task.Run(async () =>
        {

            if (Activity.Current is { } activity)
            {
                ActivitiesByClientName[connectOptions.ClientName] = activity;
            }
            using var releaseActivity = Disposable.Create(() => ActivitiesByClientName.TryRemove(connectOptions.ClientName, out _));
            RdpLogon(connectOptions, out var releaseObjectName);

            while (ActivitiesByClientName.ContainsKey(connectOptions.ClientName))
                await Task.Delay(10);

            return new AsyncDisposable(() =>
            {
                Disconnect(releaseObjectName); 
                return ValueTask.CompletedTask;
            });
        });
    }

    private static void RegisterThreadScope(string scope)
    {
        if (!ActivitiesByClientName.TryGetValue(scope, out var parentActivity))
        {
            BeginScope(scope);
            return;
        }

        var activity = new Activity("FreeRdp_transport_loop");
        foreach (var bagage in parentActivity.Baggage)
            activity.AddBaggage(bagage.Key, bagage.Value);
        activity.Start();

        ActivitiesByClientName.TryRemove(scope, out _);
        if (parentActivity.GetBaggageItem(ScopeName) is { } inheritedScopeValue)
        {
            BeginScope(inheritedScopeValue);
        }

        static void BeginScope(string scopeValue)
        {
            _ = LoggerFactory?.CreateLogger(nameof(RegisterThreadScope)).BeginScope($"{{{ScopeName}}}", scopeValue);
        }
    }

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
        LoggerFactory = loggerFactory;
        InitializeLogging(logCallback: LogCallbackDelegate, registerThreadScopeCallback: RegisterThreadScopeCallbackDelegate);
    }

    private static void Disconnect(string releaseObjectName)
    {
        if (releaseObjectName != default)
            RdpRelease(releaseObjectName);
    }

    public static IServiceCollection AddFreeRdp(this IServiceCollection services, string scopeName = "RunId")
    {
        ScopeName = scopeName;
        return services.AddHostedService<FreeRdpInitilizer>();
    }

    public static IServiceCollection UseFreeRdpFailLogFilter(this IServiceCollection services, string[]? categories = null, string[]? notContains = null)
    {
        FilterCategories ??= DefaultFilterCategories;
        FilterNotContains ??= DefaultFilterNotContains;
        return services;
    }

    private static bool FilterLogs(string category, LogLevel logLevel, string message)
    {
        if (logLevel is LogLevel.Error
            && FilterCategories!.Contains(category)
            && FilterNotContains.Any(f => message.Contains(f)))
            return false;

        return true;
    }

    private sealed class FreeRdpInitilizer : IHostedService
    {
        private readonly ILoggerFactory _loggerFactory;

        public FreeRdpInitilizer(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            SetupLogging(_loggerFactory);
            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            SetupLogging(null);
            return Task.CompletedTask;
        }
    }
}
