using System.Diagnostics;
using FinLegal.Search.Indexer.Data.Entities;
using FinLegal.Search.Indexer.Data.Readers;
using FinLegal.Search.Shared;
using FinLegal.Search.Shared.IndexModels;
using OpenSearch.Client;
using OpenSearch.Net;

namespace FinLegal.Search.Indexer.Indexers;

public class ContactSearchIndexer(ILogger<ContactSearchIndexer> logger,
    IOpenSearchClient openSearchClient,
    IContactQuery contactQuery, ICompanyQuery companyQuery) : IContactSearchIndexer
{
    public async Task GlobalReindex()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        logger.LogInformation($"Beginning global Basic Contact Search reindex");

        var companies = await companyQuery.GetAll();

        foreach (var company in companies)
        {
            await CompanyReindex(company.Id);
        }
        
        stopwatch.Stop();
        logger.LogInformation($"Global basic Contact Search reindex complete in {stopwatch.Elapsed:d\\.hh\\:mm\\:ss}");
    }
    
    public async Task ParityIndex()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        logger.LogInformation($"Beginning parity Basic Contact Search reindex");

        var companies = await companyQuery.GetAll();

        foreach (var company in companies)
        {
            await CompanyParityIndex(company.Id);
        }
        
        stopwatch.Stop();
        logger.LogInformation($"Global parity Contact Search reindex complete in {stopwatch.Elapsed:d\\.hh\\:mm\\:ss}");
    }

    public async Task CompanyReindex(Guid companyId)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var companyDetails = await companyQuery.GetById(companyId);
        
        logger.LogInformation($"Beginning full Contact Search reindex for company: {companyDetails.Name} ({companyId})");
        var tempId = Guid.NewGuid().ToString("N");

        var activeIndexName = $"search-contacts_{companyId}";
        var buildingIndexName = $"search-contacts_{companyId}_building";
        var tempIndexName = $"search-contacts_{companyId}_{tempId}";
        
        var buildingIndexExistsResult = await openSearchClient.Indices.ExistsAsync(buildingIndexName);

        // if (buildingIndexExistsResult.Exists)
        // {
        //     var buildingIndex = await openSearchClient.Indices.GetAsync(buildingIndexName);
        //     await openSearchClient.Indices.DeleteAsync(buildingIndex.Indices.First().Key);
        // }

        if (buildingIndexExistsResult.Exists)
        {
            logger.LogWarning($"Full Contact Search reindex already in progress for company: {companyDetails.Name} ({companyId}), request cancelled");
        }
        else
        {
            await openSearchClient.Indices.CreateAsync(tempIndexName,
                c => c.Map(m => m.AutoMap<ContactIndexItem>()));
            await openSearchClient.Indices.PutAliasAsync(tempIndexName, buildingIndexName);
        
            logger.LogInformation($"Created temporary index: {tempIndexName}");
        
            var activeIndexExistsResult = await openSearchClient.Indices.ExistsAsync(activeIndexName);
            GetIndexResponse? activeIndex = null;
            if (activeIndexExistsResult.Exists)
            {
                activeIndex = await openSearchClient.Indices.GetAsync(activeIndexName);
            }
        
            try
            {
                var batchSize = 8000;
        
                var processedCount = 0;
        
                await foreach (var batch in contactQuery.GetContactBatches(companyId, batchSize))
                {
                    var result = openSearchClient.BulkAll(batch.Select(c => c.ToIndexItem()), r => r
                        .Index(tempIndexName)
                        .BackOffRetries(2)
                        .BackOffTime("30s")
                        .MaxDegreeOfParallelism(4)
                        .Size(1000));
        
                    _ = result.Wait(
                        TimeSpan.FromMinutes(10),
                        a =>
                        {
                        });
        
                    processedCount += batch.Count;
                    
                    logger.LogInformation($"Processed {processedCount} contacts");
                }
                
                var aliasRequest = new BulkAliasRequest { Actions = [] };
        
                if (activeIndex != null)
                {
                    aliasRequest.Actions.Add(new AliasRemoveAction()
                    {
                        Remove = new AliasRemoveOperation()
                        {
                            Index = activeIndex.Indices.First().Key,
                            Alias = activeIndexName
                        }
                    });
                }
        
                aliasRequest.Actions.Add(new AliasAddAction()
                {
                    Add = new AliasAddOperation()
                    {
                        Index = tempIndexName,
                        Alias = activeIndexName
                    }
                });
        
                logger.LogInformation($"Added active alias to index: {tempIndexName} -> {activeIndexName}");
        
                await openSearchClient.Indices.BulkAliasAsync(aliasRequest);
                
                await openSearchClient.Indices.DeleteAsync($"{activeIndexName}*,-{tempIndexName}", d => d.ExpandWildcards(ExpandWildcards.All));
                await openSearchClient.Indices.DeleteAliasAsync(tempIndexName, buildingIndexName);
                logger.LogInformation($"Removed building alias from index: {tempIndexName} -> {buildingIndexName}");
            }
            catch (Exception exception)
            {
                await openSearchClient.Indices.DeleteAsync($"{tempIndexName}");
                logger.LogError($"Error occurred during full Contact Search reindex for company: {companyDetails.Name} ({companyId})",
                    exception);
            }
        }

        stopwatch.Stop();
        logger.LogInformation($"Full Contact Search reindex complete for company: {companyDetails.Name} ({companyId}) in {stopwatch.Elapsed:d\\.hh\\:mm\\:ss}");
    }

    private async Task CompanyParityIndex(Guid companyId)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var companyDetails = await companyQuery.GetById(companyId);
        
        logger.LogInformation($"Beginning full Contact Search reindex for company: {companyDetails.Name} ({companyId})");

        var activeIndexName = $"search-contacts_{companyId}";
        var buildingIndexName = $"search-contacts_{companyId}_building";
        
        var buildingIndexExistsResult = await openSearchClient.Indices.ExistsAsync(buildingIndexName);

        // if (buildingIndexExistsResult.Exists)
        // {
        //     var buildingIndex = await openSearchClient.Indices.GetAsync(buildingIndexName);
        //     await openSearchClient.Indices.DeleteAsync(buildingIndex.Indices.First().Key);
        // }

        if (buildingIndexExistsResult.Exists)
        {
            logger.LogWarning($"Full Contact Search reindex in progress for company: {companyDetails.Name} ({companyId}), request cancelled");
        }
        else
        {
            var activeIndexExistsResult = await openSearchClient.Indices.ExistsAsync(activeIndexName);

            if (!activeIndexExistsResult.Exists)
            {
                logger.LogError($"No active Contact Search index exists for company: {companyDetails.Name} ({companyId}), request cancelled");
            }
            else
            {
                try
                {
                    var batchSize = 8000;
                    // var contactCount = await contactQuery.GetCount(companyId);
                    // var pages = Math.Ceiling((double) contactCount / pageSize);
                    // logger.LogInformation($"Processing {contactCount} contacts with a page size of {pageSize} over {pages} page(s)");

                    var updatedCount = 0;
                    var processedCount = 0;
                    await foreach (var batch in contactQuery.GetContactBatches(companyId, batchSize, true))
                    {
                        var results = openSearchClient.Search<ContactIndexItem>(
                            x =>
                                x.Index(activeIndexName)
                                    .Query(q => q.Ids(id => id.Values(batch.Select(c => c.Id))))
                                    .Size(batchSize));

                        var contactsToProcess = (from c in batch

                            join cii in results.Documents on c.Id equals cii.Id into contactIndexItems
                            from ciiResult in contactIndexItems.DefaultIfEmpty()
                            where c.IsScrubbed && ciiResult != null ||
                                  !c.IsScrubbed && ciiResult == null ||
                                  !c.IsScrubbed && ciiResult.RowVersion < c.RowVersion

                            select c).ToList();

                        // var contactsToProcess = new List<Contact>();
                        //
                        // var deletedContacts = contacts.Where(c => c.IsScrubbed && results.Documents.Any(d => d.Id == c.Id)).ToList();
                        // contactsToProcess.AddRange(deletedContacts);
                        // var newContacts = contacts.Where(c => !c.IsScrubbed && results.Documents.All(d => d.Id != c.Id)).ToList();
                        // contactsToProcess.AddRange(newContacts);
                        // var changedContacts = contacts.Where(c => !c.IsScrubbed && results.Documents.Any(d => d.Id == c.Id) &&
                        //                                           results.Documents.Single(d => d.Id == c.Id).RowVersion <
                        //                                           c.RowVersion).ToList();
                        // contactsToProcess.AddRange(changedContacts);

                        foreach (var contact in contactsToProcess)
                        {
                            await ContactReindexInternal(contact,
                                results.Documents.SingleOrDefault(d => d.Id == contact.Id));
                        }

                        updatedCount += contactsToProcess.Count;
                        processedCount += batch.Count;

                        logger.LogInformation($"Processed {processedCount} contacts, updated {updatedCount}");
                    }

                    // Guid? lastId = null;
                    //
                    // for (var currentPage = 0; currentPage < pages; currentPage++)
                    // {
                    //     logger.LogInformation($"Processing page {currentPage + 1}/{pages}");
                    //     var contacts = await contactQuery.GetContactBatch(companyId, lastId, pageSize, true);
                    //     var results = openSearchClient.Search<ContactIndexItem>(
                    //         x =>
                    //             x.Index(activeIndexName)
                    //                 .Query(q => q.Ids(id => id.Values(contacts.Select(c => c.Id))))
                    //                 .Size(pageSize)
                    //     );
                    //
                    //     var contactsToProcess = from c in contacts
                    //
                    //         join cii in results.Documents on c.Id equals cii.Id into contactIndexItems
                    //         from ciiResult in contactIndexItems.DefaultIfEmpty()
                    //         where c.IsScrubbed && ciiResult != null ||
                    //               !c.IsScrubbed && ciiResult == null ||
                    //               !c.IsScrubbed && ciiResult.RowVersion < c.RowVersion
                    //
                    //         select c;
                    //
                    //     // var contactsToProcess = new List<Contact>();
                    //     //
                    //     // var deletedContacts = contacts.Where(c => c.IsScrubbed && results.Documents.Any(d => d.Id == c.Id)).ToList();
                    //     // contactsToProcess.AddRange(deletedContacts);
                    //     // var newContacts = contacts.Where(c => !c.IsScrubbed && results.Documents.All(d => d.Id != c.Id)).ToList();
                    //     // contactsToProcess.AddRange(newContacts);
                    //     // var changedContacts = contacts.Where(c => !c.IsScrubbed && results.Documents.Any(d => d.Id == c.Id) &&
                    //     //                                           results.Documents.Single(d => d.Id == c.Id).RowVersion <
                    //     //                                           c.RowVersion).ToList();
                    //     // contactsToProcess.AddRange(changedContacts);
                    //
                    //     foreach (var contact in contactsToProcess)
                    //     {
                    //         await ContactReindexInternal(contact, results.Documents.SingleOrDefault(d => d.Id == contact.Id));
                    //     }
                    //     
                    //     lastId = contacts.Last().Id;
                    // }
                }
                catch (Exception exception)
                {
                    logger.LogError(
                        $"Error occurred during parity Basic Contact Search reindex for company: {companyDetails.Name} ({companyId})",
                        exception);
                }
            }
        }

        stopwatch.Stop();
        logger.LogInformation($"Parity Basic Contact Search reindex complete for company: {companyDetails.Name} ({companyId}) in {stopwatch.Elapsed:d\\.hh\\:mm\\:ss}");
    }

    public async Task ContactReindex(Guid contactId)
    {
        var contact = await contactQuery.GetById(contactId);
        
        logger.LogInformation($"Beginning single contact indexing for contactId: {contactId}");
        
        var indexName = $"search-contacts_{contact.CompanyId}";

        var indexExists = await openSearchClient.Indices.ExistsAsync(indexName);

        if (indexExists.Exists)
        {
            var contactIndexItemResponse =
                await openSearchClient.GetAsync<ContactIndexItem>(contactId, index => index.Index(indexName));
            ContactIndexItem? contactIndexItem = null;
            if (contactIndexItemResponse.Found)
            {
                contactIndexItem = contactIndexItemResponse.Source;
            }
            
            await ContactReindexInternal(contact, contactIndexItem);
        }
        logger.LogInformation($"Single contact indexing complete for contactId: {contactId}");
    }

    private async Task ContactReindexInternal(Contact contact, ContactIndexItem? contactIndexItem)
    {
        var indexName = $"search-contacts_{contact.CompanyId}";
        var contactId = contact.Id;

        if (contactIndexItem != null && contact.IsScrubbed)
        {        
            await openSearchClient.DeleteAsync<ContactIndexItem>(contactId,
                index => index.Index(indexName));
        }
        else if (!contact.IsScrubbed && (contactIndexItem == null || contactIndexItem.RowVersion < contact.RowVersion))
        {
            await openSearchClient.UpdateAsync<ContactIndexItem>(contact.Id,
                update =>
                    update.Doc(contact.ToIndexItem())
                        .DocAsUpsert()
                        .WaitForActiveShards(openSearchClient.Nodes.Stats().Nodes.Count > 1 ? "all" : "1")
                        .RetryOnConflict(2)
                        .Index(indexName));
        }
    }
}