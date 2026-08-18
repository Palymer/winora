namespace WindowsOptimizer.Infrastructure.IO;

internal static class FileSystemScanHelper
{
    public static IEnumerable<FileInfo> EnumerateFilesSafe(
        string root,
        string searchPattern = "*",
        SearchOption option = SearchOption.AllDirectories)
    {
        if (!Directory.Exists(root))
        {
            yield break;
        }

        var pending = new Stack<string>();
        pending.Push(root);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            string[] files;
            try
            {
                files = Directory.GetFiles(current, searchPattern, SearchOption.TopDirectoryOnly);
            }
            catch (Exception)
            {
                files = Array.Empty<string>();
            }

            foreach (var file in files)
            {
                FileInfo info;
                try
                {
                    info = new FileInfo(file);
                }
                catch (Exception)
                {
                    continue;
                }

                yield return info;
            }

            if (option != SearchOption.AllDirectories)
            {
                continue;
            }

            string[] dirs;
            try
            {
                dirs = Directory.GetDirectories(current);
            }
            catch (Exception)
            {
                continue;
            }

            foreach (var dir in dirs)
            {
                pending.Push(dir);
            }
        }
    }

    public static long SumSizes(IEnumerable<FileInfo> files)
    {
        long total = 0;
        foreach (var file in files)
        {
            try
            {
                total += file.Length;
            }
            catch (Exception)
            {
                // skip locked/inaccessible files
            }
        }

        return total;
    }

    public static bool TryDeleteFile(string path, out string? error)
    {
        error = null;
        try
        {
            if (!File.Exists(path))
            {
                return true;
            }

            File.SetAttributes(path, FileAttributes.Normal);
            File.Delete(path);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public static bool TryDeleteDirectory(string path, out string? error)
    {
        error = null;
        try
        {
            if (!Directory.Exists(path))
            {
                return true;
            }

            Directory.Delete(path, recursive: true);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
}
