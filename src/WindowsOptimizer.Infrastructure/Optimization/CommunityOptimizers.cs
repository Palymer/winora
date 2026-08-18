using System.Diagnostics;
using System.ServiceProcess;
using Microsoft.Win32;
using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.Infrastructure.Optimization;

public sealed class ExplorerUiOptimizer : IOptimizer
{
    public string Id => "explorer-ui";
    public string DisplayName => "Проводник";

    private const string ClassicMenuKey = @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32";

    private static readonly RegistryToggleSpec[] Specs =
    [
        new()
        {
            Id = "task-view",
            Group = "Проводник",
            Name = "Скрыть «Представление задач»",
            Description = "ShowTaskViewButton = 0 (Win11Debloat / winutil)",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            ValueName = "ShowTaskViewButton",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "taskbar-chat",
            Group = "Проводник",
            Name = "Скрыть чат Teams на панели задач",
            Description = "TaskbarMn = 0",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            ValueName = "TaskbarMn",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "snap-flyout",
            Group = "Проводник",
            Name = "Отключить подсказки Snap Layouts",
            Description = "EnableSnapAssistFlyout = 0",
            IsRecommended = false,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            ValueName = "EnableSnapAssistFlyout",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "show-hidden",
            Group = "Проводник",
            Name = "Показывать скрытые файлы",
            Description = "Hidden = 1",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            ValueName = "Hidden",
            EnabledValue = 1,
            DisabledValue = 2
        },
        new()
        {
            Id = "sync-provider",
            Group = "Проводник",
            Name = "Скрыть подсказки OneDrive в проводнике",
            Description = "ShowSyncProviderNotifications = 0",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            ValueName = "ShowSyncProviderNotifications",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "search-box-suggestions",
            Group = "Проводник",
            Name = "Отключить подсказки в поле поиска",
            Description = "DisableSearchBoxSuggestions (политика)",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Policies\Microsoft\Windows\Explorer",
            ValueName = "DisableSearchBoxSuggestions",
            EnabledValue = 1,
            DisabledValue = 0
        },
        new()
        {
            Id = "lock-spotlight",
            Group = "Проводник",
            Name = "Отключить Windows Spotlight на экране блокировки",
            Description = "RotatingLockScreenEnabled = 0",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            ValueName = "RotatingLockScreenEnabled",
            EnabledValue = 0,
            DisabledValue = 1
        }
    ];

    public IReadOnlyList<OptimizationToggle> GetToggles()
    {
        var list = Specs.Select(RegistryToggleHelper.ToToggle).ToList();
        list.Add(new OptimizationToggle
        {
            Id = "classic-context-menu",
            Group = "Проводник",
            Name = "Классическое контекстное меню Windows 11",
            Description = "Возвращает полное меню ПКМ без «Показать дополнительные параметры»",
            IsRecommended = true,
            RequiresRestart = false,
            IsEnabled = IsClassicMenuEnabled()
        });
        return list;
    }

    public Task<OperationResult> ApplyAsync(IReadOnlyList<OptimizationToggle> toggles, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var result = PerformanceOptimizer.ApplySpecs(Specs, toggles, cancellationToken);
            var messages = result.Messages.ToList();
            var fixedCount = result.FixedCount;
            var failed = result.FailedCount;
            var classic = toggles.FirstOrDefault(t => t.Id == "classic-context-menu");
            if (classic is not null)
            {
                try
                {
                    SetClassicMenu(classic.IsEnabled);
                    fixedCount++;
                }
                catch (Exception ex)
                {
                    failed++;
                    messages.Add($"{classic.Name}: {ex.Message}");
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

    private static bool IsClassicMenuEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(ClassicMenuKey);
        return key is not null;
    }

    private static void SetClassicMenu(bool enabled)
    {
        const string parent = @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}";
        if (enabled)
        {
            using var key = Registry.CurrentUser.CreateSubKey(ClassicMenuKey);
            key?.SetValue(null, "", RegistryValueKind.String);
        }
        else
        {
            Registry.CurrentUser.DeleteSubKeyTree(parent, throwOnMissingSubKey: false);
        }
    }
}

public sealed class InputOptimizer : IOptimizer
{
    public string Id => "input";
    public string DisplayName => "Ввод";

