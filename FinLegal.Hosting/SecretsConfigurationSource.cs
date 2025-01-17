using Amazon.Runtime;
using Amazon.SecretsManager;
using Microsoft.Extensions.Configuration;

namespace FinLegal.Hosting;

public class SecretsConfigurationSource(IConfiguration configuration) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new SecretsConfigurationProvider(CreateClient(), configuration);
    }

    private AmazonSecretsManagerClient CreateClient()
    {
        var clientConfig = new AmazonSecretsManagerConfig { RegionEndpoint = configuration.GetAwsRegionEndpoint() };

        var credentials = configuration.GetAwsCredentials() ?? FallbackCredentialsFactory.GetCredentials();
        return new AmazonSecretsManagerClient(credentials, clientConfig);
    }
}