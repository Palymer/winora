using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WindowsOptimizer.Core.Formatting;
using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.App.ViewModels;

public sealed partial class CleaningViewModel : ObservableObject
{
    private readonly ICleanerOrchestrator _orchestrator;
    private readonly IOperationLogger _logger;

    public CleaningViewModel(ICleanerOrchestrator orchestrator, IOperationLogger logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
        foreach (var cleaner in orchestrator.Cleaners)
        {
            Cleaners.Add(new CleanerRowViewModel(cleaner));
        }
    }

    public ObservableCollection<CleanerRowViewModel> Cleaners { get; } = new();
    public ObservableCollection<IssueRowViewModel> Issues { get; } = new();

    [ObservableProperty] private string _status = "Выберите категории и запустите сканирование";
    [ObservableProperty] private int _progressPercent;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _summary = "0 элементов";

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
            _logger.Error("Ошибка сканирования очистки", ex);
            Status = ex.Message;
        }
        finally
        {
            IsBusy = false;
            ProgressPercent = 0;
        }
    }

    [RelayCommand]
    private async Task CleanAsync()
    {
        var selected = Issues.Where(i => i.IsSelected).Select(i => i.Item).ToList();
        if (selected.Count == 0)
        {
            Status = "Нет выбранных элементов для очистки";
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

            var result = await _orchestrator.CleanAsync(selected, progress).ConfigureAwait(true);
            Status = result.Success
                ? $"Очистка завершена. Удалено файлов: {result.FixedCount}, освобождено {ByteFormatter.ToHuman(result.FreedBytes)}"
                : $"Очистка завершена с ошибками. Успешно: {result.FixedCount}, ошибок: {result.FailedCount}";
            await ScanInternalAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            _logger.Error("Ошибка очистки", ex);
            Status = ex.Message;
        }
        finally
        {
            IsBusy = false;
            ProgressPercent = 0;
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
        var ids = Cleaners.Where(c => c.IsSelected).Select(c => c.Cleaner.Id).ToList();
        if (ids.Count == 0)
        {
            Status = "Выберите хотя бы одну категорию очистки";
            return;
        }

        Issues.Clear();
        var progress = new Progress<OperationProgress>(p =>
        {
            Status = p.CurrentStep;
            ProgressPercent = p.Percent;
        });

        var result = await _orchestrator.ScanAsync(ids, progress).ConfigureAwait(true);
        foreach (var issue in result.Issues)
        {
            Issues.Add(new IssueRowViewModel(issue));
        }

        Summary = $"{result.IssueCount} элементов · {ByteFormatter.ToHuman(result.TotalSizeBytes)}";
        Status = result.IssueCount == 0 ? "Мусор не найден" : $"Можно освободить {ByteFormatter.ToHuman(result.TotalSizeBytes)}";
    }
}