    private static readonly RegistryToggleSpec[] Specs =
    [
        new()
        {
            Id = "mouse-accel",
            Group = "Ввод",
            Name = "Отключить ускорение мыши",
            Description = "MouseSpeed/Threshold = 0 (winutil, полезно в играх)",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Control Panel\Mouse",
            ValueName = "MouseSpeed",
            EnabledValue = "0",
            DisabledValue = "1",
            Kind = RegistryValueKind.String
        },
        new()
        {
            Id = "mouse-threshold1",
            Group = "Ввод",
            Name = "Порог ускорения мыши 1",
            Description = "MouseThreshold1 = 0",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Control Panel\Mouse",
            ValueName = "MouseThreshold1",
            EnabledValue = "0",
            DisabledValue = "6",
            Kind = RegistryValueKind.String
        },
        new()
        {
            Id = "mouse-threshold2",
            Group = "Ввод",
            Name = "Порог ускорения мыши 2",
            Description = "MouseThreshold2 = 0",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Control Panel\Mouse",
            ValueName = "MouseThreshold2",
            EnabledValue = "0",
            DisabledValue = "10",
            Kind = RegistryValueKind.String
        },
        new()
        {
            Id = "sticky-keys",
            Group = "Ввод",
            Name = "Отключить залипание клавиш",
            Description = "StickyKeys Flags = 506 (Sophia Script)",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Control Panel\Accessibility\StickyKeys",
            ValueName = "Flags",
            EnabledValue = "506",
            DisabledValue = "510",
            Kind = RegistryValueKind.String
        },
        new()
        {
            Id = "autoplay",
            Group = "Ввод",
            Name = "Отключить автозапуск носителей",
            Description = "DisableAutoplay = 1",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers",
            ValueName = "DisableAutoplay",
            EnabledValue = 1,
            DisabledValue = 0
        },
        new()
        {
            Id = "numlock-boot",
            Group = "Ввод",
            Name = "NumLock при входе",
            Description = "InitialKeyboardIndicators = 2 для .DEFAULT",
            IsRecommended = false,
            RequiresRestart = true,
            Hive = RegistryHive.Users,
            KeyPath = @".DEFAULT\Control Panel\Keyboard",
            ValueName = "InitialKeyboardIndicators",
            EnabledValue = "2",
            DisabledValue = "0",
            Kind = RegistryValueKind.String
        }
    ];

    public IReadOnlyList<OptimizationToggle> GetToggles() =>
        Specs.Select(RegistryToggleHelper.ToToggle).ToList();

    public Task<OperationResult> ApplyAsync(IReadOnlyList<OptimizationToggle> toggles, CancellationToken cancellationToken = default) =>
        Task.Run(() => PerformanceOptimizer.ApplySpecs(Specs, toggles, cancellationToken), cancellationToken);
}

public sealed class EdgeOptimizer : IOptimizer
{
    public string Id => "edge";
    public string DisplayName => "Microsoft Edge";

