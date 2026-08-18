using WindowsOptimizer.Core.Enums;
using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;
using WindowsOptimizer.Infrastructure.IO;

namespace WindowsOptimizer.Infrastructure.Cleaning;

public sealed class ShaderCacheCleaner : ISystemCleaner
{
    public string Id => "shader-cache";
    public string DisplayName => "Кэш шейдеров GPU";
    public string Description => "DirectX D3DSCache, кэш NVIDIA/AMD. После очистки первый запуск игр может быть дольше.";
    public bool RequiresAdministrator => false;

    public Task<IReadOnlyList<IssueItem>> ScanAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var folders = new (string Path, string Title)[]
            {
                (Path.Combine(local, "D3DSCache"), "DirectX Shader Cache"),
                (Path.Combine(local, @"NVIDIA\DXCache"), "NVIDIA DXCache"),
                (Path.Combine(local, @"NVIDIA\GLCache"), "NVIDIA GLCache"),
                (Path.Combine(local, @"AMD\DxCache"), "AMD DxCache"),
                (Path.Combine(local, @"AMD\GLCache"), "AMD GLCache")
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

public sealed class IconCacheCleaner : ISystemCleaner
{
    public string Id => "icon-cache";
    public string DisplayName => "Кэш значков";
    public string Description => "IconCache.db и iconcache_*.db проводника";
    public bool RequiresAdministrator => false;

    public Task<IReadOnlyList<IssueItem>> ScanAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var issues = new List<IssueItem>();
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var iconDb = Path.Combine(local, "IconCache.db");
            if (File.Exists(iconDb))
            {
                issues.Add(new IssueItem
                {
                    Id = Guid.NewGuid(),
                    CheckId = Id,
                    Title = "IconCache.db",
                    Description = iconDb,
                    Category = OperationCategory.Cleaning,
                    Severity = IssueSeverity.Info,
                    Action = RepairAction.DeleteFile,
                    SizeBytes = new FileInfo(iconDb).Length,
                    FilePath = iconDb
                });
            }

            var explorer = Path.Combine(local, @"Microsoft\Windows\Explorer");
            if (Directory.Exists(explorer))
            {
                var files = FileSystemScanHelper.EnumerateFilesSafe(explorer, "iconcache_*.db", SearchOption.TopDirectoryOnly).ToList();
                var size = FileSystemScanHelper.SumSizes(files);
                if (files.Count > 0)
                {
                    issues.Add(new IssueItem
                    {
                        Id = Guid.NewGuid(),
                        CheckId = Id,
                        Title = "iconcache_*.db",
                        Description = $"{files.Count} файлов в {explorer}",
                        Category = OperationCategory.Cleaning,
                        Severity = IssueSeverity.Info,
                        Action = RepairAction.DeleteFile,
                        SizeBytes = size,
                        FilePath = explorer,
                        Metadata = new Dictionary<string, string> { ["pattern"] = "iconcache_*.db" }
                    });
                }
            }

            return (IReadOnlyList<IssueItem>)issues;
        }, cancellationToken);
    }

    public Task<OperationResult> CleanAsync(
        IReadOnlyList<IssueItem> items,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var files = items.Where(i => i.FilePath is not null && File.Exists(i.FilePath)).ToList();
            var dirs = items.Where(i => i.FilePath is not null && Directory.Exists(i.FilePath)).ToList();
            var a = files.Count == 0
                ? new OperationResult { Success = true }
                : CleanerShared.DeleteExactFiles(files, progress, cancellationToken);
            var b = dirs.Count == 0
                ? new OperationResult { Success = true }
                : CleanerShared.DeleteListedFiles(dirs, progress, cancellationToken, "iconcache_*.db");
            return new OperationResult
            {
                Success = a.Success && b.Success,
                FixedCount = a.FixedCount + b.FixedCount,
                FailedCount = a.FailedCount + b.FailedCount,
                FreedBytes = a.FreedBytes + b.FreedBytes,
                Messages = a.Messages.Concat(b.Messages).ToList()
            };
        }, cancellationToken);
    }
}
