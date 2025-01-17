using FinLegal.Search.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinLegal.SearchService;

[Route("[controller]")]
[ApiController]
public class SearchController(IBasicSearch<ContactSearchResult> contactSearch) : ControllerBase
{
    [HttpPost("contacts")]
    public async Task<ActionResult<SearchResult<ContactSearchResult>>> ContactSearch(
        BasicSearchRequest searchRequest)
    {
        var result = await contactSearch.Search(searchRequest);
        return Ok(result);
    }
    [HttpGet("indexes")]
    public async Task<ActionResult<List<string>>> GetIndexes()
    {
        var result = await contactSearch.GetIndexes();
        return Ok(result);
    }

    [HttpGet("contacts/parityindex")]
    public async Task<ActionResult> ContactParityIndex()
    {
        await contactSearch.ParityIndex();
        return Ok();
    }

    [HttpGet("contacts/reindex")]
    public async Task<ActionResult> ContactReindex([FromQuery] Guid? companyId)
    {
        await contactSearch.Reindex(companyId);
        return Ok();
    }

    [HttpGet("contacts/single-index")]
    public async Task<ActionResult> ContactSingleIndex([FromQuery] Guid contactId)
    {
        await contactSearch.SingleIndex(contactId);
        return Ok();
    }
    
}