    private static readonly RegistryToggleSpec[] Specs =
    [
        new()
        {
            Id = "edge-boost",
            Group = "Microsoft Edge",
            Name = "Отключить Startup Boost",
            Description = "Edge не держит процессы после закрытия",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SOFTWARE\Policies\Microsoft\Edge",
            ValueName = "StartupBoostEnabled",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "edge-background",
            Group = "Microsoft Edge",
            Name = "Запретить фоновый режим Edge",
            Description = "BackgroundModeEnabled = 0",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SOFTWARE\Policies\Microsoft\Edge",
            ValueName = "BackgroundModeEnabled",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "edge-sidebar",
            Group = "Microsoft Edge",
            Name = "Скрыть боковую панель Edge",
            Description = "HubsSidebarEnabled = 0",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SOFTWARE\Policies\Microsoft\Edge",
            ValueName = "HubsSidebarEnabled",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "edge-first-run",
            Group = "Microsoft Edge",
            Name = "Пропустить первый запуск Edge",
            Description = "HideFirstRunExperience = 1",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SOFTWARE\Policies\Microsoft\Edge",
            ValueName = "HideFirstRunExperience",
            EnabledValue = 1,
            DisabledValue = 0
        },
        new()
        {
            Id = "edge-diagnostics",
            Group = "Microsoft Edge",
            Name = "Отключить диагностику Edge",
            Description = "DiagnosticData = 0 (Win11Debloat)",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SOFTWARE\Policies\Microsoft\Edge",
            ValueName = "DiagnosticData",
            EnabledValue = 0,
            DisabledValue = 1
        }
    ];

    public IReadOnlyList<OptimizationToggle> GetToggles() =>
        Specs.Select(RegistryToggleHelper.ToToggle).ToList();

    public Task<OperationResult> ApplyAsync(IReadOnlyList<OptimizationToggle> toggles, CancellationToken cancellationToken = default) =>
        Task.Run(() => PerformanceOptimizer.ApplySpecs(Specs, toggles, cancellationToken), cancellationToken);
}

public sealed class ServicesOptimizer : IOptimizer
{
    public string Id => "services";
    public string DisplayName => "Службы";

    private static readonly RegistryToggleSpec[] Specs =
    [
        new()
        {
            Id = "diagtrack",
            Group = "Службы",
            Name = "Отключить DiagTrack (телеметрия)",
            Description = "Connected User Experiences and Telemetry",
            IsRecommended = true,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SYSTEM\CurrentControlSet\Services\DiagTrack",
            ValueName = "Start",
            EnabledValue = 4,
            DisabledValue = 2
        },
        new()
        {
            Id = "dmwappush",
            Group = "Службы",
            Name = "Отключить dmwappushservice",
            Description = "WAP Push — связан с телеметрией",
            IsRecommended = true,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SYSTEM\CurrentControlSet\Services\dmwappushservice",
            ValueName = "Start",
            EnabledValue = 4,
            DisabledValue = 3
        },
        new()
        {
            Id = "remote-registry",
            Group = "Службы",
            Name = "Отключить Remote Registry",
            Description = "Удаленный доступ к реестру обычно не нужен",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SYSTEM\CurrentControlSet\Services\RemoteRegistry",
            ValueName = "Start",
            EnabledValue = 4,
            DisabledValue = 3
        },
        new()
        {
            Id = "retail-demo",
            Group = "Службы",
            Name = "Отключить Retail Demo",
            Description = "Демо-режим для магазинных ПК",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SYSTEM\CurrentControlSet\Services\RetailDemo",
            ValueName = "Start",
            EnabledValue = 4,
            DisabledValue = 3
        },
        new()
        {
            Id = "smb1",
            Group = "Службы",
            Name = "Отключить SMBv1",
            Description = "Устаревший и небезопасный протокол (Sophia / Debloat-Windows-10)",
            IsRecommended = true,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters",
            ValueName = "SMB1",
            EnabledValue = 0,
            DisabledValue = 1
        }
    ];

    public IReadOnlyList<OptimizationToggle> GetToggles() =>
        Specs.Select(RegistryToggleHelper.ToToggle).ToList();

