using WindowsOptimizer.Core.Enums;
using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;
using WindowsOptimizer.Infrastructure.IO;

namespace WindowsOptimizer.Infrastructure.Cleaning;

public sealed class WindowsLogsCleaner : ISystemCleaner
{
    public string Id => "windows-logs";
    public string DisplayName => "Журналы и дампы";
    public string Description => "Отчёты об ошибках Windows, дампы памяти и устаревшие логи";
    public bool RequiresAdministrator => true;

    public Task<IReadOnlyList<IssueItem>> ScanAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Scan(cancellationToken), cancellationToken);
    }

    public Task<OperationResult> CleanAsync(
        IReadOnlyList<IssueItem> items,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() => CleanerShared.DeleteListedFiles(items, progress, cancellationToken), cancellationToken);
    }

    private static IReadOnlyList<IssueItem> Scan(CancellationToken cancellationToken)
    {
        var issues = new List<IssueItem>();
        var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        var folders = new (string Path, string Title)[]
        {
            (Path.Combine(windows, "Minidump"), "Дампы памяти"),
            (Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Microsoft\Windows\WER\ReportQueue"), "Очередь отчётов об ошибках"),
            (Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\WER"), "Локальные отчёты об ошибках"),
            (Path.Combine(windows, @"Logs\CBS"), "Логи CBS"),
            (Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"CrashDumps"), "Дампы приложений")
        };

        foreach (var (path, title) in folders)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!Directory.Exists(path))
            {
                continue;
            }

            var files = FileSystemScanHelper.EnumerateFilesSafe(path).ToList();
            var size = FileSystemScanHelper.SumSizes(files);
            if (files.Count == 0)
            {
                continue;
            }

            issues.Add(new IssueItem
            {
                Id = Guid.NewGuid(),
                CheckId = "windows-logs",
                Title = title,
                Description = $"{files.Count} файлов в {path}",
                Category = OperationCategory.Cleaning,
                Severity = IssueSeverity.Low,
                Action = RepairAction.DeleteFile,
                SizeBytes = size,
                FilePath = path
            });
        }

        return issues;
    }
}
