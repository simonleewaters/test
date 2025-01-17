namespace FinLegal.Search.Indexer.Data.Entities;

public class Company
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required DateTime CreationDateTime { get; set; }
}