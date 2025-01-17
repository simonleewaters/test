using Microsoft.Extensions.Configuration;

namespace FinLegal.Hosting.EnvFile;

public class EnvFileConfigurationSource : IConfigurationSource
{
    private readonly string _path;

    public EnvFileConfigurationSource(string path) => this._path = path;

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return (IConfigurationProvider) new EnvFileConfigurationProvider(this._path);
    }
}