using WindowsOptimizer.Core.Enums;
using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;
using WindowsOptimizer.Infrastructure.IO;

namespace WindowsOptimizer.Infrastructure.Cleaning;

public sealed class BrokenShortcutsCleaner : ISystemCleaner
{
    public string Id => "broken-shortcuts";
    public string DisplayName => "Битые ярлыки";
    public string Description => "Поиск ярлыков (.lnk) с несуществующей целью на рабочем столе, в меню Пуск и на панели задач";
    public bool RequiresAdministrator => false;

    public Task<IReadOnlyList<IssueItem>> ScanAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var issues = ShortcutScanner.FindBroken(cancellationToken)
                .Select(item => new IssueItem
                {
                    Id = Guid.NewGuid(),
                    CheckId = Id,
                    Title = Path.GetFileNameWithoutExtension(item.LinkPath),
                    Description = $"Цель не найдена: {item.Target}",
                    Category = OperationCategory.Cleaning,
                    Severity = IssueSeverity.Medium,
                    Action = RepairAction.DeleteFile,
                    SizeBytes = item.Size,
                    FilePath = item.LinkPath
                })
                .ToList();

            return (IReadOnlyList<IssueItem>)issues;
        }, cancellationToken);
    }

    public Task<OperationResult> CleanAsync(
        IReadOnlyList<IssueItem> items,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() => CleanerShared.DeleteExactFiles(items, progress, cancellationToken), cancellationToken);
    }
}
