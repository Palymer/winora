# Winora — оптимизация Windows 10 и Windows 11, очистка диска и ремонт реестра

**Winora** — бесплатная настольная программа для **оптимизации Windows**, **ускорения компьютера**, **очистки диска от мусора** и **ремонта реестра**. Подходит для Windows 10 и Windows 11: домашние ПК, игровые сборки и рабочие станции после чистой установки.

Ищите по запросам: Winora, оптимизатор Windows, очистка реестра Windows 11, очистка временных файлов, удаление мусора с диска, ускорение Windows 10, отключение телеметрии, ремонт битых ярлыков, аналог CCleaner на русском, privacy tweaks, Windows registry cleaner.

**GitHub topics:** `winora` `windows` `windows-10` `windows-11` `optimizer` `disk-cleanup` `registry-cleaner` `privacy` `telemetry` `debloat` `csharp` `wpf` `dotnet` `performance` `cleaner` `portable` `system-utilities` `recycle-bin` `tweaks` `open-source`

---

## Зачем нужен оптимизатор Windows

После установки Windows 10 или Windows 11 в фоне остаются телеметрия, подсказки, кэш браузеров, очередь обновлений, записи удалённых программ в реестре и ярлыки на несуществующие файлы. Это занимает место на SSD, увеличивает время запуска и добавляет фоновую нагрузку.

Winora собирает в одном окне то, что обычно делают вручную через Параметры, `regedit`, очистку диска и сторонние твикеры:

1. **Очистка Windows** — temp, корзина, кэш Chrome / Edge / Firefox, Delivery Optimization, логи, дампы, шейдеры GPU, кэш значков, Prefetch, недавние документы.
2. **Ремонт реестра** — битые Uninstall, автозагрузка, App Paths, SharedDLLs; перед правками создаётся `.reg`-бэкап и точка восстановления.
3. **Оптимизация и твики** — производительность, конфиденциальность, проводник, мышь, Microsoft Edge, игры, сеть, службы DiagTrack, схема питания.

Сначала сканирование и предпросмотр, затем вы сами отмечаете, что удалять или включать. Winora **не отключает защитник Windows** и **не выключает Windows Update**.

## Кому подходит

- Нужно **освободить место на диске C** без удаления личных файлов
- Система «тормозит» после года работы: кэш, автозагрузка, визуальные эффекты
- Хочется **отключить телеметрию, Copilot, Cortana, рекламу в Параметрах**
- После удаления программ остались **битые ярлыки** и пункты в «Программы и компоненты»
- Игровой ПК: Game DVR, ускорение мыши, NetworkThrottling, SysMain на SSD

## Возможности

Полный перечень — в [списке функций](docs/FEATURES.md). Кратко:

| Раздел | Что индексируется поиском |
|---|---|
| Очистка диска | временные файлы Windows, корзина `$Recycle.Bin`, кэш браузера, Windows Update, DirectX shader cache |
| Ярлыки | поиск битых `.lnk` на рабочем столе, в меню Пуск и на панели задач |
| Реестр | registry repair, leftover uninstall keys, startup Run keys |
| Приватность | telemetry, Advertising ID, Recall, геолокация, Wi‑Fi Sense, Office logging |
| Интерфейс | классическое контекстное меню Windows 11, виджеты, Bing в поиске, Spotlight |
| Игры и ввод | Game Bar, отключение ускорения мыши, залипание клавиш |
| Службы | DiagTrack, Remote Registry, SMBv1, задачи CEIP |

## Документация

- [Руководство пользователя](docs/USER-GUIDE.md) — как сканировать, чистить и откатывать изменения
- [Список функций](docs/FEATURES.md) — все категории очистки и твиков
- [Безопасность](docs/SAFETY.md) — бэкапы, что не стоит включать без необходимости
- [О проекте и ключевые слова](docs/ABOUT.md) — развёрнутое описание для каталогов и поиска
- [История версий](docs/CHANGELOG.md)

## Скачать Winora

Актуальная сборка: [релизы GitHub](https://github.com/Palymer/winora/releases) — **0.1.0 Alpha**, portable `Winora.exe` (win-x64, .NET внутри, установка не нужна).

Запуск из исходников:

```powershell
dotnet run --project src/WindowsOptimizer.App
```

Сборка:

```powershell
dotnet publish src/WindowsOptimizer.App -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o dist/Winora
```

При старте Windows запросит права администратора (UAC) — они нужны для очистки системных папок и HKLM.

Бэкапы реестра: `%LocalAppData%\Winora\Backups`  
Журналы: `%LocalAppData%\Winora\Logs`

## Часто задаваемые вопросы

**Winora — это CCleaner?**  
Нет. Это отдельный открытый оптимизатор Windows: очистка мусора, ремонт реестра и твики производительности/приватности с предпросмотром.

**Работает на Windows 11 24H2 / 25H2?**  
Да, рассчитана на Windows 10 и Windows 11. Часть политик (Copilot, Recall, виджеты) имеет смысл именно на Windows 11.

**Удаляет ли Winora личные фото и документы?**  
Нет. Чистятся временные каталоги, кэш, корзина (если вы её выбрали) и битые ссылки. Куки и пароли браузеров не трогаются.

**Можно ли вернуть твики?**  
Да. Тумблеры оптимизации обратимы. Для реестра сохраняется `.reg`. Рекомендуется точка восстановления.

**Нужен ли интернет?**  
Нет. Winora работает локально, без облака и без обязательной регистрации.

## English summary

**Winora** is a Windows 10/11 optimizer, disk cleaner and registry repair tool. It scans junk files (temp, Recycle Bin, browser caches, Windows Update leftovers, GPU shader cache, broken shortcuts), fixes leftover registry uninstall/startup entries, and applies reversible performance and privacy tweaks (telemetry, Copilot, Edge, Game DVR, SysMain). Preview first, then apply. No Defender or Windows Update kill-switch. Portable `Winora.exe` for win-x64.

Keywords: `windows-optimizer` `disk-cleaner` `registry-cleaner` `windows-11` `windows-10` `privacy` `debloat` `performance-tweaks` `telemetry` `recycle-bin` `broken-shortcuts` `wpf` `dotnet`

## Релиз

Тег `v0.1.0-alpha` собирает portable exe в GitHub Actions и публикует GitHub Release (prerelease). Следующий релиз: обновить версию в `Directory.Build.props`, запушить тег `vX.Y.Z` или `vX.Y.Z-alpha`.

## Исходный код

```
src/WindowsOptimizer.Core            модели
src/WindowsOptimizer.Infrastructure  очистка, реестр, твики
src/WindowsOptimizer.App             интерфейс Winora (WPF)
tests/WindowsOptimizer.Tests
docs                                 документация
```
