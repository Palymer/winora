using Microsoft.Win32;
using WindowsOptimizer.Core.Enums;
using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.Infrastructure.RegistryRepair.Checks;

public sealed class UninstallEntriesCheck : IRegistryCheck
{
    public string Id => "uninstall-entries";
    public string DisplayName => "Записи удаления программ";

    private static readonly string[] Roots =
    [
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
    ];

    public IReadOnlyList<IssueItem> Scan()
    {
        var issues = new List<IssueItem>();
        ScanHive(Registry.LocalMachine, "HKLM", issues);
        ScanHive(Registry.CurrentUser, "HKCU", issues);
        return issues;
    }

    private void ScanHive(RegistryKey hive, string hiveName, List<IssueItem> issues)
    {
        foreach (var root in Roots)
        {
            using var key = hive.OpenSubKey(root);
            if (key is null)
            {
                continue;
            }

            foreach (var sub in RegistryPathHelper.OpenSubKeysSafe(key))
            {
                using (sub)
                {
                    var displayName = sub.GetValue("DisplayName") as string;
                    var uninstall = RegistryPathHelper.ExtractExistingPath(sub.GetValue("UninstallString") as string);
                    var installLocation = sub.GetValue("InstallLocation") as string;
                    installLocation = string.IsNullOrWhiteSpace(installLocation)
                        ? null
                        : Environment.ExpandEnvironmentVariables(installLocation.Trim().Trim('"'));

                    var missingUninstall = RegistryPathHelper.PathLooksValidButMissing(uninstall);
                    var missingInstall = !string.IsNullOrWhiteSpace(installLocation) &&
                                         Path.IsPathRooted(installLocation) &&
                                         !Directory.Exists(installLocation) &&
                                         !File.Exists(installLocation);

                    if (!missingUninstall && !missingInstall)
                    {
                        continue;
                    }

                    issues.Add(new IssueItem
                    {
                        Id = Guid.NewGuid(),
                        CheckId = Id,
                        Title = displayName ?? sub.Name,
                        Description = missingUninstall
                            ? $"UninstallString указывает на отсутствующий файл: {uninstall}"
                            : $"InstallLocation не существует: {installLocation}",
                        Category = OperationCategory.Registry,
                        Severity = IssueSeverity.Medium,
                        Action = RepairAction.DeleteRegistryKey,
                        RegistryHive = hiveName,
                        RegistryPath = $"{root}\\{Path.GetFileName(sub.Name)}"
                    });
                }
            }
        }
    }
}
