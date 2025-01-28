using FinLegal.Search.Indexer.Indexers;
using FinLegal.Search.Messages;
using MassTransit;

namespace FinLegal.Search.Indexer.Consumers;

public class SimpleContactReindexRequestConsumer(ILogger<SimpleContactReindexRequestConsumer> logger, IContactSearchIndexer contactSearchIndexer) : IConsumer<SimpleContactFullIndexRequest>
{
    public async Task Consume(ConsumeContext<SimpleContactFullIndexRequest> context)
    {
        switch (context.Message.Type)
        {
            case ReindexType.Global:
                logger.LogInformation($"Global basic contact search reindex request received for {context.Message.CompanyId}");
                await contactSearchIndexer.GlobalReindex();
                break;
            case ReindexType.Parity:
                logger.LogInformation($"Parity basic contact search reindex request received");
                await contactSearchIndexer.ParityIndex();
                break;
            case ReindexType.Single:
                if (context.Message.CompanyId == null)
                {
                    throw new ArgumentException("CompanyId must be specified for CompanyContactSearch reindexing");
                }
                logger.LogInformation($"Company basic contact search reindex request received for {context.Message.CompanyId}");
                await contactSearchIndexer.CompanyReindex(context.Message.CompanyId.Value);
                break;
        }
    }
}