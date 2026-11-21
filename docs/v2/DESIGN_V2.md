# RPGMod v2 — Calamity-режим, класс Разбойник

Дополнение к [основному дизайну](../DESIGN.md) для релиза 2.0. Таблицы описывают значения по умолчанию. Баланс Разбойника и его пробуждения настраивается в серверном конфиге.

## 1. Главная идея

Мод работает в двух режимах. Режим определяется **один раз при загрузке мода** (`ModLoader.HasMod("CalamityMod")`) и не меняется до перезагрузки модов.

| Что | Без Calamity (ванилла) | С Calamity |
| --- | --- | --- |
| Классы | 4: Воин, Стрелок, Маг, Призыватель | 5: + **Разбойник** |
| Статы | 6 | 7: + **Хитрость** |
| Узлы древа | 60 | 75 (+15 узлов Разбойника) |
| Пробуждение | **Нет**: ни слота «???», ни предметов, ни эффектов, ни HUD | Есть, 5 вариантов (+ Разбойник) |
| Предметы | Сброс статов, Сброс древа | + Эссенция пробуждения, Камень пробуждения |
| Уровни боссов | только ванильные | ванильные + Calamity (уже есть в v1) |

Мультиплеер: tModLoader требует одинаковый набор модов у сервера и клиентов, поэтому режим у всех один.

## 2. Архитектура переключения

Одна точка правды — `Common/Systems/CalamityCompat.cs` (`ModSystem`):

```csharp
public static class CalamityCompat            // + ModSystem для Load/Unload
{
    public static bool Enabled { get; private set; }        // HasMod("CalamityMod")
    public static Mod Calamity { get; private set; }
    public static DamageClass Rogue { get; private set; }    // TryFind<DamageClass>("RogueDamageClass"), иначе null
    public static bool RogueAvailable => Rogue != null;      // класс Разбойник и стат Хитрость
    public static bool AwakeningAvailable => Enabled;
}
```

Правила:

- Ни один файл, кроме `CalamityCompat`, `BossLevels` и рецепта Камня, не вызывает `ModLoader.TryGetMod("CalamityMod")` сам — только через `CalamityCompat`.
- Контент пробуждения (`AwakeningEssence`, `AwakeningStone`, `AwakeningBolt`, `AwakeningDropNPC`, `AwakeningHud`, новый снаряд Разбойника) не загружается без Calamity: `public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CalamityMod");` (в `IsLoadingEnabled` нельзя полагаться на `CalamityCompat.Enabled` — он ещё не заполнен). Если у игрока в инвентаре лежал Камень, а Calamity выключили, tModLoader сам сохранит его как Unloaded Item и вернёт при включении.
- `AwakeningPlayer` остаётся (он хранит `EssenceSources`), но все эффекты под `CalamityCompat.AwakeningAvailable`.
- `SkillTree` регистрирует узлы Разбойника всегда (ID нужны для загрузки сохранений), но `ClassIds.Available(classId)` возвращает `false` для Разбойника без Calamity. Эффекты узлов Разбойника — no-op, если `CalamityCompat.Rogue == null`.
- UI строит списки классов и статов из `ClassIds.AvailableClasses()` и `StatIds.Visible()`, а не из констант 4 и 6.

## 3. Сохранения — ничего не теряем

| Ситуация | Поведение |
| --- | --- |
| Сохранение v1 загружено в v2 | Работает как есть. Хитрость = 0. |
| `Awakened = true`, Calamity выключен | Флаг **сохраняется**, но ни на что не влияет и не показывается. Вернули Calamity — пробуждение снова активно. |
| Сброс древа без Calamity, `Awakened = true` | `Awakened = false`, ставится флаг `PendingStoneRefund = true` (сохраняется). При первом входе с Calamity Камень выдаётся и флаг снимается. |
| Класс Разбойник, Calamity выключен | Класс и узлы сохраняются. В окне персонажа вместо древа отображается сообщение о Calamity. Сброс древа снова открывает выбор класса. |
| Очки в Хитрости, Calamity выключен | При загрузке персонажа вложенные в Хитрость очки возвращаются в свободные (`Invested[Cunning] = 0`). Потерь нет. |
| Неизвестный `ClassId` (> Rogue) | Как в v1: сброс в «без класса». |

`ClassId` Разбойника = **5**, индекс Хитрости в `Invested` = **6**. Порядок старых значений не меняется. Сохранение `Invested` — список; при загрузке короче 7 — дополнить нулями.

## 4. Стат «Хитрость» (только с Calamity)

| Стат | ID иконки | Основной эффект | Вторичный эффект |
| --- | --- | --- | --- |
| Хитрость | `stat_cun` | Урон разбойника, до +60% | Скорость атаки разбойника, до +20% |

