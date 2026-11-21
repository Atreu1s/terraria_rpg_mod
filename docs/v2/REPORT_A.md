# Отчёт A — каркас RPGMod v2

Дата: 21.11.2026. Основа: `CLAUDE.md`, `docs/DESIGN.md`, `docs/UI_SPEC.md`, `docs/v2/DESIGN_V2.md` (§9, задача A). Коммит не создавался.

## 1. Сверка с Calamity

Проверен исходник ветки **1.4.4**: [DamageClasses/RogueDamageClasses.cs](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/DamageClasses/RogueDamageClasses.cs).

Точное имя типа — **`CalamityMod.RogueDamageClass`**, базовый C#-тип — `DamageClass`. Имя для поиска контента — **`RogueDamageClass`**. Файл называется во множественном числе: `RogueDamageClasses.cs`.

В `GetModifierInheritance` (строки 16–22) возвращается `StatInheritanceData.Full` для `Throwing` и `Generic`, иначе `None`. `GetEffectInheritance` (24–25) возвращает true только для `Throwing`. Это наследование механик tModLoader, а не C#-наследование от типа Throwing. В том же файле `StealthDamageClass` наследует модификаторы и эффекты Rogue; поэтому бонусы назначаются найденному DamageClass, без подмены на Throwing и без проверки точного совпадения типа оружия. Никакой ссылки на сборку Calamity не добавлено.

## 2. CalamityCompat

Добавлен `Common/Systems/CalamityCompat.cs`: `Load` определяет режим, получает Mod и `DamageClass` через `TryFind`; `Unload` очищает все статические ссылки. Есть `Enabled`, `Calamity`, `Rogue`, `RogueAvailable`, `AwakeningAvailable`. Если Calamity загружен, но Rogue не найден, класс и Хитрость недоступны, в журнал записывается предупреждение.

`AwakeningDropNPC` переведён на общий Mod. Прямой `TryGetMod` остался только в разрешённых местах: CalamityCompat, BossLevels и рецепт Камня.

## 3. Пробуждение и возврат Камня

У `AwakeningEssence`, `AwakeningStone`, `AwakeningBolt`, `AwakeningDropNPC`, `AwakeningHud` добавлен `IsLoadingEnabled` через `ModLoader.HasMod`, независимо от порядка `Load`.

Все игровые эффекты `AwakeningPlayer` проходят через проверку `AwakeningAvailable`; сохранение источников эссенций остаётся. Слот пробуждения и золотое свечение узлов скрыты без Calamity. Создание эссенции и снаряда дополнительно проверяет наличие типа через `ModContent.TryFind`.

`ResetTree` при отсутствии загруженного Камня ставит `PendingStoneRefund`, не теряя право на возврат. При входе локального игрока с доступным типом Камень выдаётся один раз, флаг снимается. Немедленный возврат в `TreeResetItem` тоже использует безопасный поиск загруженного типа. Без Calamity tooltip сброса не обещает возврат Камня. Предметы, сохранённые в инвентаре, остаются на штатном механизме Unloaded Item tModLoader.

## 4. Разбойник и Хитрость

Добавлены `ClassIds.Rogue = 5`, `Count = 6`, `Available`, `AvailableClasses`, `Stat.Cunning = 6`, `StatIds.Visible`, `RPGPlayer.StatCount = 7`.

Хитрость видна после Духа; её бонусы проходят через `StatMath` / `StatBonuses` и применяются к `CalamityCompat.Rogue`: асимптоты урона 0,6 и скорости атаки 0,2. Прирост Разбойника: Хитрость 0,55, Ловкость 0,1, Выносливость 0,2. Без доступного класса урона прирост и эффекты узлов отключаются.

В реестр всегда добавлены все 15 ID в точном порядке §5: `rogue`, `assassin`, `trickster`, `ambush`, `shadow`, `juggler`, `evasion`, `executioner`, `first_blood`, `vanish`, `dusk_blade`, `ricochet`, `bravado`, `smoke_bomb`, `lucky_break`. Реализованы только простые эффекты первых трёх узлов; остальные зарегистрированы с `Apply = null`.

