using FinLegal.Search.Models;

namespace FinLegal.SearchService;

public interface IBasicSearch<T> where T : class
{
    Task<SearchResult<T>> Search(
        BasicSearchRequest searchRequest
    );

    Task<List<string>> GetIndexes();

    Task Reindex(Guid? companyId);
    Task ParityIndex();
    Task SingleIndex(Guid entityId);
}