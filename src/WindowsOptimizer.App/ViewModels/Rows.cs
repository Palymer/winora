using CommunityToolkit.Mvvm.ComponentModel;
using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.App.ViewModels;

public sealed class IssueRowViewModel : ObservableObject
{
    private bool _isSelected = true;

    public IssueRowViewModel(IssueItem item)
    {
        Item = item;
    }

    public IssueItem Item { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

public sealed class CleanerRowViewModel : ObservableObject
{
    private bool _isSelected = true;

    public CleanerRowViewModel(ISystemCleaner cleaner)
    {
        Cleaner = cleaner;
    }

    public ISystemCleaner Cleaner { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

public sealed class ToggleRowViewModel : ObservableObject
{
    public ToggleRowViewModel(OptimizationToggle model)
    {
        Model = model;
        _isEnabled = model.IsEnabled;
    }

    public OptimizationToggle Model { get; }

    private bool _isEnabled;

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (SetProperty(ref _isEnabled, value))
            {
                Model.IsEnabled = value;
            }
        }
    }
}

public sealed class ToggleGroupViewModel
{
    public required string Name { get; init; }

    public required IReadOnlyList<ToggleRowViewModel> Items { get; init; }
}
