using Microsoft.Win32;
using WindowsOptimizer.Core.Enums;
using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.Infrastructure.RegistryRepair.Checks;

public sealed class SharedDllsCheck : IRegistryCheck
{
    public string Id => "shared-dlls";
    public string DisplayName => "SharedDLLs";

    public IReadOnlyList<IssueItem> Scan()
    {
        const string path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\SharedDLLs";
        var issues = new List<IssueItem>();
        using var key = Registry.LocalMachine.OpenSubKey(path);
        if (key is null)
        {
            return issues;
        }

        foreach (var name in key.GetValueNames())
        {
            var file = RegistryPathHelper.ExtractExistingPath(name);
            if (!RegistryPathHelper.PathLooksValidButMissing(file))
            {
                continue;
            }

            issues.Add(new IssueItem
            {
                Id = Guid.NewGuid(),
                CheckId = Id,
                Title = Path.GetFileName(file) ?? name,
                Description = $"SharedDLLs ссылается на отсутствующий файл: {file}",
                Category = OperationCategory.Registry,
                Severity = IssueSeverity.Low,
                Action = RepairAction.DeleteRegistryValue,
                RegistryHive = "HKLM",
                RegistryPath = path,
                RegistryValueName = name
            });
        }

        return issues;
    }
}
