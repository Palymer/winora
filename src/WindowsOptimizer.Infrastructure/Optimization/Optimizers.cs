using Microsoft.Win32;
using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.Infrastructure.Optimization;

internal sealed class RegistryToggleSpec
{
    public required string Id { get; init; }
    public required string Group { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required bool IsRecommended { get; init; }
    public required bool RequiresRestart { get; init; }
    public required RegistryHive Hive { get; init; }
    public required string KeyPath { get; init; }
    public required string ValueName { get; init; }
    public required object EnabledValue { get; init; }
    public required object DisabledValue { get; init; }
    public RegistryValueKind Kind { get; init; } = RegistryValueKind.DWord;
}

internal static class RegistryToggleHelper
{
    public static OptimizationToggle ToToggle(RegistryToggleSpec spec)
    {
        return new OptimizationToggle
        {
            Id = spec.Id,
            Group = spec.Group,
            Name = spec.Name,
            Description = spec.Description,
            IsRecommended = spec.IsRecommended,
            RequiresRestart = spec.RequiresRestart,
            IsEnabled = IsEnabled(spec)
        };
    }

    public static bool IsEnabled(RegistryToggleSpec spec)
    {
        using var baseKey = RegistryKey.OpenBaseKey(spec.Hive, RegistryView.Registry64);
        using var key = baseKey.OpenSubKey(spec.KeyPath);
        var current = key?.GetValue(spec.ValueName);
        if (current is null)
        {
            return false;
        }

        return ValuesEqual(current, spec.EnabledValue);
    }

    public static void Apply(RegistryToggleSpec spec, bool enabled)
    {
        using var baseKey = RegistryKey.OpenBaseKey(spec.Hive, RegistryView.Registry64);
        using var key = baseKey.CreateSubKey(spec.KeyPath, writable: true)
                        ?? throw new InvalidOperationException($"Не удалось открыть {spec.KeyPath}");
        key.SetValue(spec.ValueName, enabled ? spec.EnabledValue : spec.DisabledValue, spec.Kind);
    }

    private static bool ValuesEqual(object current, object expected)
    {
        if (current is int i && expected is int ei)
        {
            return i == ei;
        }

        return string.Equals(Convert.ToString(current), Convert.ToString(expected), StringComparison.OrdinalIgnoreCase);
    }
}

public sealed class PerformanceOptimizer : IOptimizer
{
    public string Id => "performance";
    public string DisplayName => "Производительность";

    private static readonly RegistryToggleSpec[] Specs =
    [
        new()
        {
            Id = "visual-fx-performance",
            Group = "Производительность",
            Name = "Визуальные эффекты: максимальная скорость",
            Description = "Отключает анимации и тени окон ради отзывчивости системы",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects",
            ValueName = "VisualFXSetting",
            EnabledValue = 2,
            DisabledValue = 0
        },
        new()
        {
            Id = "menu-delay",
            Group = "Производительность",
            Name = "Убрать задержку меню",
            Description = "Меню Пуск и контекстные меню открываются сразу",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Control Panel\Desktop",
            ValueName = "MenuShowDelay",
            EnabledValue = "0",
            DisabledValue = "400",
            Kind = RegistryValueKind.String
        },
        new()
        {
            Id = "startup-delay",
            Group = "Производительность",
            Name = "Отключить задержку автозагрузки",
            Description = "Программы из автозагрузки стартуют без искусственной паузы",
            IsRecommended = true,
            RequiresRestart = true,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize",
            ValueName = "StartupDelayInMSec",
            EnabledValue = 0,
            DisabledValue = 10000
        }
    ];

    public IReadOnlyList<OptimizationToggle> GetToggles() =>
        Specs.Select(RegistryToggleHelper.ToToggle).ToList();

    public Task<OperationResult> ApplyAsync(
        IReadOnlyList<OptimizationToggle> toggles,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() => ApplySpecs(Specs, toggles, cancellationToken), cancellationToken);
    }

    internal static OperationResult ApplySpecs(
        IReadOnlyList<RegistryToggleSpec> specs,
        IReadOnlyList<OptimizationToggle> toggles,
        CancellationToken cancellationToken)
    {
        var map = toggles.ToDictionary(t => t.Id, StringComparer.OrdinalIgnoreCase);
        var fixedCount = 0;
        var failed = 0;
        var messages = new List<string>();

        foreach (var spec in specs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!map.TryGetValue(spec.Id, out var toggle))
            {
                continue;
            }

            try
            {
                RegistryToggleHelper.Apply(spec, toggle.IsEnabled);
                fixedCount++;
            }
            catch (Exception ex)
            {
                failed++;
                messages.Add($"{spec.Name}: {ex.Message}");
            }
        }

