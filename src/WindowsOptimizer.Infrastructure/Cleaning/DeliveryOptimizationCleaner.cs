using WindowsOptimizer.Core.Enums;
using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;
using WindowsOptimizer.Infrastructure.IO;

namespace WindowsOptimizer.Infrastructure.Cleaning;

public sealed class DeliveryOptimizationCleaner : ISystemCleaner
{
    public string Id => "delivery-optimization";
    public string DisplayName => "Кэш обновлений";
    public string Description => "Delivery Optimization и загруженные пакеты Windows Update";
    public bool RequiresAdministrator => true;

    public Task<IReadOnlyList<IssueItem>> ScanAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            var folders = new (string Path, string Title)[]
            {
                (Path.Combine(windows, @"SoftwareDistribution\Download"), "Загрузки Windows Update"),
                (Path.Combine(windows, @"ServiceProfiles\NetworkService\AppData\Local\Microsoft\Windows\DeliveryOptimization\Cache"), "Delivery Optimization")
            };

            var issues = new List<IssueItem>();
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
                    CheckId = Id,
                    Title = title,
                    Description = $"{files.Count} файлов в {path}",
                    Category = OperationCategory.Cleaning,
                    Severity = IssueSeverity.Low,
                    Action = RepairAction.DeleteFile,
                    SizeBytes = size,
                    FilePath = path
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
