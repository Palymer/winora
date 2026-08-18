using WindowsOptimizer.Core;
using WindowsOptimizer.Core.Interfaces;

namespace WindowsOptimizer.Infrastructure.Logging;

public sealed class FileOperationLogger : IOperationLogger
{
    private readonly object _sync = new();
    private readonly string _filePath;

    public FileOperationLogger()
    {
        AppPaths.EnsureCreated();
        LogDirectory = AppPaths.Logs;
        _filePath = Path.Combine(LogDirectory, $"optimizer-{DateTime.Now:yyyyMMdd}.log");
    }

    public string LogDirectory { get; }

    public void Info(string message) => Write("INFO", message, null);

    public void Warning(string message) => Write("WARN", message, null);

    public void Error(string message, Exception? exception = null) => Write("ERROR", message, exception);

    private void Write(string level, string message, Exception? exception)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
        if (exception is not null)
        {
            line += $"{Environment.NewLine}{exception}";
        }

        lock (_sync)
        {
            File.AppendAllText(_filePath, line + Environment.NewLine);
        }
    }
}
