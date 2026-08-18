using WindowsOptimizer.Core.Enums;
using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;
using WindowsOptimizer.Infrastructure.IO;

namespace WindowsOptimizer.Infrastructure.Cleaning;

public sealed class PrefetchCleaner : ISystemCleaner
{
    public string Id => "prefetch";
    public string DisplayName => "Prefetch";
    public string Description => "Очистка кэша предзагрузки приложений (C:\\Windows\\Prefetch)";
    public bool RequiresAdministrator => true;

    public Task<IReadOnlyList<IssueItem>> ScanAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch");
            if (!Directory.Exists(path))
            {
                return (IReadOnlyList<IssueItem>)Array.Empty<IssueItem>();
            }

            var files = FileSystemScanHelper.EnumerateFilesSafe(path, "*.pf", SearchOption.TopDirectoryOnly).ToList();
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
                    Title = "Файлы Prefetch",
                    Description = $"{files.Count} файлов .pf. Очистка может временно замедлить первый запуск программ.",
                    Category = OperationCategory.Cleaning,
                    Severity = IssueSeverity.Medium,
                    Action = RepairAction.DeleteFile,
                    SizeBytes = size,
                    FilePath = path,
                    Metadata = new Dictionary<string, string> { ["pattern"] = "*.pf" }
                }
            };
        }, cancellationToken);
    }

    public Task<OperationResult> CleanAsync(
        IReadOnlyList<IssueItem> items,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() => CleanerShared.DeleteListedFiles(items, progress, cancellationToken, "*.pf"), cancellationToken);
    }
}
