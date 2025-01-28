using Microsoft.Extensions.Configuration;
using OpenSearch.Client;
using OpenSearch.Client.JsonNetSerializer;
using OpenSearch.Net;

namespace FinLegal.Search.Shared.OpenSearch;

public class OpenSearchClientFactory(IConfiguration configuration) : IOpenSearchClientFactory
{
    public OpenSearchClient GetClient()
    {
        var uri = configuration.GetValue<string>("OPENSEARCH_URI") ?? "";
        var pool = new SingleNodeConnectionPool(new Uri(uri));
        var connectionSettings = new ConnectionSettings(pool, sourceSerializer: JsonNetSerializer.Default);
        
        return new OpenSearchClient(connectionSettings);
    }
}
