namespace FinLegal.Search.Models;

public record ContactSearchResult(
    Guid Id,
    Guid CompanyId,
    string? FirstName,
    string? LastName,
    string? MiddleName,
    string? Email,
    string? CompanyName,
    string? Phone,
    string? SecondaryPhone,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? Country,
    string? PostCode,
    ContactType ContactType,
    DateTime CreationDateTime);