using System.Diagnostics;
using System.ServiceProcess;
using Microsoft.Win32;
using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.Infrastructure.Optimization;

public sealed class ExplorerOptimizer : IOptimizer
{
    public string Id => "explorer";
    public string DisplayName => "Проводник";

    private static readonly RegistryToggleSpec[] Specs =
    [
        new()
        {
            Id = "show-extensions",
            Group = "Проводник",
            Name = "Показывать расширения файлов",
            Description = "HideFileExt = 0",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            ValueName = "HideFileExt",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "launch-this-pc",
            Group = "Проводник",
            Name = "Открывать проводник в «Этот компьютер»",
            Description = "LaunchTo = 1",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            ValueName = "LaunchTo",
            EnabledValue = 1,
            DisabledValue = 2
        },
        new()
        {
            Id = "transparency",
            Group = "Проводник",
            Name = "Отключить прозрачность",
            Description = "Снимает Acrylic/прозрачность окон и панели задач",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
            ValueName = "EnableTransparency",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "widgets",
            Group = "Проводник",
            Name = "Скрыть виджеты / новости",
            Description = "TaskbarDa = 0 (Windows 11 Widgets)",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            ValueName = "TaskbarDa",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "bing-search",
            Group = "Проводник",
            Name = "Отключить поиск Bing в меню Пуск",
            Description = "BingSearchEnabled = 0",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Search",
            ValueName = "BingSearchEnabled",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "search-highlights",
            Group = "Проводник",
            Name = "Отключить подсветку поиска",
            Description = "IsDynamicSearchBoxEnabled = 0",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\Windows\CurrentVersion\SearchSettings",
            ValueName = "IsDynamicSearchBoxEnabled",
            EnabledValue = 0,
            DisabledValue = 1
        }
    ];

    public IReadOnlyList<OptimizationToggle> GetToggles() =>
        Specs.Select(RegistryToggleHelper.ToToggle).ToList();

    public Task<OperationResult> ApplyAsync(IReadOnlyList<OptimizationToggle> toggles, CancellationToken cancellationToken = default) =>
        Task.Run(() => PerformanceOptimizer.ApplySpecs(Specs, toggles, cancellationToken), cancellationToken);
}

public sealed class GamingOptimizer : IOptimizer
{
    public string Id => "gaming";
    public string DisplayName => "Игры";

    private static readonly RegistryToggleSpec[] Specs =
    [
        new()
        {
            Id = "game-bar",
            Group = "Игры",
            Name = "Отключить Xbox Game Bar",
            Description = "AutoGameModeEnabled / ShowStartupPanel",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"SOFTWARE\Microsoft\GameBar",
            ValueName = "ShowStartupPanel",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "game-dvr-policy",
            Group = "Игры",
            Name = "Запретить Game DVR политикой",
            Description = "HKLM AllowGameDVR = 0",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SOFTWARE\Policies\Microsoft\Windows\GameDVR",
            ValueName = "AllowGameDVR",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "app-capture",
            Group = "Игры",
            Name = "Отключить запись экрана игр",
            Description = "AppCaptureEnabled = 0",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR",
            ValueName = "AppCaptureEnabled",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "system-responsiveness",
            Group = "Игры",
            Name = "Приоритет мультимедиа для игр",
            Description = "SystemResponsiveness = 10 (меньше резерва CPU под фоновые задачи)",
            IsRecommended = true,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
            ValueName = "SystemResponsiveness",
            EnabledValue = 10,
            DisabledValue = 20
        },
        new()
        {
            Id = "network-throttling",
            Group = "Игры",
            Name = "Отключить троттлинг сети Multimedia",
            Description = "NetworkThrottlingIndex = 0xFFFFFFFF",
            IsRecommended = true,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
            ValueName = "NetworkThrottlingIndex",
            EnabledValue = unchecked((int)0xFFFFFFFF),
            DisabledValue = 10
        }
    ];

    public IReadOnlyList<OptimizationToggle> GetToggles() =>
        Specs.Select(RegistryToggleHelper.ToToggle).ToList();

    public Task<OperationResult> ApplyAsync(IReadOnlyList<OptimizationToggle> toggles, CancellationToken cancellationToken = default) =>
        Task.Run(() => PerformanceOptimizer.ApplySpecs(Specs, toggles, cancellationToken), cancellationToken);
}

public sealed class NetworkOptimizer : IOptimizer
{
    public string Id => "network";
    public string DisplayName => "Сеть";

    private static readonly RegistryToggleSpec[] Specs =
    [
        new()
        {
            Id = "qos-reserve",
            Group = "Сеть",
            Name = "Снять резерв QoS (20%)",
            Description = "NonBestEffortLimit = 0 — не резервировать канал для QoS",
            IsRecommended = true,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SOFTWARE\Policies\Microsoft\Windows\Psched",
            ValueName = "NonBestEffortLimit",
            EnabledValue = 0,
            DisabledValue = 20
        },
        new()
        {
            Id = "delivery-opt-off",
            Group = "Сеть",
            Name = "Отключить раздачу обновлений (P2P)",
            Description = "Delivery Optimization DODownloadMode = 0",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization",
            ValueName = "DODownloadMode",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "onedrive-policy",
            Group = "Сеть",
            Name = "Запретить OneDrive (политика)",
            Description = "DisableFileSyncNGSC — не удаляет клиент, только блокирует синхронизацию",
            IsRecommended = false,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SOFTWARE\Policies\Microsoft\Windows\OneDrive",
            ValueName = "DisableFileSyncNGSC",
            EnabledValue = 1,
            DisabledValue = 0
        }
    ];

    public IReadOnlyList<OptimizationToggle> GetToggles() =>
        Specs.Select(RegistryToggleHelper.ToToggle).ToList();

