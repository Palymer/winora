namespace WindowsOptimizer.Core.Models;

public sealed class ScanResult
{
    public required DateTimeOffset StartedAt { get; init; }

    public required TimeSpan Duration { get; init; }

    public required IReadOnlyList<IssueItem> Issues { get; init; }

    public int IssueCount => Issues.Count;

    public long TotalSizeBytes => Issues.Sum(i => i.SizeBytes);
}
