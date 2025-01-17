using FinLegal.Search.Indexer.Indexers;
using FinLegal.Search.Messages;
using MassTransit;

namespace FinLegal.Search.Indexer.Consumers;

public class ReindexSingleEntityConsumer(ILogger<ReindexSingleEntityConsumer> logger, IContactSearchIndexer contactSearchIndexer) : IConsumer<ReindexSingleEntityMessage>
{
    public async Task Consume(ConsumeContext<ReindexSingleEntityMessage> context)
    {
        switch (context.Message.EntityType)
        {
            case EntityType.Contact:
                logger.LogInformation(
                    $"Single contact reindex request received for contact ID: {context.Message.EntityId}");
                await contactSearchIndexer.ContactReindex(context.Message.EntityId);
                // Contact filter reindex
                break;
            case EntityType.Claim:
                logger.LogInformation(
                    $"Single claim reindex request received for claim ID: {context.Message.EntityId}");
                // Basic claim reindex
                // Claim filter reindex 
                break;
            case EntityType.Activity:
                logger.LogInformation(
                    $"Single activity reindex request received for activity ID: {context.Message.EntityId}");
                // Activity filter reindex
                break;
        }
    }
}