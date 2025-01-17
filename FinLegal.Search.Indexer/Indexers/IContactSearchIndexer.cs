namespace FinLegal.Search.Indexer.Indexers;

public interface IContactSearchIndexer
{
    Task GlobalReindex();
    Task ParityIndex();
    Task CompanyReindex(Guid companyId);
    Task ContactReindex(Guid contactId);
}