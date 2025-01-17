using Microsoft.Extensions.Configuration;

namespace FinLegal.Hosting.EnvFile;

public static class ServiceCollectionExtensions
{
    public static IConfigurationBuilder AddEnvFile(this IConfigurationBuilder builder, string file = ".env")
    {
        if (!Path.IsPathRooted(file))
            file = Path.Combine(Directory.GetCurrentDirectory(), file);
        if (File.Exists(file))
            builder.Add((IConfigurationSource) new EnvFileConfigurationSource(file));
        return builder;
    }
}