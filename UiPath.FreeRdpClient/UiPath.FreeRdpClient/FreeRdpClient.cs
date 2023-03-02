using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nito.Disposables;
using System.Net.Sockets;


namespace UiPath.Rdp;


public static class FreeRdpClient
{
    static FreeRdpClient()
    {
        //Make sure winsock is initialized
        using var _ = new TcpClient();
    }

    public static async Task<IAsyncDisposable> Connect(RdpConnectionSettings connectionSettings)
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


    private static void Disconnect(string releaseObjectName)
    {
        if (releaseObjectName != default)
            NativeInterface.RdpRelease(releaseObjectName);
    }

    public static IServiceCollection AddFreeRdp(this IServiceCollection services, string scopeName = "RunId")
    {
        Logging.ScopeName = scopeName;
        return services.AddHostedService<FreeRdpInitilizer>();
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
            Logging.SetupLogging(_loggerFactory);
            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            Logging.SetupLogging(null);
            return Task.CompletedTask;
        }
    }
}
