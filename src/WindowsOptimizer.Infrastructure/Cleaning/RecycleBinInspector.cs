using WindowsOptimizer.Infrastructure.IO;
using WindowsOptimizer.Infrastructure.Native;

namespace WindowsOptimizer.Infrastructure.Cleaning;

internal sealed record RecycleBinSnapshot(int ItemCount, long SizeBytes);

internal static class RecycleBinInspector
{
    public static RecycleBinSnapshot Query()
    {
        return ComApartment.Run(() =>
        {
            var fromDisk = QueryFromDisk();
            var fromApi = QueryFromShellApi();
            return new RecycleBinSnapshot(
                Math.Max(fromDisk.ItemCount, fromApi.ItemCount),
                Math.Max(fromDisk.SizeBytes, fromApi.SizeBytes));
        });
    }

    public static int Empty()
    {
        return ComApartment.Run(() =>
        {
            var flags = ShellNative.SHERB_NOCONFIRMATION | ShellNative.SHERB_NOPROGRESSUI | ShellNative.SHERB_NOSOUND;
            var lastHr = 0;

            foreach (var root in GetDriveRoots().Append(string.Empty))
            {
                var hr = ShellNative.SHEmptyRecycleBin(IntPtr.Zero, root, flags);
                if (hr != 0 && (uint)hr != 0x8000FFFF)
                {
                    lastHr = hr;
                }
            }

            EmptyFromDisk();
            return lastHr;
        });
    }

    private static RecycleBinSnapshot QueryFromShellApi()
    {
        long size = 0;
        long items = 0;
        var anyDrive = false;
        foreach (var root in GetDriveRoots())
        {
            var info = new ShellNative.SHQUERYRBINFO { cbSize = ShellNative.ShQueryRbInfoSize };
            if (ShellNative.SHQueryRecycleBin(root, ref info) != 0)
            {
                continue;
            }

            anyDrive = true;
            size += Math.Max(0, info.i64Size);
            items += Math.Max(0, info.i64NumItems);
        }

        if (!anyDrive)
        {
            var info = new ShellNative.SHQUERYRBINFO { cbSize = ShellNative.ShQueryRbInfoSize };
            if (ShellNative.SHQueryRecycleBin(string.Empty, ref info) == 0)
            {
                size = Math.Max(0, info.i64Size);
                items = Math.Max(0, info.i64NumItems);
            }
        }

        return new RecycleBinSnapshot((int)Math.Min(items, int.MaxValue), size);
    }

    private static RecycleBinSnapshot QueryFromDisk()
    {
        long size = 0;
        var items = 0;
        foreach (var file in EnumerateRecycleFiles())
        {
            if (file.Name.StartsWith("$R", StringComparison.OrdinalIgnoreCase))
            {
                items++;
            }

            try
            {
                size += file.Length;
            }
            catch
            {
                // ignore
            }
        }

        return new RecycleBinSnapshot(items, size);
    }

    private static void EmptyFromDisk()
    {
        foreach (var file in EnumerateRecycleFiles().ToList())
        {
            FileSystemScanHelper.TryDeleteFile(file.FullName, out _);
        }
    }

    private static IEnumerable<FileInfo> EnumerateRecycleFiles()
    {
        foreach (var root in GetDriveRoots())
        {
            var recycleDir = Path.Combine(root, "$Recycle.Bin");
            if (!Directory.Exists(recycleDir))
            {
                continue;
            }

            foreach (var file in FileSystemScanHelper.EnumerateFilesSafe(recycleDir))
            {
                var name = file.Name;
                if (name.Equals("desktop.ini", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (name.StartsWith("$", StringComparison.OrdinalIgnoreCase))
                {
                    yield return file;
                }
            }
        }
    }

    private static IEnumerable<string> GetDriveRoots()
    {
        foreach (var drive in DriveInfo.GetDrives())
        {
            if (drive.IsReady && (drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Removable))
            {
                yield return drive.RootDirectory.FullName;
            }
        }
    }
}
