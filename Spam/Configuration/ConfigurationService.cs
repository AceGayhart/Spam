using Microsoft.Extensions.Configuration;
using Serilog;

namespace Spam.Configuration;

public class ConfigurationService : IConfigurationService
{
    public void ConfigureLogger()
    {
        IConfiguration configuration = GetConfiguration();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
    }

    public IConfiguration GetConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        return builder.Build();
    }

    public Settings GetSettings()
    {
        var configuration = GetConfiguration();
        var settings = new Settings();
        configuration.Bind(settings);

        ValidateProperties(settings);
        return settings;
    }

    private static void ValidateProperties(object obj)
    {
        var properties = obj.GetType().GetProperties();

        foreach (var property in properties)
        {
            var value = property.GetValue(obj);
            var propertyType = property.PropertyType;

            if (propertyType.IsClass && !propertyType.Equals(typeof(string)))
            {
                if (value != null)
                {
                    ValidateProperties(value);
                }
            }
            else
            {
                var defaultValue = propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null;

                if (value == null || value.Equals(defaultValue))
                {
                    throw new InvalidOperationException($"Property '{property.Name}' in '{obj.GetType().Name}' cannot have a null or default value");
                }
            }
        }
    }
}