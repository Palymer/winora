namespace WindowsOptimizer.Core;

public static class AppPaths
{
    public static string Root =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Winora");

    public static string Logs => Path.Combine(Root, "Logs");

    public static string Backups => Path.Combine(Root, "Backups");

    public static void EnsureCreated()
    {
        Directory.CreateDirectory(Logs);
        Directory.CreateDirectory(Backups);
    }
}
