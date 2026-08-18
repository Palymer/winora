namespace WindowsOptimizer.Core.Models;

public sealed class IssueItem
{
    public required Guid Id { get; init; }

    public required string CheckId { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public required Enums.OperationCategory Category { get; init; }

    public required Enums.IssueSeverity Severity { get; init; }

    public required Enums.RepairAction Action { get; init; }

    public long SizeBytes { get; init; }

    public string? FilePath { get; init; }

    public string? RegistryHive { get; init; }

    public string? RegistryPath { get; init; }

    public string? RegistryValueName { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>();
}
