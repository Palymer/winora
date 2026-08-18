using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.Core.Interfaces;

public interface IScanEngine
{
    Task<ScanResult> ScanAllAsync(
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
