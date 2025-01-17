using OpenSearch.Client;

namespace FinLegal.Search.Shared.IndexModels;

public record BaseIndexItem
{
    [Text]
    public required Guid Id { get; set; }
    [Number]
    public required long RowVersion { get; set; }
    [Date]
    public required DateTime CreationDateTime { get; set; }
}