Формула та же: `bonus(n) = X * n / (n + 100)`. В панели статов строка стоит после Духа, перед Выносливостью; без Calamity строки нет.

Классовый прирост Разбойника (сумма 0,85): Хитрость 0,55 · Ловкость 0,1 · Выносливость 0,2.

## 5. Древо Разбойника (цвет класса #D46BB0)

Тир 1 — роль с плюсом и минусом, тир 2 — механика, тир 3 — условный эффект. Индексация как в DESIGN §4. Все эффекты считаются только для урона класса `CalamityCompat.Rogue` (через `CountsAsClass`, чтобы работали и наследники класса).

Собственная механика «**В тени**»: 2 с без атак (без использования предметов с уроном) → статус «в тени». Следующее попадание разбойника его снимает. Это наша механика, не стелс Calamity: так не нужен доступ к внутренним полям Calamity. HUD: маленький значок у полосы XP, когда статус активен.

| # | Тир | ID | Узел | Родитель | Эффект |
| --- | --- | --- | --- | --- | --- |
| 0 | Класс | `rogue` | Разбойник | — | +5% урона разбойника, +5% крита разбойника |
| 1 | 1 | `assassin` | Убийца | Разбойник | +12% урона, +6% крита разбойника; −10% макс. HP |
| 2 | 1 | `trickster` | Ловкач | Разбойник | +15% скорости атаки разбойника, +10% скорости передвижения; −6% урона разбойника |
| 3 | 2 | `ambush` | Засада | Убийца | Первый удар разбойника по врагу с полным HP наносит +50% |
| 4 | 2 | `shadow` | Тень | Убийца | Открывает «В тени»: удар из тени +25% урона и гарантированный крит |
| 5 | 2 | `juggler` | Жонглёр | Ловкач | Попадание разбойника даёт стак +3% скорости атаки разбойника на 3 с, до 5 стаков |
| 6 | 2 | `evasion` | Уклонение | Ловкач | 10% шанс полностью избежать удара, перезарядка 5 с |
| 7 | 3 | `executioner` | Палач | Засада | +30% урона разбойника по врагам ниже 30% HP |
| 8 | 3 | `first_blood` | Первая кровь | Засада | Удар Засады вешает кровотечение: 10% урона удара в секунду, 5 с |
| 9 | 3 | `vanish` | Исчезновение | Тень | Убийство врага сразу возвращает «в тени» |
| 10 | 3 | `dusk_blade` | Сумеречный клинок | Тень | Удар из тени: крит ×2,5 вместо ×2 и игнор 15 защиты |
| 11 | 3 | `ricochet` | Рикошет | Жонглёр | На 5 стаках попадание выпускает осколок в ближайшего другого врага в 15 блоках на 40% урона, не чаще 3 раз в секунду |
| 12 | 3 | `bravado` | Кураж | Жонглёр | На 5 стаках: +15% скорости передвижения и +10% крита разбойника |
| 13 | 3 | `smoke_bomb` | Дымовая шашка | Уклонение | После уклонения: 1,5 с неуязвимости и следующий удар разбойника +40% |
| 14 | 3 | `lucky_break` | Счастливый случай | Уклонение | +1% шанса уклонения за каждые 20 Удачи, до +10%; уклонение лечит 5% макс. HP |

## 6. Пробуждение Разбойника

| Класс | Пробуждение | Основной эффект | Надбавка от подкласса тира 1 |
| --- | --- | --- | --- |
| Разбойник | Повелитель теней | Каждое 8-е попадание разбойника выпускает веер из 3 теневых кинжалов по 60% урона (самонаводятся) | Убийца: кинжалы по 100% · Ловкач: каждое 5-е попадание |

Источники эссенций, рецепт Камня и условия — без изменений (DESIGN §5). Снаряд — новый `ShadowDagger` по образцу `AwakeningBolt`, текстура ванильная (без нового PNG).

Кинжалы пробуждения и Рикошета могут наносить разбойничий урон, но сами не запускают эффекты древа и не продвигают счётчик пробуждения. «Первая кровь» применяет кровотечение через регенерацию NPC. В сетевой игре его урон рассчитывает сервер, повторное кровотечение заменяет прежнее при равном или более высоком уроне.

## 7. UI

