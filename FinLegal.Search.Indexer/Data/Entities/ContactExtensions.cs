using FinLegal.Search.Shared.IndexModels;

namespace FinLegal.Search.Indexer.Data.Entities;

public static class ContactExtensions
{
    public static ContactIndexItem ToIndexItem(this Contact contact)
    {
        return new ContactIndexItem()
        {
            Id = contact.Id,
            CompanyId = contact.CompanyId,
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            MiddleName = contact.MiddleName,
            Email = contact.Email,
            CompanyName = contact.CompanyName,
            Phone = contact.Phone,
            SecondaryPhone = contact.SecondaryPhone,
            AddressLine1 = contact.AddressLine1,
            AddressLine2 = contact.AddressLine2,
            City = contact.City,
            State = contact.State,
            Country = contact.Country,
            PostCode = contact.PostCode,
            ContactType = contact.ContactType,
            RowVersion = contact.RowVersion,
            CreationDateTime = contact.CreationDateTime,
        };
    }
}