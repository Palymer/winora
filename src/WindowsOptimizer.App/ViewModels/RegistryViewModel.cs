using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.App.ViewModels;

public sealed partial class RegistryViewModel : ObservableObject
{
    private readonly IRegistryRepairService _registry;
    private readonly IBackupService _backup;
    private readonly IOperationLogger _logger;

    public RegistryViewModel(IRegistryRepairService registry, IBackupService backup, IOperationLogger logger)
    {
        _registry = registry;
        _backup = backup;
        _logger = logger;
    }

    public ObservableCollection<IssueRowViewModel> Issues { get; } = new();

    [ObservableProperty] private string _status = "Сначала выполните сканирование. Перед ремонтом будет создана резервная копия.";
    [ObservableProperty] private int _progressPercent;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _summary = "0 проблем";
    [ObservableProperty] private string? _lastBackupPath;

    [RelayCommand]
    private async Task ScanAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            await ScanInternalAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            _logger.Error("Ошибка сканирования реестра", ex);
            Status = ex.Message;
        }
        finally
        {
            IsBusy = false;
            ProgressPercent = 0;
        }
    }

    [RelayCommand]
    private async Task RepairAsync()
    {
        var selected = Issues.Where(i => i.IsSelected).Select(i => i.Item).ToList();
        if (selected.Count == 0)
        {
            Status = "Нет выбранных записей";
            return;
        }

        IsBusy = true;
        try
        {
            var progress = new Progress<OperationProgress>(p =>
            {
                Status = p.CurrentStep;
                ProgressPercent = p.Percent;
            });

            var result = await _registry.RepairAsync(selected, progress).ConfigureAwait(true);
            LastBackupPath = result.BackupPath;
            var repairStatus = result.Success
                ? $"Исправлено записей: {result.FixedCount}. Резервная копия: {result.BackupPath}"
                : $"Готово с ошибками. Успешно: {result.FixedCount}, ошибок: {result.FailedCount}";
            await ScanInternalAsync().ConfigureAwait(true);
            Status = repairStatus;
        }
        catch (Exception ex)
        {
            _logger.Error("Ошибка ремонта реестра", ex);
            Status = ex.Message;
        }
        finally
        {
            IsBusy = false;
            ProgressPercent = 0;
        }
    }

    [RelayCommand]
    private async Task BackupOnlyAsync()
    {
        IsBusy = true;
        Status = "Создание резервной копии реестра…";
        try
        {
            LastBackupPath = await _backup.CreateRegistryBackupAsync().ConfigureAwait(true);
            Status = $"Резервная копия сохранена: {LastBackupPath}";
        }
        catch (Exception ex)
        {
            _logger.Error("Ошибка резервного копирования", ex);
            Status = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RestorePointAsync()
    {
        IsBusy = true;
        Status = "Создание точки восстановления…";
        try
        {
            var ok = await _backup.CreateRestorePointAsync("Windows Optimizer").ConfigureAwait(true);
            Status = ok
                ? "Точка восстановления создана"
                : "Не удалось создать точку восстановления. Проверьте, что защита системы включена.";
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var issue in Issues)
        {
            issue.IsSelected = true;
        }
    }

    [RelayCommand]
    private void SelectNone()
    {
        foreach (var issue in Issues)
        {
            issue.IsSelected = false;
        }
    }

    private async Task ScanInternalAsync()
    {
        Issues.Clear();
        var progress = new Progress<OperationProgress>(p =>
        {
            Status = p.CurrentStep;
            ProgressPercent = p.Percent;
        });

        var result = await _registry.ScanAsync(progress).ConfigureAwait(true);
        foreach (var issue in result.Issues)
        {
            Issues.Add(new IssueRowViewModel(issue));
        }

        Summary = $"{result.IssueCount} проблем реестра";
        Status = result.IssueCount == 0
            ? "Битых записей не найдено"
            : "Проверьте список и исправьте выбранные записи";
    }
}
