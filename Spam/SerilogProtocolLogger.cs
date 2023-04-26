using MailKit;
using Serilog;
using System.Text;

namespace Spam;

public class SerilogProtocolLogger : IProtocolLogger
{
    private static readonly ILogger Log = Serilog.Log.ForContext<SerilogProtocolLogger>();
    private readonly StringBuilder _logBuilder;

    public SerilogProtocolLogger()
    {
        _logBuilder = new StringBuilder();
    }

    public IAuthenticationSecretDetector? AuthenticationSecretDetector { get; set; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public void LogClient(byte[] buffer, int offset, int count)
    {
        WriteLog("C: ", buffer, offset, count);
    }

    public void LogConnect(Uri uri)
    {
        Log.Debug("Connected to {Uri}", uri);
    }

    public void LogServer(byte[] buffer, int offset, int count)
    {
        WriteLog("S: ", buffer, offset, count);
    }

    private void WriteLog(string prefix, byte[] buffer, int offset, int count)
    {
        for (int i = 0; i < count; i++)
        {
            byte b = buffer[offset + i];
            if (b == (byte)'\r')
            {
                // Ignore '\r'
            }
            else if (b == (byte)'\n')
            {
                Log.Verbose("{Prefix}{Message}", prefix, _logBuilder.ToString());
                _logBuilder.Clear();
            }
            else
            {
                _logBuilder.Append((char)b);
            }
        }
    }
}