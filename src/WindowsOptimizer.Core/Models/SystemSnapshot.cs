namespace WindowsOptimizer.Core.Models;

public sealed class SystemSnapshot
{
    public required string ComputerName { get; init; }

    public required string OsName { get; init; }

    public required string OsVersion { get; init; }

    public required string ProcessorName { get; init; }

    public required int ProcessorCount { get; init; }

    public required long TotalMemoryBytes { get; init; }

    public required long AvailableMemoryBytes { get; init; }

    public required long SystemDriveTotalBytes { get; init; }

    public required long SystemDriveFreeBytes { get; init; }

    public required bool IsAdministrator { get; init; }

    public required DateTimeOffset CapturedAt { get; init; }

    public int UsedMemoryPercent =>
        TotalMemoryBytes == 0
            ? 0
            : (int)Math.Clamp(100 - AvailableMemoryBytes * 100 / TotalMemoryBytes, 0, 100);

    public int UsedDiskPercent =>
        SystemDriveTotalBytes == 0
            ? 0
            : (int)Math.Clamp((SystemDriveTotalBytes - SystemDriveFreeBytes) * 100 / SystemDriveTotalBytes, 0, 100);
}
