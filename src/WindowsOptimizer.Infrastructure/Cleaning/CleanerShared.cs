using WindowsOptimizer.Core.Enums;
using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;
using WindowsOptimizer.Infrastructure.IO;

namespace WindowsOptimizer.Infrastructure.Cleaning;

internal static class CleanerShared
{
    public static OperationResult DeleteListedFiles(
        IReadOnlyList<IssueItem> items,
        IProgress<OperationProgress>? progress,
        CancellationToken cancellationToken,
        string pattern = "*")
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
                CurrentStep = item.Title,
                Percent = items.Count == 0 ? 100 : (i + 1) * 100 / items.Count,
                ProcessedItems = i + 1,
                TotalItems = items.Count
            });

            if (string.IsNullOrWhiteSpace(item.FilePath) || !Directory.Exists(item.FilePath))
            {
                continue;
            }

            var usedPattern = item.Metadata.TryGetValue("pattern", out var metaPattern) ? metaPattern : pattern;
            foreach (var file in FileSystemScanHelper.EnumerateFilesSafe(item.FilePath, usedPattern))
            {
                var length = 0L;
                try
                {
                    length = file.Length;
                }
                catch
                {
                    // ignore
                }

                if (FileSystemScanHelper.TryDeleteFile(file.FullName, out var error))
                {
                    freed += length;
                    fixedCount++;
                }
                else
                {
                    failed++;
                    if (messages.Count < 20 && error is not null)
                    {
                        messages.Add($"{file.Name}: {error}");
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

    public static OperationResult DeleteExactFiles(
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
                CurrentStep = item.Title,
                Percent = items.Count == 0 ? 100 : (i + 1) * 100 / items.Count,
                ProcessedItems = i + 1,
                TotalItems = items.Count
            });

            if (string.IsNullOrWhiteSpace(item.FilePath) || !File.Exists(item.FilePath))
            {
                continue;
            }

            var length = item.SizeBytes;
            if (FileSystemScanHelper.TryDeleteFile(item.FilePath, out var error))
            {
                freed += length;
                fixedCount++;
            }
            else
            {
                failed++;
                if (error is not null)
                {
                    messages.Add($"{item.Title}: {error}");
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
}
