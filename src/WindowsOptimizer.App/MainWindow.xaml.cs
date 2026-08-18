using System.Reflection;
using System.Windows;
using WindowsOptimizer.App.ViewModels;

namespace WindowsOptimizer.App;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        VersionLabel.Text = FormatVersionLabel();
    }

    private static string FormatVersionLabel()
    {
        var info = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        var version = string.IsNullOrWhiteSpace(info) ? "0.1.0-Alpha" : info.Split('+')[0];
        return $"v{version} · win-x64";
    }
}
