using OpenSearch.Client;

namespace FinLegal.Search.Shared.OpenSearch;

public interface IOpenSearchClientFactory
{
    OpenSearchClient GetClient();
}