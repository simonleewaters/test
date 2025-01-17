using FinLegal.Search.Indexer.Data.Entities;

namespace FinLegal.Search.Indexer.Data.Readers;

public interface ICompanyQuery
{
    Task<List<Company>> GetAll();
    Task<Company> GetById(Guid companyId);
}