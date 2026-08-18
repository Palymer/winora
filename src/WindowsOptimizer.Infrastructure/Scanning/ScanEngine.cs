using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.Infrastructure.Scanning;

public sealed class ScanEngine : IScanEngine
{
    private readonly ICleanerOrchestrator _cleaner;
    private readonly IRegistryRepairService _registry;

    public ScanEngine(ICleanerOrchestrator cleaner, IRegistryRepairService registry)
    {
        _cleaner = cleaner;
        _registry = registry;
    }

    public async Task<ScanResult> ScanAllAsync(
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.Now;
        var issues = new List<IssueItem>();

        var cleaning = await _cleaner.ScanAsync(progress: new Progress<OperationProgress>(p =>
        {
            progress?.Report(new OperationProgress
            {
                CurrentStep = p.CurrentStep,
                Percent = p.Percent / 2,
                ProcessedItems = p.ProcessedItems,
                TotalItems = p.TotalItems * 2
            });
        }), cancellationToken: cancellationToken).ConfigureAwait(false);
        issues.AddRange(cleaning.Issues);

        var registry = await _registry.ScanAsync(new Progress<OperationProgress>(p =>
        {
            progress?.Report(new OperationProgress
            {
                CurrentStep = p.CurrentStep,
                Percent = 50 + p.Percent / 2,
                ProcessedItems = p.ProcessedItems,
                TotalItems = p.TotalItems * 2
            });
        }), cancellationToken).ConfigureAwait(false);
        issues.AddRange(registry.Issues);

        return new ScanResult
        {
            StartedAt = started,
            Duration = DateTimeOffset.Now - started,
            Issues = issues
        };
    }
}
