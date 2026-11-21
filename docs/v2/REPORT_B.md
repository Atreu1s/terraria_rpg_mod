# Задача B — иконки v2

Созданы 17 новых иконок из DESIGN_V2.md §8: 16 в SkillIcons и 1 в StatIcons. Обновлены манифест (91 запись) и каталог (75 узлов, 5 пробуждений, 7 статов, 4 предмета).

## Файлы

| ID | Итоговый PNG | Исходник 512×512 |
| --- | --- | --- |
| rogue | Assets/SkillIcons/rogue.png | tools/icons/_raw/rogue.png |
| assassin | Assets/SkillIcons/assassin.png | tools/icons/_raw/assassin.png |
| trickster | Assets/SkillIcons/trickster.png | tools/icons/_raw/trickster.png |
| ambush | Assets/SkillIcons/ambush.png | tools/icons/_raw/ambush.png |
| shadow | Assets/SkillIcons/shadow.png | tools/icons/_raw/shadow.png |
| juggler | Assets/SkillIcons/juggler.png | tools/icons/_raw/juggler.png |
| evasion | Assets/SkillIcons/evasion.png | tools/icons/_raw/evasion.png |
| executioner | Assets/SkillIcons/executioner.png | tools/icons/_raw/executioner.png |
| first_blood | Assets/SkillIcons/first_blood.png | tools/icons/_raw/first_blood.png |
| vanish | Assets/SkillIcons/vanish.png | tools/icons/_raw/vanish.png |
| dusk_blade | Assets/SkillIcons/dusk_blade.png | tools/icons/_raw/dusk_blade.png |
| ricochet | Assets/SkillIcons/ricochet.png | tools/icons/_raw/ricochet.png |
| bravado | Assets/SkillIcons/bravado.png | tools/icons/_raw/bravado.png |
| smoke_bomb | Assets/SkillIcons/smoke_bomb.png | tools/icons/_raw/smoke_bomb.png |
| lucky_break | Assets/SkillIcons/lucky_break.png | tools/icons/_raw/lucky_break.png |
| awaken_rogue | Assets/SkillIcons/awaken_rogue.png | tools/icons/_raw/awaken_rogue.png |
| stat_cun | Assets/StatIcons/stat_cun.png | tools/icons/_raw/stat_cun.png |

Остальные файлы задачи:

- docs/icons.json — 17 записей с полным общим стилем, групповой палитрой и Subject.
- docs/ICONS.md — раздел Разбойника, палитра, строки пробуждения и Хитрости, счётчик 91.
- tools/icons/preview.js — режим --v2; обычный режим сохранён.
- tools/icons/_preview_v2.png — 21 иконка: четыре существующих корня и все 17 новых, семь в ряд; увеличение ×4 nearest neighbor, фон #2f3a96.
- tools/icons/v2-ids.json — порядок 17 новых ID.
- tools/icons/baseline-v1.json — SHA-256 74 исходных PNG v1.
- tools/icons/check-v2.js — проверка уникальности ID, групп, 512×512 исходников и неизменности v1.
- tools/icons/generation-v2.json — журнал запросов и исходников генератора.
- tools/icons/README.md — команды обработки и проверки v2.
- tools/icons/_raw/_attempts/lucky_break_silver.png — сохранённый неудачный вариант монеты.
- docs/v2/REPORT_B.md — этот отчёт (прямо запрошенное исключение из общего списка разрешённых путей).

## Конвейер

Генерация встроенным image generator с transparent_background=true, затем существующий Node + pngjs tools/icons/process.js --id <ID>. process.js, common.js и check.js не менялись. Проверены все 74 существующих PNG: каждый 32×32. Новые используют тот же алгоритм: нормализация сырья до 512×512 nearest neighbor, очистка фона, порог alpha 128, обрезка по непрозрачным границам, вписывание в 28×28 на холсте 32×32, nearest neighbor, median-cut до 16 непрозрачных цветов, 8-bit RGBA. Генератор возвращал больший размер, который штатно нормализован конвейером v1.

## Повторные запросы

- smoke_bomb: первая попытка отклонена системой генерации (moderation_blocked, illicit), изображения не было. Повтор с пояснением про вымышленный игровой предмет с условными неработающими пропорциями успешен. Базовый манифестный промпт сохранён.
- lucky_break: первая генерация дала серебряную монету с пурпурным клевером вместо золотой. Вторая попытка явно требует жёлто-золотую монету и охристую тень. Первый вариант сохранён отдельно.
- Старые 74 PNG не перегенерировались и не перезаписывались.

## Визуальная оценка

Полное превью просмотрено рядом с корнями warrior, ranger, mage, summoner. Общий масштаб, крупные пиксели, контуры и тематические палитры согласованы с v1. Самые слабые места при 32×32:

- shadow: дым дробит силуэт, поза присевшей фигуры читается неоднозначно.
- evasion: серебристые следы движения сливаются с фигурой; ощущение скорости сильнее читаемости персонажа.
- ambush: куст и кинжал различимы, но два глаза слишком мелкие и теряются в тенях.
- dusk_blade: серебряный полумесяц перетягивает внимание с тёмного кинжала.

Условное «размытие» evasion выполнено жёсткими пиксельными следами, без полупрозрачности и фильтра размытия, согласно общему стилю. Технический валидатор не оценивает художественную читаемость.

## Проверки

После окончательного превью:

```text
node tools/icons/check.js
PASS: 91/91 icons; 32x32, RGBA, transparent corners, binary alpha, <=16 opaque colors.
node tools/icons/check-v2.js
PASS: 17 v2 IDs/raw sources; 91 unique entries; all 74 v1 PNG hashes unchanged.
node tools/icons/preview.js --v2
Preview: tools/icons/_preview_v2.png (21/21 icons, 7 per row)
```

74 старые записи icons.json также сравнены с HEAD и полностью сохранены. C#, локализация и существующие PNG задачей B не менялись. Параллельные изменения других задач оставлены как есть. Сборка мода не запускалась: задача меняет только графические ресурсы и каталог. Коммит не создавался.
