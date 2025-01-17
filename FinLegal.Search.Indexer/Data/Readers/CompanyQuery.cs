using Dapper;
using FinLegal.Search.Indexer.Data.Entities;

namespace FinLegal.Search.Indexer.Data.Readers;

public class CompanyQuery(IDbConnectionFactory connectionFactory) : ICompanyQuery
{
    public async Task<List<Company>> GetAll()
    {
        using var connection = connectionFactory.GetConnection();

        var companies = await connection.QueryAsync<Company>(@"
SELECT      c.Id, c.Name, c.CreationDateTime
FROM        Company c
ORDER BY    c.Id");

        return companies.ToList();
    }

    public async Task<Company> GetById(Guid companyId)
    {
        using var connection = connectionFactory.GetConnection();

        var queryParams = new { CompanyId = companyId };
        
        var company = await connection.QuerySingleAsync<Company>(@"
SELECT      c.Id, c.Name, c.CreationDateTime
FROM        Company c
WHERE       c.Id = @CompanyId", queryParams);

        return company;
    }
}