- **Выбор класса:** слотов столько, сколько `AvailableClasses()` (4 или 5). Ширину ряда считать от количества; при 5 уменьшить `gap`, чтобы ряд влез в ту же ширину области.
- **Древо Разбойника:** та же раскладка 1 → 2 → 4 → 8, цвет #D46BB0.
- **Панель статов:** 6 или 7 строк. Проверить, что 7 строк влезают по высоте (UI_SPEC); при нехватке — уменьшить шаг строк, не панель.
- **Слот пробуждения «???»** и **HUD Пыла** — только с Calamity.
- **Плашка «неактивно»** для Разбойника без Calamity (см. §3) — над древом, цвет `Theme.Accent`.
- **Подсказки предметов:** «Сброс древа» упоминает возврат Камня только с Calamity.
- **Главное меню / описание мода:** в `description.txt` и README коротко про два режима.

## 8. Иконки v2 (17 штук)

Стиль и формат — как в `docs/ICONS.md`. Палитра Разбойника: **dark plum, rose magenta, smoky grey, polished silver**.

| ID | Файл | Что нарисовано (Subject) |
| --- | --- | --- |
| `rogue` | `Assets/SkillIcons/rogue.png` | a hooded figure's shadowed face with two glinting eyes above a pair of crossed throwing daggers |
| `assassin` | `Assets/SkillIcons/assassin.png` | a curved silver dagger dripping a single rose-magenta drop |
| `trickster` | `Assets/SkillIcons/trickster.png` | a spinning playing card with a small dagger stuck through it |
| `ambush` | `Assets/SkillIcons/ambush.png` | a dagger thrusting out from a dark bush with two glowing eyes |
| `shadow` | `Assets/SkillIcons/shadow.png` | a dark plum silhouette of a crouching figure dissolving into smoke |
| `juggler` | `Assets/SkillIcons/juggler.png` | three throwing knives arranged in a circular juggling arc |
| `evasion` | `Assets/SkillIcons/evasion.png` | a blurred afterimage of a running figure with motion lines |
| `executioner` | `Assets/SkillIcons/executioner.png` | a heavy hooded executioner's blade above a cracked skull |
| `first_blood` | `Assets/SkillIcons/first_blood.png` | a dagger tip with three falling crimson blood drops |
| `vanish` | `Assets/SkillIcons/vanish.png` | a puff of grey smoke with an empty hood fading inside it |
| `dusk_blade` | `Assets/SkillIcons/dusk_blade.png` | a dagger with a dark purple blade glowing at the edge against a setting crescent moon |
| `ricochet` | `Assets/SkillIcons/ricochet.png` | a throwing star bouncing off a wall with a zigzag silver trail |
| `bravado` | `Assets/SkillIcons/bravado.png` | a tilted feathered rogue hat with a small dagger tucked in the band |
| `smoke_bomb` | `Assets/SkillIcons/smoke_bomb.png` | a round black bomb with a lit fuse releasing grey-magenta smoke |
| `lucky_break` | `Assets/SkillIcons/lucky_break.png` | a gold coin flipping in the air with a four-leaf clover on its face |
| `awaken_rogue` | `Assets/SkillIcons/awaken_rogue.png` | a radiant fan of three shadow daggers spreading from a glowing golden hood (группа «Пробуждения»: gold and white + rose magenta) |
| `stat_cun` | `Assets/StatIcons/stat_cun.png` | a sly half-closed eye inside a diamond (single hue: rose magenta) |

## 9. Порядок работ (Codex)

| Задача | Что | Файлы | Зависит от |
| --- | --- | --- | --- |
| A | Режимы, `CalamityCompat`, скрытие пробуждения, каркас Разбойника (ID, стат, прирост, корень и тир 1), UI под 4/5 классов и 6/7 статов, сохранения, локализация, версия 2.0 | `Common/**`, `Content/**/*.cs`, `UI/**`, `Localization/**`, `build.txt`, `description.txt` | — |
| B | 17 иконок + `icons.json` / `ICONS.md` | `Assets/**/*.png`, `docs/icons.json`, `docs/ICONS.md`, `tools/icons/**` | — (параллельно с A) |
| C | Условные узлы Разбойника (тиры 2–3), «В тени», пробуждение Разбойника, `ShadowDagger` | `Common/Players/Nodes/RogueNodePlayer.cs`, `AwakeningPlayer.cs`, `Content/Projectiles/ShadowDagger.cs`, HUD-значок, локализация | A |
| Проверка (Claude) | Сборка в двух режимах, ревью, чек-лист для игры, влить в DESIGN.md, коммит | — | A, B, C |

## 10. Открытые вопросы

- Точное имя класса урона разбойника в Calamity (`RogueDamageClass`?) и наследует ли он `Throwing` — Codex сверяет с исходниками CalamityModPublic перед реализацией.
- Использовать ли настоящий стелс Calamity вместо «В тени» — отложено до v2.1 (требует рефлексии по внутренним полям Calamity).