## 5. Сохранения и сеть

Массив `Invested` расширяется нулями до 7, старые индексы сохранены. При отсутствии доступного Разбойника вложение в Хитрость обнуляется; свободные очки вычисляются как общий бюджет минус вложения, поэтому автоматически возвращаются. Повторная загрузка повторно очки не добавляет.

Разбойник, его выбранные узлы и `Awakened` сохраняются при отключении Calamity. Другой класс нельзя выбрать до сброса. Неизвестный ClassId по-прежнему сбрасывается.

`PendingStoneRefund` сохраняется в TagCompound и участвует в записи/чтении сетевого состояния, `CopyClientState`, `SendClientChanges`. Сетевые циклы передают 7 статов. В проекте ModPlayer синхронизируется через `WriteSync` / `ReceiveSync` / `SyncPlayer`, а не через методы с именами NetSend/NetReceive; расширен именно используемый протокол. Формат предмета-эссенции не менялся.

## 6. UI

Ряд выбора использует `AvailableClasses` и вычисляет ширину и gap по количеству. Панель статов строится из `StatIds.Visible`; шаг ограничен доступной высотой. Семь строк при текущих размерах заканчиваются на y=626 внутри панели высотой 892, поэтому уменьшать штатный шаг 74 px не требуется.

Добавлена плашка неактивного Разбойника над древом в `Theme.Accent`, цвет класса `#D46BB0`, ключ иконки `stat_cun`. Иконка класса использует существующий `Icons.Skill` / `Theme.IconOrLetter` с буквенной заглушкой.

Создание окна перенесено из `CharacterUISystem.Load` в `PostSetupContent`, чтобы список статов строился после `CalamityCompat.Load`, независимо от порядка загрузки систем.

## 7. Конфиг

В ServerSide RPGConfig добавлено 13 полей: два коэффициента Хитрости, три коэффициента прироста, два бонуса корня, три параметра Убийцы и три параметра Ловкача. Штрафы тоже настраиваемые. Общая константа убывающей отдачи использует существующий `SoftCapK`.

## 8. Локализация

Оба HJSON дополнены классом и приростом, Хитростью и её эффектами, всеми 15 узлами (Name/Effect), пробуждением Разбойника (Name/Effect/Bonus), плашкой, tooltip без Calamity, выводом режима и подписями новых настроек. Русские тексты узлов и пробуждения взяты из §5–6 ТЗ. Новая механика пробуждения не реализована.

## 9. Отладка

Добавлено `/rpg mode`: `calamity on/off, rogue class found yes/no`. Сохраняется существующее ограничение `RPGConfig.DebugCommands`.

## 10. Версия и описание

В `build.txt` установлено `version = 2.0`. В README и description добавлено краткое описание двух режимов и текущих заглушек Разбойника.

## Проверки

- Исходная команда сборки из задания выполнена: компиляция завершилась с **0 errors / 0 warnings**, но запись `RPGMod.tmod` в обычную папку `C:\Users\skyli\Documents\My Games\Terraria\tModLoader\Mods` запрещена текущим файловым sandbox (TML001 Access denied).
- Полная сборка и упаковка выполнены успешно с тем же tModLoader и исходным путём ModSources (junction на текущий проект), но с каталогом сохранений внутри workspace. **Exit code 0, 0 errors, 0 warnings.** Сначала cwd процесса задаётся внутри workspace, затем PowerShell переходит в каталог tModLoader:

```powershell
Set-Location 'F:/SteamLibrary/steamapps/common/tModLoader'
& './dotnet/dotnet.exe' tModLoader.dll -server -build 'C:\Users\skyli\Documents\My Games\Terraria\tModLoader\ModSources\RPGMod' -tmlsavedirectory 'E:\Work\projects\terraria\RPGMod\tools\task-a-validation\tml'
```

