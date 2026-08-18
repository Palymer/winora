using System.Runtime.InteropServices;

namespace WindowsOptimizer.Infrastructure.Native;

internal static class ShellNative
{
    public const uint SHERB_NOCONFIRMATION = 0x00000001;
    public const uint SHERB_NOPROGRESSUI = 0x00000002;
    public const uint SHERB_NOSOUND = 0x00000004;

    /// <summary>
    /// Windows expects 20 bytes (pack 4). Passing 24 on x64 makes SHQueryRecycleBin return zeros.
    /// </summary>
    public const int ShQueryRbInfoSize = 20;

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct SHQUERYRBINFO
    {
        public int cbSize;
        public long i64Size;
        public long i64NumItems;
    }

    [DllImport("shell32.dll", EntryPoint = "SHQueryRecycleBinW", CharSet = CharSet.Unicode, ExactSpelling = true)]
    public static extern int SHQueryRecycleBin(string pszRootPath, ref SHQUERYRBINFO pSHQueryRbInfo);

    [DllImport("shell32.dll", EntryPoint = "SHEmptyRecycleBinW", CharSet = CharSet.Unicode, ExactSpelling = true)]
    public static extern int SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, uint dwFlags);
}
