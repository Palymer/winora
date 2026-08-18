using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.Infrastructure.Optimization;

public sealed class OptimizerOrchestrator : IOptimizerOrchestrator
{
    private readonly IReadOnlyList<IOptimizer> _optimizers;
    private readonly IOperationLogger _logger;

    public OptimizerOrchestrator(IEnumerable<IOptimizer> optimizers, IOperationLogger logger)
    {
        _optimizers = optimizers.ToList();
        _logger = logger;
    }

    public IReadOnlyList<IOptimizer> Optimizers => _optimizers;

    public IReadOnlyList<OptimizationToggle> GetAllToggles() =>
        _optimizers.SelectMany(o => o.GetToggles()).ToList();

    public async Task<OperationResult> ApplyAsync(
        IReadOnlyList<OptimizationToggle> toggles,
        CancellationToken cancellationToken = default)
    {
        var fixedCount = 0;
        var failed = 0;
        var messages = new List<string>();

        foreach (var optimizer in _optimizers)
        {
            var ownedIds = optimizer.GetToggles().Select(t => t.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var subset = toggles.Where(t => ownedIds.Contains(t.Id)).ToList();
            if (subset.Count == 0)
            {
                continue;
            }

            _logger.Info($"Применение оптимизаций: {optimizer.DisplayName}");
            var result = await optimizer.ApplyAsync(subset, cancellationToken).ConfigureAwait(false);
            fixedCount += result.FixedCount;
            failed += result.FailedCount;
            messages.AddRange(result.Messages);
        }

        return new OperationResult
        {
            Success = failed == 0,
            FixedCount = fixedCount,
            FailedCount = failed,
            Messages = messages
        };
    }
}
