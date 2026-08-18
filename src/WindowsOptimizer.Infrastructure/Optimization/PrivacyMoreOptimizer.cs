using Microsoft.Win32;
using WindowsOptimizer.Core.Interfaces;
using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.Infrastructure.Optimization;

public sealed class PrivacyMoreOptimizer : IOptimizer
{
    public string Id => "privacy-more";
    public string DisplayName => "Конфиденциальность";

    private static readonly RegistryToggleSpec[] Specs =
    [
        new()
        {
            Id = "online-speech",
            Group = "Конфиденциальность",
            Name = "Отключить распознавание речи онлайн",
            Description = "HasAccepted = 0 (Win11Debloat Disable_Telemetry.reg)",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\Speech_OneCore\Settings\OnlineSpeechPrivacy",
            ValueName = "HasAccepted",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "inking",
            Group = "Конфиденциальность",
            Name = "Не отправлять рукописный ввод",
            Description = "RestrictImplicitInkCollection = 1",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\InputPersonalization",
            ValueName = "RestrictImplicitInkCollection",
            EnabledValue = 1,
            DisabledValue = 0
        },
        new()
        {
            Id = "typing-data",
            Group = "Конфиденциальность",
            Name = "Не отправлять данные ввода текста",
            Description = "RestrictImplicitTextCollection = 1",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\InputPersonalization",
            ValueName = "RestrictImplicitTextCollection",
            EnabledValue = 1,
            DisabledValue = 0
        },
        new()
        {
            Id = "feedback-never",
            Group = "Конфиденциальность",
            Name = "Не запрашивать отзывы",
            Description = "NumberOfSIUFInPeriod = 0",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"SOFTWARE\Microsoft\Siuf\Rules",
            ValueName = "NumberOfSIUFInPeriod",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "start-track-progs",
            Group = "Конфиденциальность",
            Name = "Не отслеживать запускаемые программы",
            Description = "Start_TrackProgs = 0",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            ValueName = "Start_TrackProgs",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "find-my-device",
            Group = "Конфиденциальность",
            Name = "Отключить «Найти устройство»",
            Description = "AllowFindMyDevice = 0",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SOFTWARE\Policies\Microsoft\FindMyDevice",
            ValueName = "AllowFindMyDevice",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "wifi-sense",
            Group = "Конфиденциальность",
            Name = "Отключить Wi‑Fi Sense",
            Description = "AutoConnectAllowedOEM = 0",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SOFTWARE\Microsoft\WcmSvc\wifinetworkmanager\config",
            ValueName = "AutoConnectAllowedOEM",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "settings-ads",
            Group = "Конфиденциальность",
            Name = "Убрать рекламу в Параметрах",
            Description = "SubscribedContent-338393Enabled = 0",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            ValueName = "SubscribedContent-338393Enabled",
            EnabledValue = 0,
            DisabledValue = 1
        },
        new()
        {
            Id = "office-telemetry",
            Group = "Конфиденциальность",
            Name = "Отключить телеметрию Office",
            Description = "Enablelogging = 0 (если установлен Microsoft Office)",
            IsRecommended = true,
            RequiresRestart = false,
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Policies\Microsoft\office\16.0\osm",
            ValueName = "Enablelogging",
            EnabledValue = 0,
            DisabledValue = 1
        }
    ];

    public IReadOnlyList<OptimizationToggle> GetToggles() =>
        Specs.Select(RegistryToggleHelper.ToToggle).ToList();

    public Task<OperationResult> ApplyAsync(IReadOnlyList<OptimizationToggle> toggles, CancellationToken cancellationToken = default) =>
        Task.Run(() => PerformanceOptimizer.ApplySpecs(Specs, toggles, cancellationToken), cancellationToken);
}
