using WindowsOptimizer.Core.Enums;
using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;
using WindowsOptimizer.Infrastructure.IO;

namespace WindowsOptimizer.Infrastructure.Cleaning;

public sealed class RecentItemsCleaner : ISystemCleaner
{
    public string Id => "recent-items";
    public string DisplayName => "Недавние файлы";
    public string Description => "Ярлыки недавних документов и Jump Lists проводника";
    public bool RequiresAdministrator => false;

    public Task<IReadOnlyList<IssueItem>> ScanAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var recent = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
            var automatic = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"Microsoft\Windows\Recent\AutomaticDestinations");
            var custom = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"Microsoft\Windows\Recent\CustomDestinations");

            var issues = new List<IssueItem>();
            foreach (var (path, title, pattern) in new[]
                     {
                         (recent, "Недавние документы", "*.lnk"),
                         (automatic, "Jump Lists", "*"),
                         (custom, "Закреплённые Jump Lists", "*")
                     })
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!Directory.Exists(path))
                {
                    continue;
                }

                var files = FileSystemScanHelper.EnumerateFilesSafe(path, pattern, SearchOption.TopDirectoryOnly).ToList();
                var size = FileSystemScanHelper.SumSizes(files);
                if (files.Count == 0)
                {
                    continue;
                }

                issues.Add(new IssueItem
                {
                    Id = Guid.NewGuid(),
                    CheckId = Id,
                    Title = title,
                    Description = $"{files.Count} файлов в {path}",
                    Category = OperationCategory.Cleaning,
                    Severity = IssueSeverity.Info,
                    Action = RepairAction.DeleteFile,
                    SizeBytes = size,
                    FilePath = path,
                    Metadata = new Dictionary<string, string> { ["pattern"] = pattern }
                });
            }

            return (IReadOnlyList<IssueItem>)issues;
        }, cancellationToken);
    }

    public Task<OperationResult> CleanAsync(
        IReadOnlyList<IssueItem> items,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() => CleanerShared.DeleteListedFiles(items, progress, cancellationToken), cancellationToken);
    }
}
