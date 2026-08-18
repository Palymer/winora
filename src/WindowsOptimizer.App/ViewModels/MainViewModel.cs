using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WindowsOptimizer.App.ViewModels;

public enum AppSection
{
    Dashboard,
    Cleaning,
    Registry,
    Optimization
}

public sealed partial class MainViewModel : ObservableObject
{
    public MainViewModel(
        DashboardViewModel dashboard,
        CleaningViewModel cleaning,
        RegistryViewModel registry,
        OptimizationViewModel optimization)
    {
        Dashboard = dashboard;
        Cleaning = cleaning;
        Registry = registry;
        Optimization = optimization;
        CurrentSection = AppSection.Dashboard;
        CurrentPage = dashboard;
    }

    public DashboardViewModel Dashboard { get; }
    public CleaningViewModel Cleaning { get; }
    public RegistryViewModel Registry { get; }
    public OptimizationViewModel Optimization { get; }

    [ObservableProperty] private AppSection _currentSection;
    [ObservableProperty] private object _currentPage = null!;
    [ObservableProperty] private string _headerTitle = "Обзор системы";
    [ObservableProperty] private string _headerSubtitle = "Состояние компьютера и полная проверка";

    public bool IsDashboard => CurrentSection == AppSection.Dashboard;
    public bool IsCleaning => CurrentSection == AppSection.Cleaning;
    public bool IsRegistry => CurrentSection == AppSection.Registry;
    public bool IsOptimization => CurrentSection == AppSection.Optimization;

    [RelayCommand]
    private void Navigate(string section)
    {
        if (!Enum.TryParse<AppSection>(section, out var parsed))
        {
            return;
        }

        CurrentSection = parsed;
        (CurrentPage, HeaderTitle, HeaderSubtitle) = parsed switch
        {
            AppSection.Cleaning => ((object)Cleaning, "Очистка диска", "Временные файлы, корзина, кэш и битые ярлыки"),
            AppSection.Registry => (Registry, "Ремонт реестра", "Битые записи, резервная копия и точка восстановления"),
            AppSection.Optimization => (Optimization, "Оптимизация", "Производительность, приватность и службы Windows"),
            _ => (Dashboard, "Обзор системы", "Состояние компьютера и полная проверка")
        };
    }

    partial void OnCurrentSectionChanged(AppSection value)
    {
        OnPropertyChanged(nameof(IsDashboard));
        OnPropertyChanged(nameof(IsCleaning));
        OnPropertyChanged(nameof(IsRegistry));
        OnPropertyChanged(nameof(IsOptimization));
    }
}
