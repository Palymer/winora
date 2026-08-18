using WindowsOptimizer.Core.Enums;
using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;
using WindowsOptimizer.Infrastructure.IO;

namespace WindowsOptimizer.Infrastructure.Cleaning;

public sealed class TempFilesCleaner : ISystemCleaner
{
    public string Id => "temp-files";
    public string DisplayName => "Временные файлы";
    public string Description => "Очистка %TEMP% пользователя и C:\\Windows\\Temp";
    public bool RequiresAdministrator => true;

    public Task<IReadOnlyList<IssueItem>> ScanAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var issues = new List<IssueItem>();
            foreach (var folder in GetFolders())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!Directory.Exists(folder))
                {
                    continue;
                }

                var files = FileSystemScanHelper.EnumerateFilesSafe(folder).ToList();
                var size = FileSystemScanHelper.SumSizes(files);
                if (size <= 0 && files.Count == 0)
                {
                    continue;
                }

                issues.Add(new IssueItem
                {
                    Id = Guid.NewGuid(),
                    CheckId = Id,
                    Title = "Временные файлы",
                    Description = $"{files.Count} файлов в {folder}",
                    Category = OperationCategory.Cleaning,
                    Severity = IssueSeverity.Low,
                    Action = RepairAction.DeleteDirectory,
                    SizeBytes = size,
                    FilePath = folder
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
        return Task.Run(() => Clean(items, progress, cancellationToken), cancellationToken);
    }

    private static OperationResult Clean(
        IReadOnlyList<IssueItem> items,
        IProgress<OperationProgress>? progress,
        CancellationToken cancellationToken)
    {
        var fixedCount = 0;
        var failed = 0;
        long freed = 0;
        var messages = new List<string>();

        for (var i = 0; i < items.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var item = items[i];
            progress?.Report(new OperationProgress
            {
                CurrentStep = $"Очистка {item.FilePath}",
                Percent = items.Count == 0 ? 100 : (i + 1) * 100 / items.Count,
                ProcessedItems = i + 1,
                TotalItems = items.Count
            });

            if (string.IsNullOrWhiteSpace(item.FilePath) || !Directory.Exists(item.FilePath))
            {
                continue;
            }

            var files = FileSystemScanHelper.EnumerateFilesSafe(item.FilePath).ToList();
            foreach (var file in files)
            {
                if (FileSystemScanHelper.TryDeleteFile(file.FullName, out var error))
                {
                    freed += file.Length;
                    fixedCount++;
                }
                else
                {
                    failed++;
                    if (messages.Count < 20 && error is not null)
                    {
                        messages.Add(error);
                    }
                }
            }
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

    private static IEnumerable<string> GetFolders()
    {
        yield return Path.GetTempPath();
        yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");
    }
}
