using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Humanizer;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FinLegal.Hosting;

public class SecretsConfigurationProvider(IAmazonSecretsManager amazonSecretsManager, IConfiguration configuration)
    : ConfigurationProvider
{
    public override void Load()
    {
        LoadAsync().GetAwaiter().GetResult();
    }

    private async Task LoadAsync()
    {
        Data.Clear();
        var deployment = configuration.GetDeploymentName();
        var secretId = "FinLegal-claimsautomation-" + deployment.Humanize().ToLowerInvariant();
        try
        {
            var getSecretValueResponse = await amazonSecretsManager.GetSecretValueAsync(new GetSecretValueRequest() { SecretId = secretId });
            var json = JsonConvert.DeserializeObject<JObject>(getSecretValueResponse.SecretString);

            Data.Add("secrets", getSecretValueResponse.SecretString);

            foreach (var jProperty in json?.Properties() ?? Array.Empty<JProperty>())
            {
                Data.Add(jProperty.Name.ToUpperInvariant(), jProperty.Value.ToString());
            }
        }
        catch (Exception e)
        {
            if (configuration.IsDeployed())
            {
                Console.WriteLine(secretId);
                Console.WriteLine(e);
            }
        }
    }
}