using Microsoft.Extensions.Configuration;

namespace Spam;

public interface IConfigurationService
{
    void ConfigureLogger();

    IConfiguration GetConfiguration();

    Settings GetSettings();
}