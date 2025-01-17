using FinLegal.Search.Models;

namespace FinLegal.Search.Indexer.Data.Entities;

public class Contact
{
    public required Guid Id { get; set; }
    public required Guid CompanyId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? Email { get; set; }
    public string? CompanyName { get; set; }
    public string? Phone { get; set; }
    public string? SecondaryPhone { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostCode { get; set; }
    public ContactType ContactType { get; set; }
    public required long RowVersion { get; set; }
    public required DateTime CreationDateTime { get; set; }
    public bool IsScrubbed { get; set; }
}