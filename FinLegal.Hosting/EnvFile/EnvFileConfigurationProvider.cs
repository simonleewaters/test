using Microsoft.Extensions.Configuration;

namespace FinLegal.Hosting.EnvFile;

public class EnvFileConfigurationProvider : ConfigurationProvider
{
    private readonly string _path;

    public EnvFileConfigurationProvider(string path) => this._path = path;

    public override void Load()
    {
        this.Data.Clear();
        if (!File.Exists(this._path))
            throw new FileNotFoundException(this._path);
        using (StreamReader streamReader = File.OpenText(this._path))
        {
            string str;
            while ((str = streamReader.ReadLine()) != null)
            {
                if (!string.IsNullOrWhiteSpace(str) && !str.StartsWith('#'))
                {
                    int length = str.IndexOf('=');
                    if (length > 0)
                        this.Data[str.Substring(0, length)] = this.PostProcessValue(str.Substring(length + 1));
                }
            }
        }
    }

    private string PostProcessValue(string value)
    {
        value = value.Trim();
        if (string.IsNullOrEmpty(value))
            return value;
        if (value.StartsWith('"'))
        {
            value = value.Trim('"');
            return value.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
        }

        if (!value.StartsWith('\''))
            return value;
        value = value.Trim('\'');
        return value.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
    }
}