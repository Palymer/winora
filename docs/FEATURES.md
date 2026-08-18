# Функции Winora

## Очистка диска

| Категория | Что удаляется |
|---|---|
| Временные файлы | `%TEMP%`, `Windows\Temp` |
| Корзина | файлы `$Recycle.Bin` на всех дисках |
| Битые ярлыки | `.lnk` без существующей цели |
| Журналы и дампы | WER, minidump, CBS, CrashDumps |
| Кэш эскизов | `thumbcache_*.db` |
| Prefetch | `*.pf` (первый запуск программ станет дольше) |
| Кэш браузеров | Chrome, Edge, Firefox, INetCache (без паролей) |
| Кэш обновлений | SoftwareDistribution\Download, Delivery Optimization |
| Недавние файлы | Recent и Jump Lists |
| Кэш шейдеров | DirectX, NVIDIA, AMD |
| Кэш значков | IconCache.db |

## Ремонт реестра

- Записи удаления программ с несуществующим путём
- Автозагрузка Run/RunOnce на пропавшие exe
- App Paths
- SharedDLLs

## Оптимизация

**Производительность:** визуальные эффекты, задержка меню, задержка автозагрузки.

**Конфиденциальность:** телеметрия, рекламный ID, Cortana, Copilot, Recall, геолокация, журнал действий, советы, речь онлайн, ввод, Wi‑Fi Sense, Office.

**Проводник:** расширения файлов, «Этот компьютер», прозрачность, виджеты, Bing, классическое меню ПКМ, Task View, Teams, Spotlight.

**Ввод:** ускорение мыши, залипание клавиш, автозапуск, NumLock.

**Edge:** Startup Boost, фон, сайдбар, первый запуск, диагностика.

**Игры:** Game Bar, Game DVR, SystemResponsiveness, NetworkThrottlingIndex.

**Сеть:** QoS, P2P Delivery Optimization, политика OneDrive.

**Службы:** DiagTrack, dmwappush, Remote Registry, Retail Demo, SMBv1, задачи CEIP.

**Система:** NTFS last access, быстрый запуск, Power Throttling, SysMain, длинные пути, схема питания, гибернация.
