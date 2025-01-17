using FinLegal.Search.Indexer.Data.Entities;

namespace FinLegal.Search.Indexer.Data.Readers;

public interface IContactQuery
{
    Task<Contact> GetById(Guid contactId);
    IAsyncEnumerable<List<Contact>> GetContactBatches(Guid companyId, int batchSize, bool includeScrubbed = false);
}