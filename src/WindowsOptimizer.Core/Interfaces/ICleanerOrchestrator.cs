using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.Core.Interfaces;

public interface ICleanerOrchestrator
{
    IReadOnlyList<ISystemCleaner> Cleaners { get; }

    Task<ScanResult> ScanAsync(
        IEnumerable<string>? cleanerIds = null,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default);

    Task<OperationResult> CleanAsync(
        IReadOnlyList<IssueItem> items,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