    public Task<OperationResult> ApplyAsync(IReadOnlyList<OptimizationToggle> toggles, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var result = PerformanceOptimizer.ApplySpecs(Specs, toggles, cancellationToken);
            foreach (var toggle in toggles.Where(t => t.IsEnabled && t.Id is "diagtrack" or "dmwappush" or "remote-registry" or "retail-demo"))
            {
                var name = toggle.Id switch
                {
                    "diagtrack" => "DiagTrack",
                    "dmwappush" => "dmwappushservice",
                    "remote-registry" => "RemoteRegistry",
                    "retail-demo" => "RetailDemo",
                    _ => null
                };
                if (name is not null)
                {
                    TryStopService(name);
                }
            }

            return result;
        }, cancellationToken);
    }

    private static void TryStopService(string name)
    {
        try
        {
            using var sc = new ServiceController(name);
            if (sc.Status is not ServiceControllerStatus.Stopped and not ServiceControllerStatus.StopPending)
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(12));
            }
        }
        catch
        {
            // service may be missing
        }
    }
}

public sealed class TelemetryTasksOptimizer : IOptimizer
{
    public string Id => "telemetry-tasks";
    public string DisplayName => "Задачи телеметрии";

    private static readonly string[] Tasks =
    [
        @"\Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser",
        @"\Microsoft\Windows\Application Experience\ProgramDataUpdater",
        @"\Microsoft\Windows\Autochk\Proxy",
        @"\Microsoft\Windows\Customer Experience Improvement Program\Consolidator",
        @"\Microsoft\Windows\Customer Experience Improvement Program\UsbCeip",
        @"\Microsoft\Windows\DiskDiagnostic\Microsoft-Windows-DiskDiagnosticDataCollector",
        @"\Microsoft\Windows\Feedback\Siuf\DmClient",
        @"\Microsoft\Windows\Feedback\Siuf\DmClientOnScenarioDownload",
        @"\Microsoft\Windows\Windows Error Reporting\QueueReporting"
    ];

    public IReadOnlyList<OptimizationToggle> GetToggles() =>
    [
        new()
        {
            Id = "ceip-tasks",
            Group = "Службы",
            Name = "Отключить задачи CEIP / Compatibility Appraiser",
            Description = "Планировщик: Consolidator, UsbCeip, Compatibility Appraiser (Windows10Debloater / Sophia)",
            IsRecommended = true,
            RequiresRestart = false,
            IsEnabled = AreTasksDisabled()
        }
    ];

    public Task<OperationResult> ApplyAsync(IReadOnlyList<OptimizationToggle> toggles, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var toggle = toggles.FirstOrDefault(t => t.Id == "ceip-tasks");
            if (toggle is null)
            {
                return OperationResult.Empty("Нет изменений");
            }

            var action = toggle.IsEnabled ? "/Disable" : "/Enable";
            var ok = 0;
            var failed = 0;
            foreach (var task in Tasks)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var code = RunSchTasks($"/Change /TN \"{task}\" {action}");
                if (code == 0)
                {
                    ok++;
                }
                else
                {
                    failed++;
                }
            }

            return new OperationResult
            {
                Success = failed == 0 || ok > 0,
                FixedCount = ok,
                FailedCount = failed,
                Messages = failed == 0
                    ? Array.Empty<string>()
                    : new[] { $"Часть задач недоступна на этой редакции Windows ({failed})." }
            };
        }, cancellationToken);
    }

    private static bool AreTasksDisabled()
    {
        var output = RunSchTasksQuery(@"\Microsoft\Windows\Customer Experience Improvement Program\Consolidator");
        return output.Contains("Disabled", StringComparison.OrdinalIgnoreCase) ||
               output.Contains("Отключена", StringComparison.OrdinalIgnoreCase);
    }

    private static int RunSchTasks(string args)
    {
        var start = new ProcessStartInfo
        {
            FileName = "schtasks.exe",
            Arguments = args,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        using var process = Process.Start(start);
        if (process is null)
        {
            return -1;
        }

        process.WaitForExit(8_000);
        return process.ExitCode;
    }

    private static string RunSchTasksQuery(string task)
    {
        var start = new ProcessStartInfo
        {
            FileName = "schtasks.exe",
            Arguments = $"/Query /TN \"{task}\" /FO LIST",
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
        process.WaitForExit(8_000);
        return output;
    }
}
