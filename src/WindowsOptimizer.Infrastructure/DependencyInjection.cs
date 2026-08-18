using Microsoft.Extensions.DependencyInjection;
using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Infrastructure.Backup;
using WindowsOptimizer.Infrastructure.Cleaning;
using WindowsOptimizer.Infrastructure.Logging;
using WindowsOptimizer.Infrastructure.Optimization;
using WindowsOptimizer.Infrastructure.RegistryRepair;
using WindowsOptimizer.Infrastructure.RegistryRepair.Checks;
using WindowsOptimizer.Infrastructure.Scanning;
using WindowsOptimizer.Infrastructure.SystemInfo;

namespace WindowsOptimizer.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWindowsOptimizer(this IServiceCollection services)
    {
        services.AddSingleton<IOperationLogger, FileOperationLogger>();
        services.AddSingleton<ISystemInfoService, SystemInfoService>();
        services.AddSingleton<IBackupService, BackupService>();

        services.AddSingleton<ISystemCleaner, TempFilesCleaner>();
        services.AddSingleton<ISystemCleaner, RecycleBinCleaner>();
        services.AddSingleton<ISystemCleaner, BrokenShortcutsCleaner>();
        services.AddSingleton<ISystemCleaner, WindowsLogsCleaner>();
        services.AddSingleton<ISystemCleaner, ThumbnailCacheCleaner>();
        services.AddSingleton<ISystemCleaner, PrefetchCleaner>();
        services.AddSingleton<ISystemCleaner, BrowserCacheCleaner>();
        services.AddSingleton<ISystemCleaner, DeliveryOptimizationCleaner>();
        services.AddSingleton<ISystemCleaner, RecentItemsCleaner>();
        services.AddSingleton<ISystemCleaner, ShaderCacheCleaner>();
        services.AddSingleton<ISystemCleaner, IconCacheCleaner>();
        services.AddSingleton<ICleanerOrchestrator, CleanerOrchestrator>();

        services.AddSingleton<IRegistryCheck, UninstallEntriesCheck>();
        services.AddSingleton<IRegistryCheck, StartupEntriesCheck>();
        services.AddSingleton<IRegistryCheck, AppPathsCheck>();
        services.AddSingleton<IRegistryCheck, SharedDllsCheck>();
        services.AddSingleton<IRegistryRepairService, RegistryRepairService>();

        services.AddSingleton<IOptimizer, PerformanceOptimizer>();
        services.AddSingleton<IOptimizer, PrivacyOptimizer>();
        services.AddSingleton<IOptimizer, PrivacyMoreOptimizer>();
        services.AddSingleton<IOptimizer, ExplorerOptimizer>();
        services.AddSingleton<IOptimizer, ExplorerUiOptimizer>();
        services.AddSingleton<IOptimizer, GamingOptimizer>();
        services.AddSingleton<IOptimizer, NetworkOptimizer>();
        services.AddSingleton<IOptimizer, SystemOptimizer>();
        services.AddSingleton<IOptimizer, InputOptimizer>();
        services.AddSingleton<IOptimizer, EdgeOptimizer>();
        services.AddSingleton<IOptimizer, ServicesOptimizer>();
        services.AddSingleton<IOptimizer, TelemetryTasksOptimizer>();
        services.AddSingleton<IOptimizerOrchestrator, OptimizerOrchestrator>();

        services.AddSingleton<IScanEngine, ScanEngine>();
        return services;
    }
}
