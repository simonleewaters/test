using System.ComponentModel;

namespace FinLegal.Hosting;

public enum DisputedDeployment
{
    [Description("Unknown")]
    Unknown = 0,

    [Description(ConfigurationExtensions.STAGING)]
    Staging = 2,

    [Description(ConfigurationExtensions.DEV)]
    Dev = 3,

    [Description(ConfigurationExtensions.SYDNEY)]
    Sydney = 4,

    [Description(ConfigurationExtensions.LONDON)]
    London = 5,

    [Description(ConfigurationExtensions.LOCAL)]
    Local = 6,

    [Description(ConfigurationExtensions.OHIO)]
    Ohio = 7,
}