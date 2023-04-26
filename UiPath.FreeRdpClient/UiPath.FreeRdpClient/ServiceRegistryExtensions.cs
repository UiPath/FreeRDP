using Microsoft.Extensions.DependencyInjection;

namespace UiPath.Rdp;

public static class ServiceRegistryExtensions
{
    public static IServiceCollection AddFreeRdp(this IServiceCollection services, string scopeName = "RunId")
    {
        Logging.ScopeName = scopeName;
        return services
            .AddSingleton<Logging>()
            .AddSingleton<IFreeRdpClient>(sp =>
            {
                sp.GetRequiredService<Logging>().EnsureNativeLogsForwarding();
                return ActivatorUtilities.CreateInstance<FreeRdpClient>(sp);
            });
    }
}
