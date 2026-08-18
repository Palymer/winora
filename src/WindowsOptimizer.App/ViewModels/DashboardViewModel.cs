using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WindowsOptimizer.Core.Formatting;
using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.App.ViewModels;

public sealed partial class DashboardViewModel : ObservableObject
{
    private readonly ISystemInfoService _systemInfo;
    private readonly IScanEngine _scanEngine;

    public DashboardViewModel(ISystemInfoService systemInfo, IScanEngine scanEngine)
    {
        _systemInfo = systemInfo;
        _scanEngine = scanEngine;
        RefreshSnapshot();
    }

    [ObservableProperty] private SystemSnapshot? _snapshot;
    [ObservableProperty] private string _status = "Готов к полной проверке системы";
    [ObservableProperty] private int _progressPercent;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private int _lastIssueCount;
    [ObservableProperty] private string _lastFreedHint = "—";
    [ObservableProperty] private string _lastDuration = "—";

    public ObservableCollection<IssueRowViewModel> LastIssues { get; } = new();

    public void RefreshSnapshot() => Snapshot = _systemInfo.GetSnapshot();

    [RelayCommand]
    private async Task ScanAllAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        Status = "Полное сканирование…";
        ProgressPercent = 0;
        LastIssues.Clear();

        try
        {
            var progress = new Progress<OperationProgress>(p =>
            {
                Status = p.CurrentStep;
                ProgressPercent = p.Percent;
            });

            var result = await _scanEngine.ScanAllAsync(progress).ConfigureAwait(true);
            foreach (var issue in result.Issues)
            {
                LastIssues.Add(new IssueRowViewModel(issue));
            }

            LastIssueCount = result.IssueCount;
            LastFreedHint = ByteFormatter.ToHuman(result.TotalSizeBytes);
            LastDuration = $"{result.Duration.TotalSeconds:0.0} с";
            Status = result.IssueCount == 0
                ? "Проблем не найдено"
                : $"Найдено проблем: {result.IssueCount}";
            RefreshSnapshot();
        }
        catch (Exception ex)
        {
            Status = $"Ошибка сканирования: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            ProgressPercent = 0;
        }
    }
}
