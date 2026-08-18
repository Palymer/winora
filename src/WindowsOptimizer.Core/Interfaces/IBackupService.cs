namespace WindowsOptimizer.Core.Interfaces;

public interface IBackupService
{
    Task<string> CreateRegistryBackupAsync(CancellationToken cancellationToken = default);

    Task<bool> CreateRestorePointAsync(string description, CancellationToken cancellationToken = default);
}
