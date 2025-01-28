namespace FinLegal.Search.Messages;

public class SingleEntityReindexRequest
{
    public EntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
}