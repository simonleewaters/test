namespace FinLegal.Search.Messages;

public class SimpleContactReindexRequest
{
    public ReindexType Type { get; set; }
    public Guid? CompanyId { get; set; }
}