    public Task<OperationResult> ApplyAsync(IReadOnlyList<OptimizationToggle> toggles, CancellationToken cancellationToken = default) =>
        Task.Run(() => PerformanceOptimizer.ApplySpecs(Specs, toggles, cancellationToken), cancellationToken);
}

public sealed class SystemOptimizer : IOptimizer
{
    public string Id => "system";
    public string DisplayName => "Система";

    private const string HighPerfGuid = "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c";

    private static readonly RegistryToggleSpec[] Specs =
    [
        new()
        {
            Id = "ntfs-last-access",
            Group = "Система",
            Name = "Не обновлять Last Access NTFS",
            Description = "NtfsDisableLastAccessUpdate = 1 — меньше записи на диск",
            IsRecommended = true,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SYSTEM\CurrentControlSet\Control\FileSystem",
            ValueName = "NtfsDisableLastAccessUpdate",
            EnabledValue = 1,
            DisabledValue = 0
        },
        new()
        {
            Id = "fast-startup",
            Group = "Система",
            Name = "Отключить быстрый запуск",
            Description = "HiberbootEnabled = 0 — чище завершение работы, меньше проблем с драйверами",
            IsRecommended = false,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SYSTEM\CurrentControlSet\Control\Session Manager\Power",
            ValueName = "HiberbootEnabled",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "power-throttling",
            Group = "Система",
            Name = "Отключить Power Throttling",
            Description = "PowerThrottlingOff = 1 — не душить фоновые процессы на десктопе",
            IsRecommended = true,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling",
            ValueName = "PowerThrottlingOff",
            EnabledValue = 1,
            DisabledValue = 0
        },
        new()
        {
            Id = "sysmain-service",
            Group = "Система",
            Name = "Отключить SysMain (Superfetch)",
            Description = "Полезно на SSD: служба перестаёт префетчить в фоне",
            IsRecommended = true,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SYSTEM\CurrentControlSet\Services\SysMain",
            ValueName = "Start",
            EnabledValue = 4,
            DisabledValue = 3
        },
        new()
        {
            Id = "long-paths",
            Group = "Система",
            Name = "Разрешить длинные пути (>260 символов)",
            Description = "LongPathsEnabled = 1 (Sophia / winutil)",
            IsRecommended = true,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SYSTEM\CurrentControlSet\Control\FileSystem",
            ValueName = "LongPathsEnabled",
            EnabledValue = 1,
            DisabledValue = 0
        },
        new()
        {
            Id = "linked-connections",
            Group = "Система",
            Name = "Сетевые диски в программах от администратора",
            Description = "EnableLinkedConnections = 1",
            IsRecommended = false,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System",
            ValueName = "EnableLinkedConnections",
            EnabledValue = 1,
            DisabledValue = 0
        }
    ];

    public IReadOnlyList<OptimizationToggle> GetToggles()
    {
        var toggles = Specs.Select(RegistryToggleHelper.ToToggle).ToList();
        toggles.Add(new OptimizationToggle
        {
            Id = "high-performance-plan",
            Group = "Система",
            Name = "Схема питания «Высокая производительность»",
            Description = "powercfg /setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c",
            IsRecommended = true,
            RequiresRestart = false,
            IsEnabled = IsHighPerformance()
        });
        toggles.Add(new OptimizationToggle
        {
            Id = "hibernate-off",
            Group = "Система",
            Name = "Отключить гибернацию",
            Description = "Удаляет hiberfil.sys и освобождает место на системном диске",
            IsRecommended = false,
            RequiresRestart = true,
            IsEnabled = !HibernateFileExists()
        });
        return toggles;
    }

    public Task<OperationResult> ApplyAsync(IReadOnlyList<OptimizationToggle> toggles, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var result = PerformanceOptimizer.ApplySpecs(Specs, toggles, cancellationToken);
            var messages = result.Messages.ToList();
            var fixedCount = result.FixedCount;
            var failed = result.FailedCount;

            foreach (var toggle in toggles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    if (toggle.Id == "high-performance-plan")
                    {
                        RunPowerCfg(toggle.IsEnabled ? $"/setactive {HighPerfGuid}" : "/setactive SCHEME_BALANCED");
                        fixedCount++;
                    }
                    else if (toggle.Id == "hibernate-off")
                    {
                        RunPowerCfg(toggle.IsEnabled ? "/hibernate off" : "/hibernate on");
                        fixedCount++;
                    }
                    else if (toggle.Id == "sysmain-service")
                    {
                        ApplySysMain(toggle.IsEnabled);
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    messages.Add($"{toggle.Name}: {ex.Message}");
                }
            }

            return new OperationResult
            {
                Success = failed == 0,
                FixedCount = fixedCount,
                FailedCount = failed,
                Messages = messages
            };
        }, cancellationToken);
    }

    private static void ApplySysMain(bool disable)
    {
        try
        {
            using var sc = new ServiceController("SysMain");
            if (disable)
            {
                if (sc.Status != ServiceControllerStatus.Stopped && sc.Status != ServiceControllerStatus.StopPending)
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(15));
                }
            }
        }
        catch
        {
            // service may already be stopped or missing
        }
    }

    private static bool IsHighPerformance()
    {
        var output = RunPowerCfg("/getactivescheme");
        return output.Contains(HighPerfGuid, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HibernateFileExists()
    {
        var root = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
        return File.Exists(Path.Combine(root, "hiberfil.sys"));
    }

    private static string RunPowerCfg(string args)
    {
        var start = new ProcessStartInfo
        {
            FileName = "powercfg.exe",
            Arguments = args,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        using var process = Process.Start(start);
        if (process is null)
        {
            return string.Empty;
        }

        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit(10_000);
        return output;
    }
}
