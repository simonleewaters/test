using FinLegal.Search.Models;
using OpenSearch.Client;

namespace FinLegal.Search.Shared.IndexModels;

[OpenSearchType]
public record ContactIndexItem : BaseIndexItem
{
    [Text]
    public required Guid CompanyId { get; set; }
    [Text]
    public string? FirstName { get; set; }
    [Text]
    public string? LastName { get; set; }
    [Text]
    public string? MiddleName { get; set; }
    [Text]
    public string? Email { get; set; }
    [Text]
    public string? CompanyName { get; set; }
    [Text]
    public string? Phone { get; set; }
    [Text]
    public string? SecondaryPhone { get; set; }
    [Text]
    public string? AddressLine1 { get; set; }
    [Text]
    public string? AddressLine2 { get; set; }
    [Text]
    public string? City { get; set; }
    [Text]
    public string? State { get; set; }
    [Text]
    public string? Country { get; set; }
    [Text]
    public string? PostCode { get; set; }
    [Number]
    public ContactType ContactType { get; set; }
}