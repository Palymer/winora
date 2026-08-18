using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.Core.Interfaces;

public interface IOptimizer
{
    string Id { get; }

    string DisplayName { get; }

    IReadOnlyList<OptimizationToggle> GetToggles();

    Task<OperationResult> ApplyAsync(
        IReadOnlyList<OptimizationToggle> toggles,
        CancellationToken cancellationToken = default);
}
