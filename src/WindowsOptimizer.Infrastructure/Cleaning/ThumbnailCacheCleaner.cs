using WindowsOptimizer.Core.Enums;
using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;
using WindowsOptimizer.Infrastructure.IO;

namespace WindowsOptimizer.Infrastructure.Cleaning;

public sealed class ThumbnailCacheCleaner : ISystemCleaner
{
    public string Id => "thumbnails";
    public string DisplayName => "Кэш эскизов";
    public string Description => "Удаление кэша миниатюр проводника (thumbcache_*.db)";
    public bool RequiresAdministrator => false;

    public Task<IReadOnlyList<IssueItem>> ScanAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var explorer = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Microsoft\Windows\Explorer");

            if (!Directory.Exists(explorer))
            {
                return (IReadOnlyList<IssueItem>)Array.Empty<IssueItem>();
            }

            var files = FileSystemScanHelper.EnumerateFilesSafe(explorer, "thumbcache_*.db", SearchOption.TopDirectoryOnly).ToList();
            var size = FileSystemScanHelper.SumSizes(files);
            if (files.Count == 0)
            {
                return Array.Empty<IssueItem>();
            }

            return new[]
            {
                new IssueItem
                {
                    Id = Guid.NewGuid(),
                    CheckId = Id,
                    Title = "Кэш эскизов проводника",
                    Description = $"{files.Count} файлов thumbcache в {explorer}",
                    Category = OperationCategory.Cleaning,
                    Severity = IssueSeverity.Info,
                    Action = RepairAction.DeleteFile,
                    SizeBytes = size,
                    FilePath = explorer,
                    Metadata = new Dictionary<string, string> { ["pattern"] = "thumbcache_*.db" }
                }
            };
        }, cancellationToken);
    }

    public Task<OperationResult> CleanAsync(
        IReadOnlyList<IssueItem> items,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() => CleanerShared.DeleteListedFiles(items, progress, cancellationToken, "thumbcache_*.db"), cancellationToken);
    }
}
