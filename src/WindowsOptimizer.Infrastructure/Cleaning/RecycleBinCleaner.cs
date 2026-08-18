using WindowsOptimizer.Core.Enums;
using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.Infrastructure.Cleaning;

public sealed class RecycleBinCleaner : ISystemCleaner
{
    public string Id => "recycle-bin";
    public string DisplayName => "Корзина";
    public string Description => "Очистка корзины на всех дисках";
    public bool RequiresAdministrator => true;

    public Task<IReadOnlyList<IssueItem>> ScanAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var snapshot = RecycleBinInspector.Query();
            if (snapshot.ItemCount <= 0 && snapshot.SizeBytes <= 0)
            {
                return (IReadOnlyList<IssueItem>)Array.Empty<IssueItem>();
            }

            return new[]
            {
                new IssueItem
                {
                    Id = Guid.NewGuid(),
                    CheckId = Id,
                    Title = "Корзина Windows",
                    Description = $"{snapshot.ItemCount} элементов в корзине",
                    Category = OperationCategory.Cleaning,
                    Severity = IssueSeverity.Info,
                    Action = RepairAction.EmptyRecycleBin,
                    SizeBytes = snapshot.SizeBytes
                }
            };
        }, cancellationToken);
    }

    public Task<OperationResult> CleanAsync(
        IReadOnlyList<IssueItem> items,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new OperationProgress
            {
                CurrentStep = "Очистка корзины",
                Percent = 40,
                ProcessedItems = 0,
                TotalItems = 1
            });

            var hr = RecycleBinInspector.Empty();
            var leftover = RecycleBinInspector.Query();
            var success = leftover.ItemCount == 0 && leftover.SizeBytes == 0;
            if (!success && (hr == 0 || (uint)hr == 0x8000FFFF))
            {
                success = leftover.ItemCount == 0;
            }

            progress?.Report(new OperationProgress
            {
                CurrentStep = success ? "Корзина очищена" : "Корзина очищена частично",
                Percent = 100,
                ProcessedItems = 1,
                TotalItems = 1
            });

            var freed = items.Sum(i => i.SizeBytes) - leftover.SizeBytes;
            return new OperationResult
            {
                Success = success,
                FixedCount = success ? Math.Max(1, items.Count) : 0,
                FailedCount = success ? 0 : 1,
                FreedBytes = Math.Max(0, freed),
                Messages = success
                    ? Array.Empty<string>()
                    : new[] { $"Не удалось удалить все файлы корзины (HRESULT 0x{hr:X8}, осталось {leftover.ItemCount})." }
            };
        }, cancellationToken);
    }
}
