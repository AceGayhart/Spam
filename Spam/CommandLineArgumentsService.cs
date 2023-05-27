using Serilog;

namespace Spam;

public class CommandLineArgumentsService
{
    public CommandLineArgumentsService(string[] args)
    {
        // If args is null or empty, set default values
        if (args == null || args.Length == 0)
        {
            ProcessSpam = true;
            ProcessResponses = true;
            ForceDailyReport = false;
            ForceMonthlyReport = false;
            return;
        }

        // Define parameter to property mapping
        var parameterToPropertyMap = new Dictionary<string, Action<bool>>
        {
            {"--process-spam", value => ProcessSpam = value},
            {"--process-responses", value => ProcessResponses = value},
            {"--force-daily-report", value => ForceDailyReport = value},
            {"--force-monthly-report", value => ForceMonthlyReport = value},
        };

        // Validate provided arguments
        foreach (string arg in args)
        {
            if (!parameterToPropertyMap.ContainsKey(arg))
            {
                throw new ArgumentException($"Invalid parameter: {arg}");
            }
        }

        // Set properties and log parameter values
        foreach (var parameter in parameterToPropertyMap.Keys)
        {
            bool value = args.Contains(parameter);
            parameterToPropertyMap[parameter](value);
            Log.Debug("Parameter {Parameter} is set to {Value}", parameter, value);
        }
    }

    public bool ForceDailyReport { get; private set; }
    public bool ForceMonthlyReport { get; private set; }
    public bool ProcessResponses { get; private set; }
    public bool ProcessSpam { get; private set; }
}