using FinLegal.Search.Shared;
using FinLegal.Search.Messages;
using FinLegal.Search.Models;
using FinLegal.Search.Shared.IndexModels;
using MassTransit;
using OpenSearch.Client;
using OpenSearch.Net;

namespace FinLegal.SearchService;

public class BasicContactSearch(ISendEndpointProvider sendEndpointProvider, IOpenSearchClient openSearchClient) : IBasicSearch<ContactSearchResult>
{
    public async Task<SearchResult<ContactSearchResult>> Search(
        BasicSearchRequest searchRequest
    )
    {
        var indexName = $"search-contacts_{searchRequest.CompanyId}";

        var indexExists = await openSearchClient.Indices.ExistsAsync(indexName);

        if (!indexExists.Exists)
        {
            return SearchResult<ContactSearchResult>.EmptyResult;
        }

        var results = openSearchClient.Search<ContactIndexItem>(
            x =>
                x.Index(indexName)
                    .Query(q => BuildQuery(searchRequest.SearchPhrase, q))
                    .Sort(s => s.Descending(d => d.CreationDateTime))
                    .Size(searchRequest.PageSize)
                    .Skip((searchRequest.Page - 1) * searchRequest.PageSize)
        );

        // if (!res.IsValid) // no response here when deleting scroll
        // {
        //     _logger.LogWarning($"{res.DebugInformation}");
        //     _logger.LogWarning(res.ServerError != null ? $"{JsonConvert.SerializeObject(res.ServerError)}" : "No server error");
        //
        //     _logger.LogWarning($"Searched phrase: {phrase}"); // dont stop execution
        // }

        return results.Documents.Count != 0
            ? new SearchResult<ContactSearchResult>
            {
                Count = results.Total, Page = searchRequest.Page, Results = results.Documents.Select(d =>
                    new ContactSearchResult(
                        d.Id, d.CompanyId, d.FirstName, d.LastName, d.MiddleName, d.Email, d.CompanyName, d.Phone,
                        d.SecondaryPhone, d.AddressLine1, d.AddressLine2, d.City,
                        d.State, d.Country, d.PostCode, d.ContactType, d.CreationDateTime)).ToList()
            }
            : SearchResult<ContactSearchResult>.EmptyResult;
    }

    public async Task<List<string>> GetIndexes()
    {
        var indexName = $"search-contacts_";

        var indexes = await openSearchClient.Indices.GetAsync($"{indexName}*", d => d.ExpandWildcards(ExpandWildcards.All));
        return indexes.Indices.Select(i => $"{i.Key} ({string.Join(", ", i.Value.Aliases.Select(a => a.Key))})").ToList();
    }

    public async Task Reindex(Guid? companyId)
    {
        var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri("queue:search-reindex"));

        if (companyId.HasValue)
        {
            await sendEndpoint.Send(new SimpleContactFullIndexRequest { Type = ReindexType.Single, CompanyId = companyId });
        }
        else
        {
            await sendEndpoint.Send(new SimpleContactFullIndexRequest { Type = ReindexType.Global });
        }
    }

    public async Task ParityIndex()
    {
        var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri("queue:search-reindex"));
        
        await sendEndpoint.Send(new SimpleContactFullIndexRequest { Type = ReindexType.Parity });
    }

    public async Task SingleIndex(Guid entityId)
    {
        var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri("queue:search-indexing"));
        await sendEndpoint.Send(new ReindexSingleEntityMessage { EntityId = entityId });
    }

    private static readonly List<string> CharsToEscape =
    [
        "\\",
        "(",
        ")",
        "{",
        "}",
        "[",
        "]",
        "^",
        "~",
        "*",
        "?",
        ":",
        "\"",
        "/",
        "!"
    ];

    private static string EscapeParam(string param)
    {
        if (string.IsNullOrEmpty(param))
        {
            return param;
        }

        foreach (var charToEscape in CharsToEscape)
        {
            param = param.Replace(charToEscape, $"\\{charToEscape}");
        }

        return param;
    }

    private QueryContainer BuildQuery(string searchPhrase, QueryContainerDescriptor<ContactIndexItem> q)
    {
        string[] fields =
        [
            "id", "firstName", "lastName", "middleName", "email", "companyName", "phone", "secondaryPhone", 
            "addressLine1", "addressLine2", "city", "state", "country", "postcode"
        ];

        var phrases = searchPhrase.Split(" ").Distinct().Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => $"*{p}*");

        var phraseQueries = new List<QueryContainer>();
        
        foreach (var phrase in phrases)
        {
            var fieldQueries = new List<QueryContainer>();
            foreach (var field in fields)
            {
                fieldQueries.Add(new WildcardQuery() {Field = field, Value = phrase});
            }

            var boolQuery = new BoolQuery { Should = fieldQueries };
            phraseQueries.Add(boolQuery);
        }
        
        return q.Bool(b => b.Must(phraseQueries.ToArray()));
    }
}