using Microsoft.Win32;
using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;
using WindowsOptimizer.Infrastructure.RegistryRepair.Checks;

namespace WindowsOptimizer.Infrastructure.RegistryRepair;

public sealed class RegistryRepairService : IRegistryRepairService
{
    private readonly IReadOnlyList<IRegistryCheck> _checks;
    private readonly IBackupService _backupService;
    private readonly IOperationLogger _logger;

    public RegistryRepairService(
        IEnumerable<IRegistryCheck> checks,
        IBackupService backupService,
        IOperationLogger logger)
    {
        _checks = checks.ToList();
        _backupService = backupService;
        _logger = logger;
    }

    public Task<ScanResult> ScanAsync(
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var started = DateTimeOffset.Now;
            var issues = new List<IssueItem>();

            for (var i = 0; i < _checks.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var check = _checks[i];
                progress?.Report(new OperationProgress
                {
                    CurrentStep = $"Проверка реестра: {check.DisplayName}",
                    Percent = _checks.Count == 0 ? 100 : i * 100 / _checks.Count,
                    ProcessedItems = i,
                    TotalItems = _checks.Count
                });

                try
                {
                    issues.AddRange(check.Scan());
                }
                catch (Exception ex)
                {
                    _logger.Error($"Ошибка проверки {check.Id}", ex);
                }
            }

            return new ScanResult
            {
                StartedAt = started,
                Duration = DateTimeOffset.Now - started,
                Issues = issues
            };
        }, cancellationToken);
    }

    public async Task<OperationResult> RepairAsync(
        IReadOnlyList<IssueItem> items,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (items.Count == 0)
        {
            return OperationResult.Empty("Нет выбранных записей");
        }

        string? backupPath = null;
        try
        {
            progress?.Report(new OperationProgress
            {
                CurrentStep = "Создание резервной копии реестра",
                Percent = 5,
                ProcessedItems = 0,
                TotalItems = items.Count
            });
            backupPath = await _backupService.CreateRegistryBackupAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.Error("Не удалось создать резервную копию реестра", ex);
            return OperationResult.Fail("Ремонт отменён: не удалось создать резервную копию реестра.");
        }

        var fixedCount = 0;
        var failed = 0;
        var messages = new List<string>();

        for (var i = 0; i < items.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var item = items[i];
            progress?.Report(new OperationProgress
            {
                CurrentStep = item.Title,
                Percent = 10 + (i + 1) * 90 / items.Count,
                ProcessedItems = i + 1,
                TotalItems = items.Count
            });

            try
            {
                Apply(item);
                fixedCount++;
                _logger.Info($"Исправлено: {item.Title} ({item.RegistryHive}\\{item.RegistryPath})");
            }
            catch (Exception ex)
            {
                failed++;
                messages.Add($"{item.Title}: {ex.Message}");
                _logger.Error($"Не удалось исправить {item.Title}", ex);
            }
        }

        return new OperationResult
        {
            Success = failed == 0,
            FixedCount = fixedCount,
            FailedCount = failed,
            BackupPath = backupPath,
            Messages = messages
        };
    }

    private static void Apply(IssueItem item)
    {
        if (string.IsNullOrWhiteSpace(item.RegistryHive) || string.IsNullOrWhiteSpace(item.RegistryPath))
        {
            throw new InvalidOperationException("Не указан путь реестра");
        }

        var hive = item.RegistryHive.Equals("HKCU", StringComparison.OrdinalIgnoreCase)
            ? Registry.CurrentUser
            : Registry.LocalMachine;

        switch (item.Action)
        {
            case Core.Enums.RepairAction.DeleteRegistryKey:
                hive.DeleteSubKeyTree(item.RegistryPath, throwOnMissingSubKey: false);
                break;
            case Core.Enums.RepairAction.DeleteRegistryValue:
            {
                using var key = hive.OpenSubKey(item.RegistryPath, writable: true);
                if (key is null || string.IsNullOrWhiteSpace(item.RegistryValueName))
                {
                    throw new InvalidOperationException("Ключ или значение не найдено");
                }

                key.DeleteValue(item.RegistryValueName, throwOnMissingValue: false);
                break;
            }
            default:
                throw new NotSupportedException($"Действие {item.Action} не поддерживается");
        }
    }
}
