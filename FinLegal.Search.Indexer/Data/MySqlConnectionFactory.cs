using System.Data;
using MySql.Data.MySqlClient;

namespace FinLegal.Search.Indexer.Data;

public class MySqlConnectionFactory(IConfiguration configuration) : IDbConnectionFactory
{
    public IDbConnection GetConnection()
    {
        return new MySqlConnection(configuration.GetValue<string>("CONNECTION_STRING"));
    }
}