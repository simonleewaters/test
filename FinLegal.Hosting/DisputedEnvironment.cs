using Microsoft.Extensions.Configuration;

namespace FinLegal.Hosting;

public class DisputedEnvironment
{

    public DisputedEnvironment(IConfiguration configuration)
    {
        ApplicationName = configuration.GetApplicationName();
        DeploymentName = configuration.GetDeploymentName();
        Deployment = configuration.GetDeployment();
        IsDeployed = configuration.IsDeployed();

        GitCommit = Environment.GetEnvironmentVariable("git_commit") ?? "Unknown";
        if (GitCommit.StartsWith("git-", StringComparison.Ordinal))
        {
            GitCommit = GitCommit.Substring(4);
        }
    }

    public string ApplicationName { get; }
    public DisputedDeployment Deployment { get; }
    public string DeploymentName { get; }
    public string GitCommit { get; }

    public bool IsProduction() =>
        Deployment switch
        {
            DisputedDeployment.London => true,
            DisputedDeployment.Sydney => true,
            DisputedDeployment.Ohio => true,
            _ => false
        };

    public bool IsLocal() =>
        Deployment switch
        {
            DisputedDeployment.Local => true,
            _ => false
        };

    public bool IsDeployed { get; }

    public bool IsProdDeployed() =>
        Deployment switch
        {
            DisputedDeployment.Staging => true,
            DisputedDeployment.Sydney => true,
            DisputedDeployment.London => true,
            DisputedDeployment.Ohio => true,
            _ => false
        };
}
