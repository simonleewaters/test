using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using FinLegal.Hosting;
using FinLegal.Hosting.EnvFile;
using FinLegal.Search.Indexer.Consumers;
using FinLegal.Search.Indexer.Data;
using FinLegal.Search.Indexer.Data.Readers;
using FinLegal.Search.Indexer.Indexers;
using FinLegal.Search.Shared.OpenSearch;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddEnvFile();

var configurationManager = builder.Configuration;
builder.Configuration.AddSecrets(configurationManager);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SimpleContactReindexRequestConsumer>();
    x.AddConsumer<ReindexSingleEntityConsumer>();
    
    x.SetKebabCaseEndpointNameFormatter();

    x.UsingAmazonSqs(
        (context, cfg) =>
        {
            var configuration = context.GetRequiredService<IConfiguration>();

            var regionEndpoint = configuration.GetAwsRegionEndpoint();
            cfg.Host(
                regionEndpoint.SystemName,
                h =>
                {
                    if (configuration.IsDeployed())
                    {
                        h.Credentials(FallbackCredentialsFactory.GetCredentials());
                    }
                    else
                    {
                        var credentials = configuration.GetAwsCredentials();
                        var accessKey = credentials?.GetCredentials().AccessKey ?? "test";
                        var secretKey = credentials?.GetCredentials().SecretKey ?? "test";
                        h.AccessKey(accessKey);
                        h.SecretKey(secretKey);

                        // redirect to localstack - need to configure this really
                        h.Config(new AmazonSQSConfig { ServiceURL = "http://localhost:4566" });
                        h.Config(new AmazonSimpleNotificationServiceConfig { ServiceURL = "http://localhost:4566" });
                    }

                    // specify a scope for all queues
                    h.Scope(configuration.GetDeploymentName());

                    // scope topics as well
                    h.EnableScopedTopics();
                }
            );
            
            cfg.ReceiveEndpoint("search-reindex", e =>
            {
                e.ConfigureConsumer<SimpleContactReindexRequestConsumer>(context);
            });
            
            cfg.ReceiveEndpoint("search-indexing", e =>
            {
                e.ConfigureConsumer<ReindexSingleEntityConsumer>(context);
            });
        }
    );
});

builder.Services.AddOpenSearch();
builder.Services.AddCommon();

builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();
builder.Services.AddSingleton<IContactQuery, ContactQuery>();
builder.Services.AddSingleton<ICompanyQuery, CompanyQuery>();
builder.Services.AddSingleton<IContactSearchIndexer, ContactSearchIndexer>();

var host = builder.Build();
host.Run();