using WindowsOptimizer.Core.Enums;
using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;
using WindowsOptimizer.Infrastructure.IO;

namespace WindowsOptimizer.Infrastructure.Cleaning;

public sealed class BrowserCacheCleaner : ISystemCleaner
{
    public string Id => "browser-cache";
    public string DisplayName => "Кэш браузеров";
    public string Description => "Кэш Chrome, Edge и Firefox. Куки и пароли не трогаются.";
    public bool RequiresAdministrator => false;

    public Task<IReadOnlyList<IssueItem>> ScanAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var issues = new List<IssueItem>();
            foreach (var (path, title) in GetFolders())
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

    private static IEnumerable<(string Path, string Title)> GetFolders()
    {
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        yield return (Path.Combine(local, @"Google\Chrome\User Data\Default\Cache"), "Кэш Google Chrome");
        yield return (Path.Combine(local, @"Google\Chrome\User Data\Default\Code Cache"), "Code Cache Chrome");
        yield return (Path.Combine(local, @"Microsoft\Edge\User Data\Default\Cache"), "Кэш Microsoft Edge");
        yield return (Path.Combine(local, @"Microsoft\Edge\User Data\Default\Code Cache"), "Code Cache Edge");
        yield return (Path.Combine(local, @"Microsoft\Windows\INetCache"), "Кэш Internet Explorer / WebView");

        var firefox = Path.Combine(local, @"Mozilla\Firefox\Profiles");
        if (!Directory.Exists(firefox))
        {
            yield break;
        }

        foreach (var profile in Directory.GetDirectories(firefox))
        {
            yield return (Path.Combine(profile, "cache2"), $"Кэш Firefox ({Path.GetFileName(profile)})");
        }
    }
}
