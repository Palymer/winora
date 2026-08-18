using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WindowsOptimizer.App.ViewModels;
using WindowsOptimizer.Core;
using WindowsOptimizer.Infrastructure;

namespace WindowsOptimizer.App;

public partial class App : Application
{
    private IServiceProvider? _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        if (!IsAdministrator() && TryRelaunchElevated())
        {
            Shutdown();
            return;
        }

        base.OnStartup(e);
        AppPaths.EnsureCreated();

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddDebug());
        services.AddWindowsOptimizer();
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<CleaningViewModel>();
        services.AddSingleton<RegistryViewModel>();
        services.AddSingleton<OptimizationViewModel>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        _services = services.BuildServiceProvider();
        var window = _services.GetRequiredService<MainWindow>();
        window.Show();
    }

    private static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    }

    private static bool TryRelaunchElevated()
    {
        var exe = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(exe))
        {
            return false;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = exe,
                UseShellExecute = true,
                Verb = "runas",
                WorkingDirectory = Environment.CurrentDirectory
            });
            return true;
        }
        catch (Win32Exception)
        {
            MessageBox.Show(
                "Нет прав администратора. Часть функций очистки и реестра может быть недоступна.",
                "Winora",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.Exception.Message, "Winora", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            MessageBox.Show(ex.Message, "Winora", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
