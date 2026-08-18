using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.Core.Interfaces;

public interface ISystemCleaner
{
    string Id { get; }

    string DisplayName { get; }

    string Description { get; }

    bool RequiresAdministrator { get; }

    Task<IReadOnlyList<IssueItem>> ScanAsync(CancellationToken cancellationToken = default);

    Task<OperationResult> CleanAsync(
        IReadOnlyList<IssueItem> items,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
