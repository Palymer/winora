using System.Diagnostics;
using System.Management;
using Microsoft.Win32;
using WindowsOptimizer.Core;
using WindowsOptimizer.Core.Interfaces;

namespace WindowsOptimizer.Infrastructure.Backup;

public sealed class BackupService : IBackupService
{
    private readonly IOperationLogger _logger;

    public BackupService(IOperationLogger logger)
    {
        _logger = logger;
        AppPaths.EnsureCreated();
    }

    public async Task<string> CreateRegistryBackupAsync(CancellationToken cancellationToken = default)
    {
        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var folder = Path.Combine(AppPaths.Backups, stamp);
        Directory.CreateDirectory(folder);

        var exports = new (string Hive, string FileName)[]
        {
            ("HKCU", "hkcu.reg"),
            ("HKLM\\SOFTWARE", "hklm-software.reg"),
            ("HKLM\\SYSTEM\\CurrentControlSet\\Services", "hklm-services.reg")
        };

        foreach (var (hive, fileName) in exports)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var target = Path.Combine(folder, fileName);
            await ExportHiveAsync(hive, target, cancellationToken).ConfigureAwait(false);
        }

        _logger.Info($"Создана резервная копия реестра: {folder}");
        return folder;
    }

    public Task<bool> CreateRestorePointAsync(string description, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                EnableRestoreIfNeeded();

                using var sysRestore = new ManagementClass(@"\\.\root\default", "SystemRestore", new ObjectGetOptions());
                using var inParams = sysRestore.GetMethodParameters("CreateRestorePoint");
                inParams["Description"] = description;
                inParams["RestorePointType"] = 0; // APPLICATION_INSTALL
                inParams["EventType"] = 100;     // BEGIN_SYSTEM_CHANGE
                using var result = sysRestore.InvokeMethod("CreateRestorePoint", inParams, null);
                var status = Convert.ToInt32(result?["ReturnValue"] ?? -1);
                var ok = status == 0;
                if (ok)
                {
                    _logger.Info($"Создана точка восстановления: {description}");
                }
                else
                {
                    _logger.Warning($"Не удалось создать точку восстановления, код {status}");
                }

                return ok;
            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка создания точки восстановления", ex);
                return false;
            }
        }, cancellationToken);
    }

    private static void EnableRestoreIfNeeded()
    {
        try
        {
            using var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore");
            key?.SetValue("SystemRestorePointCreationFrequency", 0, RegistryValueKind.DWord);
        }
        catch
        {
            // best effort
        }
    }

    private async Task ExportHiveAsync(string hive, string targetPath, CancellationToken cancellationToken)
    {
        var start = new ProcessStartInfo
        {
            FileName = "reg.exe",
            Arguments = $"export \"{hive}\" \"{targetPath}\" /y",
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };

        using var process = Process.Start(start);
        if (process is null)
        {
            throw new InvalidOperationException("Не удалось запустить reg.exe");
        }

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            _logger.Warning($"reg export {hive} завершился с кодом {process.ExitCode}: {error}");
        }
    }
}
