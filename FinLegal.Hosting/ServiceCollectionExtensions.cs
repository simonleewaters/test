using Microsoft.Extensions.DependencyInjection;

namespace FinLegal.Hosting;

public static class ServiceCollectionExtensions
{
    public static void AddCommon(this IServiceCollection services)
    {
        services.AddSingleton<DisputedEnvironment>();
    }
}