using System.Configuration;
using Amazon;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;

namespace FinLegal.Hosting;

public static class ConfigurationExtensions
{
    public const string DEV = "Dev";
    public const string STAGING = "Staging";
    public const string LOCAL = "Local";
    public const string LONDON = "Prod";
    public const string SYDNEY = "Sydney";
    public const string OHIO = "Ohio";
    
    public static AWSCredentials? GetAwsCredentials(this IConfiguration configuration)
    {
        var accessKey = configuration.GetValue<string>("ACCESS_KEY");
        var secretKey = configuration.GetValue<string>("SECRET_KEY");
        var token = configuration.GetValue<string>("AWS_TOKEN");
        if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey))
        {
            return null;
        }

        var creds = new SessionAWSCredentials(accessKey, secretKey, token);
        return creds;
    }

    public static RegionEndpoint GetAwsRegionEndpoint(this IConfiguration configuration)
    {
        return configuration.GetDeployment() switch
        {
            DisputedDeployment.Sydney => RegionEndpoint.APSoutheast2,
            DisputedDeployment.Ohio => RegionEndpoint.USEast2,
            _ => RegionEndpoint.EUWest2
        };
    }

    public static DisputedDeployment GetDeployment(this IConfiguration configuration)
    {
        if (Enum.TryParse<DisputedDeployment>(configuration.GetDeploymentName(), out var deployment))
        {
            return deployment;
        }
        return DisputedDeployment.Unknown;
    }

    public static string GetDeploymentName(this IConfiguration configuration)
    {
        var environment = configuration.GetValue<string>("ENVIRONMENT_NAME");
        return environment ?? throw new ConfigurationErrorsException("ENVIRONMENT_NAME is not configured");
    }
    public static string GetApplicationName(this IConfiguration configuration) =>
        configuration.GetValue<string>("APPLICATION_NAME") ?? "disputed";

    public static bool IsDeployed(this IConfiguration configuration)
    {
        var environment = configuration.GetValue<string>("ENVIRONMENT_NAME");
        return environment switch
        {
            "LONDON" or
                "SYDNEY" or
                "OHIO" or
                "STAGING" or
                "DEV" => true,
            _ => false
        };
    }
}