namespace FinLegal.SearchService;

public record BasicSearchRequest(
    string SearchPhrase,
    Guid CompanyId,
    int Page = 1,
    int PageSize = 100);