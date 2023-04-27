using Microsoft.Extensions.Configuration;

namespace Spam.Configuration;

public interface IConfigurationService
{
    void ConfigureLogger();

    IConfiguration GetConfiguration();

    Settings GetSettings();
}