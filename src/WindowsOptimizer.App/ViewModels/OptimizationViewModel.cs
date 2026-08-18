using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.App.ViewModels;

public sealed partial class OptimizationViewModel : ObservableObject
{
    private readonly IOptimizerOrchestrator _orchestrator;
    private readonly IOperationLogger _logger;

    public OptimizationViewModel(IOptimizerOrchestrator orchestrator, IOperationLogger logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
        Reload();
    }

    public ObservableCollection<ToggleGroupViewModel> Groups { get; } = new();

    [ObservableProperty] private string _status = "Включите нужные параметры и нажмите «Применить»";
    [ObservableProperty] private bool _isBusy;

    [RelayCommand]
    private void Reload()
    {
        Groups.Clear();
        var grouped = _orchestrator.GetAllToggles()
            .GroupBy(t => t.Group)
            .Select(g => new ToggleGroupViewModel
            {
                Name = g.Key,
                Items = g.Select(t => new ToggleRowViewModel(t)).ToList()
            });

        foreach (var group in grouped)
        {
            Groups.Add(group);
        }
    }

    [RelayCommand]
    private void EnableRecommended()
    {
        foreach (var group in Groups)
        {
            foreach (var item in group.Items.Where(i => i.Model.IsRecommended))
            {
                item.IsEnabled = true;
            }
        }
    }

    [RelayCommand]
    private async Task ApplyAsync()
    {
        var toggles = Groups.SelectMany(g => g.Items.Select(i => i.Model)).ToList();
        IsBusy = true;
        try
        {
            var result = await _orchestrator.ApplyAsync(toggles).ConfigureAwait(true);
            Status = result.Success
                ? $"Применено параметров: {result.FixedCount}"
                : $"Готово с ошибками. Успешно: {result.FixedCount}, ошибок: {result.FailedCount}";
            if (toggles.Any(t => t.IsEnabled && t.RequiresRestart))
            {
                Status += ". Некоторые изменения требуют перезагрузки.";
            }

            Reload();
        }
        catch (Exception ex)
        {
            _logger.Error("Ошибка применения оптимизаций", ex);
            Status = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
