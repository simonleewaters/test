using System.Data;

namespace FinLegal.Search.Indexer.Data;

public interface IDbConnectionFactory
{
    public IDbConnection GetConnection();
}