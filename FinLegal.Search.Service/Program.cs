using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using FinLegal.Hosting;
using FinLegal.Hosting.EnvFile;
using FinLegal.Search.Models;
using FinLegal.Search.Shared.OpenSearch;
using FinLegal.SearchService;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvFile();

var configurationManager = builder.Configuration;
builder.Configuration.AddSecrets(configurationManager);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddMassTransit(x =>
{
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
        }
    );
});

builder.Services.AddOpenSearch();

builder.Services.AddTransient<IBasicSearch<ContactSearchResult>, BasicContactSearch>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();