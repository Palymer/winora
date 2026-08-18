using System.Runtime.InteropServices;
using System.Text;
using WindowsOptimizer.Infrastructure.Native;

namespace WindowsOptimizer.Infrastructure.Cleaning;

internal static class ShortcutScanner
{
    public static IReadOnlyList<(string LinkPath, string Target, long Size)> FindBroken(CancellationToken cancellationToken)
    {
        return ComApartment.Run(() =>
        {
            var broken = new List<(string LinkPath, string Target, long Size)>();
            foreach (var folder in GetSearchRoots())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!Directory.Exists(folder))
                {
                    continue;
                }

                IReadOnlyList<string> files;
                try
                {
                    files = Directory.EnumerateFiles(folder, "*.lnk", SearchOption.AllDirectories).ToList();
                }
                catch
                {
                    continue;
                }

                foreach (var file in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        var target = ResolveTarget(file);
                        if (target is null)
                        {
                            continue;
                        }

                        if (IsBroken(target))
                        {
                            broken.Add((file, target, new FileInfo(file).Length));
                        }
                    }
                    catch
                    {
                        // skip unreadable shortcuts
                    }
                }
            }

            return (IReadOnlyList<(string, string, long)>)broken;
        });
    }

    internal static bool IsBroken(string target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return false;
        }

        var expanded = Environment.ExpandEnvironmentVariables(target.Trim().Trim('"'));
        if (expanded.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            expanded.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            expanded.StartsWith("microsoft-edge:", StringComparison.OrdinalIgnoreCase) ||
            expanded.StartsWith("shell:", StringComparison.OrdinalIgnoreCase) ||
            expanded.StartsWith("::{", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!Path.IsPathRooted(expanded))
        {
            return false;
        }

        return !File.Exists(expanded) && !Directory.Exists(expanded);
    }

    private static string? ResolveTarget(string lnkPath)
    {
        try
        {
            var link = (IShellLinkW)new ShellLink();
            try
            {
                ((IPersistFile)link).Load(lnkPath, 0);
                var sb = new StringBuilder(260);
                link.GetPath(sb, sb.Capacity, IntPtr.Zero, 0);
                var target = sb.ToString();
                return string.IsNullOrWhiteSpace(target) ? null : target;
            }
            finally
            {
                Marshal.FinalReleaseComObject(link);
            }
        }
        catch
        {
            return ResolveTargetViaWsh(lnkPath);
        }
    }

    private static string? ResolveTargetViaWsh(string lnkPath)
    {
        try
        {
            var type = Type.GetTypeFromProgID("WScript.Shell");
            if (type is null)
            {
                return null;
            }

            dynamic wsh = Activator.CreateInstance(type)!;
            dynamic shortcut = wsh.CreateShortcut(lnkPath);
            var target = shortcut.TargetPath as string;
            return string.IsNullOrWhiteSpace(target) ? null : target;
        }
        catch
        {
            return null;
        }
    }

    private static IEnumerable<string> GetSearchRoots()
    {
        yield return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        yield return Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
        yield return Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
        yield return Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
        yield return Environment.GetFolderPath(Environment.SpecialFolder.Recent);
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        yield return Path.Combine(appData, @"Microsoft\Internet Explorer\Quick Launch");
        yield return Path.Combine(appData, @"Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar");
    }

    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    private class ShellLink
    {
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    private interface IShellLinkW
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cch, IntPtr pfd, int fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cch);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cch);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cch);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cch, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
        void Resolve(IntPtr hwnd, int fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [ComImport]
    [Guid("0000010b-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IPersistFile
    {
        void GetClassID(out Guid pClassId);
        [PreserveSig]
        int IsDirty();
        void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
        void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);
        void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
        void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
    }
}
