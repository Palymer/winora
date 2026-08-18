using Microsoft.Win32;
using WindowsOptimizer.Core.Enums;
using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.Infrastructure.RegistryRepair.Checks;

public sealed class StartupEntriesCheck : IRegistryCheck
{
    public string Id => "startup-entries";
    public string DisplayName => "Автозагрузка";

    private static readonly (string HiveName, RegistryHive Hive, string Path)[] Locations =
    [
        ("HKCU", RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run"),
        ("HKLM", RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"),
        ("HKLM", RegistryHive.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run")
    ];

    public IReadOnlyList<IssueItem> Scan()
    {
        var issues = new List<IssueItem>();
        foreach (var (hiveName, hive, path) in Locations)
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var key = baseKey.OpenSubKey(path);
            if (key is null)
            {
                continue;
            }

            foreach (var name in key.GetValueNames())
            {
                var raw = key.GetValue(name)?.ToString();
                var filePath = RegistryPathHelper.ExtractExistingPath(raw);
                if (!RegistryPathHelper.PathLooksValidButMissing(filePath))
                {
                    continue;
                }

                issues.Add(new IssueItem
                {
                    Id = Guid.NewGuid(),
                    CheckId = Id,
                    Title = $"Автозагрузка: {name}",
                    Description = $"Файл не найден: {filePath}",
                    Category = OperationCategory.Registry,
                    Severity = IssueSeverity.High,
                    Action = RepairAction.DeleteRegistryValue,
                    RegistryHive = hiveName,
                    RegistryPath = path,
                    RegistryValueName = name
                });
            }
        }

        return issues;
    }
}
