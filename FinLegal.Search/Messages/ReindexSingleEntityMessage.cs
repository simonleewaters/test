namespace FinLegal.Search.Messages;

public class ReindexSingleEntityMessage
{
    public EntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
}