namespace WindowsOptimizer.Core.Models;

public sealed class OperationResult
{
    public required bool Success { get; init; }

    public int FixedCount { get; init; }

    public int FailedCount { get; init; }

    public int SkippedCount { get; init; }

    public long FreedBytes { get; init; }

    public string? BackupPath { get; init; }

    public IReadOnlyList<string> Messages { get; init; } = Array.Empty<string>();

    public static OperationResult Empty(string message) => new()
    {
        Success = true,
        Messages = new[] { message }
    };

    public static OperationResult Fail(string message) => new()
    {
        Success = false,
        Messages = new[] { message }
    };
}
