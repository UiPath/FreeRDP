using Microsoft.Extensions.DependencyInjection;

namespace UiPath.Rdp;

public static class ServiceRegistryExtensions
{
    public static IServiceCollection AddFreeRdp(this IServiceCollection services, string scopeName = "RunId")
    {
        Logging.ScopeName = scopeName;
        return services
            .AddSingleton<IFreeRdpClient, FreeRdpClient>()
            .AddSingleton<Logging>()
            .AddHostedService(sp => sp.GetRequiredService<Logging>());
    }
}
