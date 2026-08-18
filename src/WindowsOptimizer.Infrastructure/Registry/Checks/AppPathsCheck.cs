using Microsoft.Win32;
using WindowsOptimizer.Core.Enums;
using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.Infrastructure.RegistryRepair.Checks;

public sealed class AppPathsCheck : IRegistryCheck
{
    public string Id => "app-paths";
    public string DisplayName => "App Paths";

    private const string Root = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths";

    public IReadOnlyList<IssueItem> Scan()
    {
        var issues = new List<IssueItem>();
        ScanHive(Registry.LocalMachine, "HKLM", issues);
        ScanHive(Registry.CurrentUser, "HKCU", issues);
        return issues;
    }

    private void ScanHive(RegistryKey hive, string hiveName, List<IssueItem> issues)
    {
        using var key = hive.OpenSubKey(Root);
        if (key is null)
        {
            return;
        }

        foreach (var sub in RegistryPathHelper.OpenSubKeysSafe(key))
        {
            using (sub)
            {
                var defaultPath = RegistryPathHelper.ExtractExistingPath(sub.GetValue(null)?.ToString());
                if (!RegistryPathHelper.PathLooksValidButMissing(defaultPath))
                {
                    continue;
                }

                issues.Add(new IssueItem
                {
                    Id = Guid.NewGuid(),
                    CheckId = Id,
                    Title = Path.GetFileName(sub.Name),
                    Description = $"App Paths ссылается на отсутствующий файл: {defaultPath}",
                    Category = OperationCategory.Registry,
                    Severity = IssueSeverity.Low,
                    Action = RepairAction.DeleteRegistryKey,
                    RegistryHive = hiveName,
                    RegistryPath = $"{Root}\\{Path.GetFileName(sub.Name)}"
                });
            }
        }
    }
}
