using Microsoft.Win32;
using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.Infrastructure.RegistryRepair.Checks;

public interface IRegistryCheck
{
    string Id { get; }

    string DisplayName { get; }

    IReadOnlyList<IssueItem> Scan();
}

internal static class RegistryPathHelper
{
    public static string? ExtractExistingPath(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var value = raw.Trim().Trim('"');
        if (value.StartsWith("\"", StringComparison.Ordinal))
        {
            var end = value.IndexOf('"', 1);
            if (end > 1)
            {
                value = value[1..end];
            }
        }
        else
        {
            var exeIndex = value.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
            if (exeIndex >= 0)
            {
                value = value[..(exeIndex + 4)];
            }
        }

        value = Environment.ExpandEnvironmentVariables(value.Trim().Trim('"'));
        return value;
    }

    public static bool PathLooksValidButMissing(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        if (path.StartsWith("msiexec", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("rundll32", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!Path.IsPathRooted(path))
        {
            return false;
        }

        return !File.Exists(path) && !Directory.Exists(path);
    }

    public static IEnumerable<RegistryKey> OpenSubKeysSafe(RegistryKey parent)
    {
        string[] names;
        try
        {
            names = parent.GetSubKeyNames();
        }
        catch
        {
            yield break;
        }

        foreach (var name in names)
        {
            RegistryKey? key = null;
            try
            {
                key = parent.OpenSubKey(name);
            }
            catch
            {
                continue;
            }

            if (key is not null)
            {
                yield return key;
            }
        }
    }
}
