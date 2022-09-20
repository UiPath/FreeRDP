using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nito.Disposables;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
        public string User;
        public string Domain;
        public string Password;
        public string ClientName;
    }

    const string FreeRdpClientDll = "UiPath.FreeRdpWrapper.dll";

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void LogCallback([MarshalAs(UnmanagedType.I4)] LogLevel logLevel, [MarshalAs(UnmanagedType.LPWStr)] string message);

    private static LogCallback LogCallbackDelegate = Log;
    private static ILogger? Logger { get; set; }

    //private static IntPtr DllHandle { get; }
    //static FreeRdpClient()
    //{
    //    var dllPath = "Rdp\\x";
    //    dllPath += Environment.Is64BitProcess ? "64" : "86";

    //    DllHandle = LoadLibrary(dllPath + "\\" + FreeRdpClientDll);
    //}

    [DllImport("kernel32.dll")]
    private static extern IntPtr LoadLibrary(string dllToLoad);

    [DllImport(FreeRdpClientDll, CharSet = CharSet.Unicode)]
    private extern static uint InitializeLogging([MarshalAs(UnmanagedType.FunctionPtr)] LogCallback logCallback);

    [DllImport(FreeRdpClientDll, CharSet = CharSet.Unicode)]
    private extern static uint RdpLogon(ref ConnectOptions rdpOptions, [MarshalAs(UnmanagedType.BStr)] out string releaseObjectName);

    [DllImport(FreeRdpClientDll, CharSet = CharSet.Unicode)]
    private extern static uint RdpRelease([MarshalAs(UnmanagedType.LPWStr)] string releaseObjectName);

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
            ClientName = connectionSettings.ClientName
        };

        return await Task.Run(() =>
        {
            ThrowOnFail(RdpLogon(ref connectOptions, out var releaseObjectName));
            return new AsyncDisposable(() => { Disconnect(releaseObjectName); return ValueTask.CompletedTask; });
        });
    }

    private static void Log(LogLevel loglevel, string message)
    => Logger?.Log(loglevel, message);

    public static void SetupLogging(ILogger? logger)
    {
        Logger = logger;
        ThrowOnFail(InitializeLogging(LogCallbackDelegate));
    }

    private static void Disconnect(string releaseObjectName)
    {
        if (releaseObjectName != default)
            ThrowOnFail(RdpRelease(releaseObjectName));
    }

    private static void ThrowOnFail(uint pinvokeResult, Action? cleanUp = null, [CallerMemberName] string? method = null, [CallerArgumentExpression("pinvokeResult")] string? expression = null)
    {
        if (pinvokeResult == 0)
        {
            return;
        }

        var error = (int)pinvokeResult;
        try
        {
            cleanUp?.Invoke();
        }
        finally
        {
            var source = $"{method}:{expression}";
            throw new Win32Exception(error);
        }
    }

    public static IServiceCollection AddFreeRdp(this IServiceCollection services)
    => services.AddHostedService<FreeRdpInitilizer>();

    private class FreeRdpInitilizer : IHostedService
    {
        private readonly ILogger _logger;

        public FreeRdpInitilizer(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(nameof(FreeRdpClient));
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            SetupLogging(_logger);
            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            SetupLogging(null);
            return Task.CompletedTask;
        }
    }
}

public class RdpConnectionSettings
{

    public string Username { get; init; }
    public string Domain { get; init; }
    public string Password { get; init; }

    public RdpConnectionSettings(string username, string domain, string password)
    {
        Username = username;
        Domain = domain;
        Password = password;
    }

    private static volatile int ConnectionId = 0;
    private static readonly string ClientNameBase = "NG_" + Process.GetCurrentProcess().Id % 1e7 + "_";

    public int DesktopWidth { get; set; } = 1024;

    public int DesktopHeight { get; set; } = 768;

    public int ColorDepth { get; set; } = 32;

    [MaxLength(15, ErrorMessage = "Sometimes :) Windows returns only first 15 chars for a session ClientName")]
    public string ClientName { get; set; } = ClientNameBase + Interlocked.Increment(ref ConnectionId) % 1e4;
    public bool FontSmoothing { get; set; }
}
