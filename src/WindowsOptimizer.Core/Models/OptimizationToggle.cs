namespace WindowsOptimizer.Core.Models;

public sealed class OptimizationToggle
{
    public required string Id { get; init; }

    public required string Group { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required bool IsRecommended { get; init; }

    public required bool RequiresRestart { get; init; }

    public bool IsEnabled { get; set; }
}
