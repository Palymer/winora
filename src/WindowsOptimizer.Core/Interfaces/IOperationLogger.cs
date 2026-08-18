namespace WindowsOptimizer.Core.Interfaces;

public interface IOperationLogger
{
    void Info(string message);

    void Warning(string message);

    void Error(string message, Exception? exception = null);

    string LogDirectory { get; }
}
