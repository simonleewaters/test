namespace FinLegal.Search.Messages;

public class SimpleContactFullIndexRequest
{
    public ReindexType Type { get; set; }
    public Guid? CompanyId { get; set; }
}