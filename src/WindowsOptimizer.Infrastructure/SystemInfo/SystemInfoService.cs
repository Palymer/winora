using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.Infrastructure.SystemInfo;

public sealed class SystemInfoService : ISystemInfoService
{
    [DllImport("kernel32.dll")]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx lpBuffer);

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatusEx
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    public SystemSnapshot GetSnapshot()
    {
        var memory = new MemoryStatusEx { dwLength = (uint)Marshal.SizeOf<MemoryStatusEx>() };
        if (!GlobalMemoryStatusEx(ref memory))
        {
            memory.ullTotalPhys = 0;
            memory.ullAvailPhys = 0;
        }

        var systemDrive = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\");
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);

        return new SystemSnapshot
        {
            ComputerName = Environment.MachineName,
            OsName = GetOsName(),
            OsVersion = Environment.OSVersion.VersionString,
            ProcessorName = GetProcessorName(),
            ProcessorCount = Environment.ProcessorCount,
            TotalMemoryBytes = (long)memory.ullTotalPhys,
            AvailableMemoryBytes = (long)memory.ullAvailPhys,
            SystemDriveTotalBytes = systemDrive.IsReady ? systemDrive.TotalSize : 0,
            SystemDriveFreeBytes = systemDrive.IsReady ? systemDrive.AvailableFreeSpace : 0,
            IsAdministrator = principal.IsInRole(WindowsBuiltInRole.Administrator),
            CapturedAt = DateTimeOffset.Now
        };
    }

    private static string GetOsName()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
            foreach (var item in searcher.Get())
            {
                return item["Caption"]?.ToString() ?? "Windows";
            }
        }
        catch
        {
            // WMI may be unavailable
        }

        return "Windows";
    }

    private static string GetProcessorName()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
            foreach (var item in searcher.Get())
            {
                return item["Name"]?.ToString()?.Trim() ?? "Неизвестно";
            }
        }
        catch
        {
            // WMI may be unavailable
        }

        return "Неизвестно";
    }
}
