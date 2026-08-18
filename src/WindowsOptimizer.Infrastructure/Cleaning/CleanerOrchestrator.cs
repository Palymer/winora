using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.Infrastructure.Cleaning;

public sealed class CleanerOrchestrator : ICleanerOrchestrator
{
    private readonly IReadOnlyList<ISystemCleaner> _cleaners;
    private readonly IOperationLogger _logger;

    public CleanerOrchestrator(IEnumerable<ISystemCleaner> cleaners, IOperationLogger logger)
    {
        _cleaners = cleaners.ToList();
        _logger = logger;
    }

    public IReadOnlyList<ISystemCleaner> Cleaners => _cleaners;

    public async Task<ScanResult> ScanAsync(
        IEnumerable<string>? cleanerIds = null,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.Now;
        var selected = cleanerIds is null
            ? _cleaners
            : _cleaners.Where(c => cleanerIds.Contains(c.Id, StringComparer.OrdinalIgnoreCase)).ToList();

        var issues = new List<IssueItem>();
        for (var i = 0; i < selected.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var cleaner = selected[i];
            progress?.Report(new OperationProgress
            {
                CurrentStep = $"Сканирование: {cleaner.DisplayName}",
                Percent = selected.Count == 0 ? 100 : i * 100 / selected.Count,
                ProcessedItems = i,
                TotalItems = selected.Count
            });

            try
            {
                var found = await cleaner.ScanAsync(cancellationToken).ConfigureAwait(false);
                issues.AddRange(found);
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка сканирования {cleaner.Id}", ex);
            }
        }

        progress?.Report(new OperationProgress
        {
            CurrentStep = "Сканирование завершено",
            Percent = 100,
            ProcessedItems = selected.Count,
            TotalItems = selected.Count
        });

        return new ScanResult
        {
            StartedAt = started,
            Duration = DateTimeOffset.Now - started,
            Issues = issues
        };
    }

    public async Task<OperationResult> CleanAsync(
        IReadOnlyList<IssueItem> items,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var groups = items.GroupBy(i => i.CheckId);
        var fixedCount = 0;
        var failed = 0;
        long freed = 0;
        var messages = new List<string>();

        foreach (var group in groups)
        {
            var cleaner = _cleaners.FirstOrDefault(c => c.Id.Equals(group.Key, StringComparison.OrdinalIgnoreCase));
            if (cleaner is null)
            {
                failed += group.Count();
                continue;
            }

            _logger.Info($"Очистка модуля {cleaner.DisplayName}");
            var result = await cleaner.CleanAsync(group.ToList(), progress, cancellationToken).ConfigureAwait(false);
            fixedCount += result.FixedCount;
            failed += result.FailedCount;
            freed += result.FreedBytes;
            messages.AddRange(result.Messages);
        }

        return new OperationResult
        {
            Success = failed == 0,
            FixedCount = fixedCount,
            FailedCount = failed,
            FreedBytes = freed,
            Messages = messages
        };
    }
}
