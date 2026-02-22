using System.IO;
using System.Text;

namespace WaterAssessment.Helpers;

public static class StartupLogger
{
    private static readonly object Sync = new();

    private static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WaterAssessment",
        "logs");

    private static readonly string LogFilePath = Path.Combine(LogDirectory, "startup.log");

    public static string CurrentLogFilePath => LogFilePath;

    public static void Info(string message)
    {
        Write("INFO", message);
    }

    public static void Warn(string message)
    {
        Write("WARN", message);
    }

    public static void Error(string message, Exception? exception = null)
    {
        var details = exception is null ? message : $"{message}{Environment.NewLine}{exception}";
        Write("ERROR", details);
    }

    private static void Write(string level, string message)
    {
        try
        {
            lock (Sync)
            {
                Directory.CreateDirectory(LogDirectory);
                var line = new StringBuilder()
                    .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"))
                    .Append(" [")
                    .Append(level)
                    .Append("] ")
                    .Append(message)
                    .ToString();

                File.AppendAllText(LogFilePath, line + Environment.NewLine, Encoding.UTF8);
            }
        }
        catch
        {
            // Do not throw from logger.
        }
    }
}