        return new OperationResult
        {
            Success = failed == 0,
            FixedCount = fixedCount,
            FailedCount = failed,
            Messages = messages
        };
    }
}

public sealed class PrivacyOptimizer : IOptimizer
{
    public string Id => "privacy";
    public string DisplayName => "Конфиденциальность";

    private static readonly RegistryToggleSpec[] Specs =
    [
        new()
        {
            Id = "telemetry-basic",
            Group = "Конфиденциальность",
            Name = "Ограничить телеметрию",
            Description = "Устанавливает AllowTelemetry = 1 (только данные безопасности на Pro/Home)",
            IsRecommended = true,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SOFTWARE\Policies\Microsoft\Windows\DataCollection",
            ValueName = "AllowTelemetry",
            EnabledValue = 1,
            DisabledValue = 3
        },
        new()
        {
            Id = "advertising-id",
            Group = "Конфиденциальность",
            Name = "Отключить рекламный идентификатор",
            Description = "Запрещает приложениям использовать Advertising ID",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo",
            ValueName = "Enabled",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "tailored-experiences",
            Group = "Конфиденциальность",
            Name = "Отключить персональные рекомендации",
            Description = "Tailored Experiences / диагностические подсказки",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Privacy",
            ValueName = "TailoredExperiencesWithDiagnosticDataEnabled",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "game-dvr",
            Group = "Конфиденциальность",
            Name = "Отключить Xbox Game DVR",
            Description = "Снимает фоновую запись игр и снижает нагрузку на GPU/диск",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"System\GameConfigStore",
            ValueName = "GameDVR_Enabled",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "cortana",
            Group = "Конфиденциальность",
            Name = "Отключить Cortana",
            Description = "Запрещает Cortana и связанные фоновые процессы поиска",
            IsRecommended = true,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
            ValueName = "AllowCortana",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "copilot",
            Group = "Конфиденциальность",
            Name = "Отключить Copilot",
            Description = "Убирает Copilot из панели задач и политик Windows 11",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Policies\Microsoft\Windows\WindowsCopilot",
            ValueName = "TurnOffWindowsCopilot",
            EnabledValue = 1,
            DisabledValue = 0
        },
        new()
        {
            Id = "recall",
            Group = "Конфиденциальность",
            Name = "Отключить Recall / Windows AI",
            Description = "Запрещает снимки экрана Recall (Windows 11)",
            IsRecommended = true,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI",
            ValueName = "DisableAIDataAnalysis",
            EnabledValue = 1,
            DisabledValue = 0
        },
        new()
        {
            Id = "location",
            Group = "Конфиденциальность",
            Name = "Отключить службы геолокации",
            Description = "Политика DisableLocation",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors",
            ValueName = "DisableLocation",
            EnabledValue = 1,
            DisabledValue = 0
        },
        new()
        {
            Id = "activity-history",
            Group = "Конфиденциальность",
            Name = "Отключить журнал действий",
            Description = "Не публиковать Activity History на устройство и в облако",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SOFTWARE\Policies\Microsoft\Windows\System",
            ValueName = "PublishUserActivities",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "consumer-features",
            Group = "Конфиденциальность",
            Name = "Отключить предлагаемые приложения",
            Description = "DisableWindowsConsumerFeatures — магазинные предложения в Пуске",
            IsRecommended = true,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SOFTWARE\Policies\Microsoft\Windows\CloudContent",
            ValueName = "DisableWindowsConsumerFeatures",
            EnabledValue = 1,
            DisabledValue = 0
        },
        new()
        {
            Id = "suggestions",
            Group = "Конфиденциальность",
            Name = "Отключить советы Windows",
            Description = "Content Delivery / SoftLanding",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            ValueName = "SoftLandingEnabled",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "background-apps",
            Group = "Конфиденциальность",
            Name = "Запретить фоновые приложения UWP",
            Description = "GlobalUserDisabled для BackgroundAccessApplications",
            IsRecommended = false,
            RequiresRestart = true,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications",
            ValueName = "GlobalUserDisabled",
            EnabledValue = 1,
            DisabledValue = 0
        }
    ];

    public IReadOnlyList<OptimizationToggle> GetToggles() =>
        Specs.Select(RegistryToggleHelper.ToToggle).ToList();

    public Task<OperationResult> ApplyAsync(
        IReadOnlyList<OptimizationToggle> toggles,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() => PerformanceOptimizer.ApplySpecs(Specs, toggles, cancellationToken), cancellationToken);
    }
}
