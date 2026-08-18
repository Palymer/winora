using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.Core.Interfaces;

public interface IRegistryRepairService
{
    Task<ScanResult> ScanAsync(
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default);

    Task<OperationResult> RepairAsync(
        IReadOnlyList<IssueItem> items,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
