using Dapper;
using FinLegal.Search.Indexer.Data.Entities;

namespace FinLegal.Search.Indexer.Data.Readers;

public class ContactQuery(IDbConnectionFactory connectionFactory) : IContactQuery
{
    public async IAsyncEnumerable<List<Contact>> GetContactBatches(Guid companyId,int batchSize, bool includeScrubbed = false)
    {
        var connection = connectionFactory.GetConnection();
        connection.Open();

        var queryParams = new { CompanyId = companyId, PageSize = batchSize };

        Type[] types = [typeof(Contact)];
        
        var contacts = connection.Query<Contact>($@"
SELECT      c.Id, c.CompanyId, c.FirstName, c.LastName, c.MiddleName, c.Email, c.CompanyName, 
            c.Phone, c.SecondaryPhone, c.CreationDateTime, c.RowVersion, c.ContactType, c.IsScrubbed,
            a.AddressLine1, a.AddressLine2, a.City, a.State, a.Country, a.PostCode
FROM        Contact c
LEFT JOIN   Address a ON c.AddressId = a.Id
WHERE       CompanyId = @CompanyId
{(includeScrubbed ? "" : "AND         IsScrubbed = 0")}", types, param: queryParams, map: m => (Contact)m[0], buffered: false);

        var batch = new List<Contact>();
        
        foreach (var contact in contacts)
        {
            batch.Add(contact);
            
            if (batch.Count == batchSize)
            {
                yield return batch;
                batch = [];
            }
        }

        if (batch.Count == 0)
        {
            yield break;
        }

        if (batch.Count < batchSize)
        {
            yield return batch;
        }
        
        connection.Close();
        connection.Dispose();
    }

    public async Task<Contact> GetById(Guid contactId)
    {
        using var connection = connectionFactory.GetConnection();

        var queryParams = new { ContactId = contactId };

        var contact = await connection.QuerySingleAsync<Contact>(@"
SELECT      c.Id, c.CompanyId, c.FirstName, c.LastName, c.MiddleName, c.Email, c.CompanyName, 
            c.Phone, c.SecondaryPhone, c.CreationDateTime, c.RowVersion, c.ContactType, c.IsScrubbed,
            a.AddressLine1, a.AddressLine2, a.City, a.State, a.Country, a.PostCode
FROM        Contact c
LEFT JOIN   Address a ON c.AddressId = a.Id
WHERE       c.Id = @ContactId", queryParams);

        return contact;
    }
}