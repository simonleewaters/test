namespace FinLegal.Search.Models;

public class SearchResult<T> where T : class
{
    public long Count { get; set; }
    public required List<T> Results { get; set; }
    public int Page { get; set; }

    public static SearchResult<T> EmptyResult => new() { Count = 0, Results = [], Page = 1 };
}