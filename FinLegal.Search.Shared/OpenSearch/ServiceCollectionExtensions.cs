using Microsoft.Extensions.DependencyInjection;
using OpenSearch.Client;

namespace FinLegal.Search.Shared.OpenSearch;

public static class ServiceCollectionExtensions
{
    public static void AddOpenSearch(this IServiceCollection services)
    {
        services.AddSingleton<IOpenSearchClientFactory, OpenSearchClientFactory>();
        services.AddSingleton<IOpenSearchClient>(sp => sp.GetRequiredService<IOpenSearchClientFactory>().GetClient());
    }
}