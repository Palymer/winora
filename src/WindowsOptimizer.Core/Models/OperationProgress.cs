namespace WindowsOptimizer.Core.Models;

public sealed class OperationProgress
{
    public required string CurrentStep { get; init; }

    public int Percent { get; init; }

    public int ProcessedItems { get; init; }

    public int TotalItems { get; init; }
}
