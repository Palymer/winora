using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.Core.Interfaces;

public interface IOptimizerOrchestrator
{
    IReadOnlyList<IOptimizer> Optimizers { get; }

    IReadOnlyList<OptimizationToggle> GetAllToggles();

    Task<OperationResult> ApplyAsync(
        IReadOnlyList<OptimizationToggle> toggles,
        CancellationToken cancellationToken = default);
}
