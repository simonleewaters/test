using Microsoft.Extensions.Configuration;

namespace FinLegal.Hosting;

public static class SecretsConfigurationExtensions
{
    public static IConfigurationBuilder AddSecrets(this IConfigurationBuilder builder, IConfiguration configuration)
    {
        builder.Add(new SecretsConfigurationSource(configuration));
        return builder;
    }
}