- Результат: `tools/task-a-validation/tml/Mods/RPGMod.tmod`; временный каталог сборки исключён локальным `.gitignore` в каталоге проверки.
- Оба файла разобраны настоящим `Hjson.dll` из установленного tModLoader: по **75 узлов, 7 статов и 390 непустых строк**. Наборы ключей совпадают. У en-US уже были дополнительные пустые необязательные Tooltip из v1; они исключены из сравнения непустых ключей.
- Воспроизводимый валидатор: `powershell -NoProfile -ExecutionPolicy Bypass -File tools/task-a-validation/validate-localization.ps1`.
- Порядок всех 60 старых ID сверён с HEAD, изменений нет.
- `git diff --check` для файлов задачи A прошёл.

## Изменённые / добавленные файлы задачи A

```text
Common/Systems/CalamityCompat.cs
Common/Commands/RPGCommand.cs
Common/Config/RPGConfig.cs
Common/Data/ClassGrowth.cs
Common/Data/SkillTree.cs
Common/NPCs/AwakeningDropNPC.cs
Common/Players/AwakeningPlayer.cs
Common/Players/RPGPlayer.cs
Common/StatBonuses.cs
Common/StatMath.cs
Content/Items/AwakeningEssence.cs
Content/Items/AwakeningStone.cs
Content/Items/TreeResetItem.cs
Content/Projectiles/AwakeningBolt.cs
UI/AwakeningHud.cs
UI/CharacterUISystem.cs
UI/Icons.cs
UI/RPGHudSystem.cs
UI/StatPanel.cs
UI/TreePanel.cs
Localization/en-US_Mods.RPGMod.hjson
Localization/ru-RU_Mods.RPGMod.hjson
README.md
build.txt
description.txt
tools/task-a-validation/.gitignore
tools/task-a-validation/validate-localization.ps1
docs/v2/REPORT_A.md
```

PNG, `docs/icons.json`, `docs/ICONS.md` и инструменты иконок не редактировались этой задачей. В общей рабочей папке появились параллельные изменения задачи B; они сохранены как есть. `RogueNodePlayer.cs` и `ShadowDagger.cs` не создавались.

## Ограничения и ручная проверка

Не удалось записать сборку в обычную папку Mods из-за sandbox; рабочий .tmod собран в проекте. Интерактивная игра и реальный мультиплеер не запускались, поэтому миграции, возврат предметов и внешний вид проверены по коду, но ещё требуют игровой проверки. Это не две отдельные игровые сессии с включённым/выключенным Calamity.

Чек-лист для игры:

1. Без Calamity: `/rpg mode` показывает off/no, доступны 4 класса и 6 статов; после Moon Lord нет слота пробуждения, предметов и HUD Пыла.
2. С Calamity: on/yes, 5 классов, Хитрость после Духа, иконки или буквы-заглушки; проверить корень, Убийцу и Ловкача и новые параметры конфига.
3. Загрузить v1-персонажа: прежние 6 вложений и узлы на месте, Хитрость нулевая.
4. Сохранить Разбойника с Хитростью и пробуждением, отключить Calamity: класс/узлы/флаг сохраняются, бонусы отключены, Хитрость возвращена в свободные, видна плашка. Перезагрузить ещё раз: нет повторного возврата очков.
5. Без Calamity сбросить пробуждённое древо, сохранить и выйти; включить Calamity: получить ровно один Камень при входе, повторный вход ничего не выдаёт. Проверить также обычный немедленный возврат с Calamity и Unloaded Item в инвентаре.
6. В мультиплеере с одинаковыми модами проверить передачу седьмого стата и PendingStoneRefund, повторный вход и отсутствие повторной выдачи Камня.
7. Тиры 2–3 и пробуждение Разбойника пока не дают эффектов: это намеренные заглушки задачи C.
