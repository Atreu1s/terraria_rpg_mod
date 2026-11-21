# Иконки RPG-мода: список

Всего иконок: 91: 75 узлов древа, 5 пробуждений, 7 статов, 4 предмета. Тот же список лежит в `icons.json` для пакетной генерации.

Задание для Codex — в `PROMPT_CODEX.md` в корне комплекта.

## Общий стиль (уже включён в каждый промпт)

```text
Pixel art game icon in the style of 2D sandbox games like Terraria. Designed on a 32x32 pixel grid and rendered at 512x512 so that every art pixel is a crisp 16x16 block. Hard pixel edges, no anti-aliasing, no blur, no smooth gradients (use 2-3 flat shading steps). 1-pixel dark outline (#1a1020) around the whole symbol, light source from the top-left. Limited palette, at most 16 colors. One centered symbol filling about 80% of the canvas. Fully transparent background. No frame, no border, no slot, no text, no letters, no numbers, no watermark.
```

Палитры групп:

- **Воин:** Palette: crimson red, warm orange, steel grey, dark leather brown.
- **Стрелок:** Palette: forest green, lime, brass yellow, wood brown.
- **Маг:** Palette: violet, deep purple, arcane blue, white glow highlights.
- **Призыватель:** Palette: teal, cyan, ghostly pale blue, dark slate.
- **Разбойник (v2, только с Calamity):** Palette: dark plum, rose magenta, smoky grey, polished silver.
- **Пробуждения:** Palette: radiant gold and white plus the class color, slightly more ornate and glowing than regular icons.
- **Статы:** Palette: a single strong hue as stated, plus outline and highlight colors.
- **Предметы:** Palette: free, readable as an in-game item sprite.

## Список иконок

### Воин

| ID | Название | Файл | Что нарисовано (Subject) |
| --- | --- | --- | --- |
| `warrior` | Воин | `Assets/SkillIcons/warrior.png` | a crossed steel sword and round shield with a red cloth wrap on the hilt |
| `berserker` | Берсерк | `Assets/SkillIcons/berserker.png` | a double-bladed battle axe with a glowing red edge and two small blood drops |
| `guardian` | Страж | `Assets/SkillIcons/guardian.png` | a large steel tower shield with a golden rim and a center boss |
| `blood_rage` | Кровавая ярость | `Assets/SkillIcons/blood_rage.png` | a cracked red heart with orange flames rising from the crack |
| `frenzy` | Неистовство | `Assets/SkillIcons/frenzy.png` | three stacked curved red-orange slash marks with short motion lines |
| `iron_flesh` | Железная плоть | `Assets/SkillIcons/iron_flesh.png` | a clenched fist made of riveted iron plates |
| `retribution` | Возмездие | `Assets/SkillIcons/retribution.png` | a dented shield with a sword thrusting out from behind it and a red spark at the tip |
| `possessed` | Одержимый | `Assets/SkillIcons/possessed.png` | a dark horned helmet visor with one glowing red eye inside |
| `unbreakable` | Несокрушимый | `Assets/SkillIcons/unbreakable.png` | a cracked steel helmet still standing, golden light shining through the cracks |
| `whirlwind` | Вихрь | `Assets/SkillIcons/whirlwind.png` | a sword spinning in a circular red-and-steel vortex |
| `iron_rhythm` | Железный ритм | `Assets/SkillIcons/iron_rhythm.png` | a war drum with two crossed short swords used as drumsticks |
| `colossus` | Колосс | `Assets/SkillIcons/colossus.png` | a massive iron breastplate with broad pauldrons |
| `will_shield` | Щит воли | `Assets/SkillIcons/will_shield.png` | a translucent golden energy barrier in front of a small upright sword |
| `eye_for_eye` | Око за око | `Assets/SkillIcons/eye_for_eye.png` | two mirrored daggers pointing at each other with a small red heart between them |
| `counterstrike` | Ответный удар | `Assets/SkillIcons/counterstrike.png` | a round spiked shield with a red shockwave ring bursting outward |

### Стрелок

| ID | Название | Файл | Что нарисовано (Subject) |
| --- | --- | --- | --- |
| `ranger` | Стрелок | `Assets/SkillIcons/ranger.png` | a wooden longbow with a nocked arrow with green fletching |
| `sniper` | Снайпер | `Assets/SkillIcons/sniper.png` | a round scope crosshair reticle centered on a single bullet |
| `gunner` | Пулемётчик | `Assets/SkillIcons/gunner.png` | a cluster of rotary gun barrels with a bright muzzle flash |
| `cold_blood` | Хладнокровие | `Assets/SkillIcons/cold_blood.png` | a pale blue ice crystal with a small crosshair inside it |
| `long_range` | Дальнобойность | `Assets/SkillIcons/long_range.png` | an arrow flying left to right with a long dotted trail toward a small distant target |
| `spin_up` | Раскрутка | `Assets/SkillIcons/spin_up.png` | rotary gun barrels seen from the front with circular speed arrows around them |
| `quartermaster` | Запасливость | `Assets/SkillIcons/quartermaster.png` | an open wooden ammo crate full of bullets and arrows |
| `deadshot` | Смертельный выстрел | `Assets/SkillIcons/deadshot.png` | a golden bullet with a sharp star-shaped critical-hit burst at its tip |
| `hunters_mark` | Метка охотника | `Assets/SkillIcons/hunters_mark.png` | a glowing red triangular hunter's sigil over a small target dummy |
| `explosive_rounds` | Разрывные снаряды | `Assets/SkillIcons/explosive_rounds.png` | a bullet with a fiery orange explosion bursting from its tip |
| `armor_breaker` | Бронебойность | `Assets/SkillIcons/armor_breaker.png` | an arrow piercing straight through a cracked steel plate |
| `lead_rain` | Свинцовый ливень | `Assets/SkillIcons/lead_rain.png` | many small bullets falling like rain from a dark storm cloud |
| `overheat` | Перегрев | `Assets/SkillIcons/overheat.png` | a red-hot glowing gun barrel with steam and small flames |
| `run_and_gun` | Бег и стрельба | `Assets/SkillIcons/run_and_gun.png` | a running leather boot next to a pistol, with motion lines |
| `bottomless_mag` | Бездонный магазин | `Assets/SkillIcons/bottomless_mag.png` | an ammo belt of bullets curling into an infinity loop |

### Маг

| ID | Название | Файл | Что нарисовано (Subject) |
| --- | --- | --- | --- |
| `mage` | Маг | `Assets/SkillIcons/mage.png` | a wooden wizard staff topped with a glowing purple crystal |
| `arcanist` | Арканист | `Assets/SkillIcons/arcanist.png` | an open spellbook with violet runes floating above the pages |
| `mystic` | Мистик | `Assets/SkillIcons/mystic.png` | a blue mana star resting inside a crescent moon |
| `overload` | Перегрузка | `Assets/SkillIcons/overload.png` | a cracked mana crystal leaking violet lightning |
| `elemental` | Стихийная мощь | `Assets/SkillIcons/elemental.png` | an orb split into three wedges: orange fire, pale blue ice and green poison |
| `flow` | Поток | `Assets/SkillIcons/flow.png` | a flowing stream of blue mana curling into a wave |
| `meditation` | Медитация | `Assets/SkillIcons/meditation.png` | a hovering lotus flower with a soft blue mana glow above it |
| `blood_magic` | Магия крови | `Assets/SkillIcons/blood_magic.png` | a red drop of blood turning into a purple glowing rune at its bottom |
| `resonance` | Резонанс | `Assets/SkillIcons/resonance.png` | five concentric violet rings around a bright white center |
| `catalyst` | Катализ | `Assets/SkillIcons/catalyst.png` | a round alchemical flask with bubbling multicolored liquid |
| `chain_reaction` | Цепная реакция | `Assets/SkillIcons/chain_reaction.png` | three small orbs linked by violet lightning arcs |
| `mana_shield` | Мана-щит | `Assets/SkillIcons/mana_shield.png` | a blue hexagonal energy barrier with a mana star in its center |
| `overflow` | Переполнение | `Assets/SkillIcons/overflow.png` | a golden chalice overflowing with glowing blue mana |
| `enlightenment` | Просветление | `Assets/SkillIcons/enlightenment.png` | a glowing open eye with radiant violet rays around it |
| `mana_blood` | Мана-кровь | `Assets/SkillIcons/mana_blood.png` | a red heart and a blue mana star intertwined |

### Призыватель

| ID | Название | Файл | Что нарисовано (Subject) |
| --- | --- | --- | --- |
| `summoner` | Призыватель | `Assets/SkillIcons/summoner.png` | a short summoning staff with a teal spirit wisp floating above it |
| `swarm_lord` | Повелитель роя | `Assets/SkillIcons/swarm_lord.png` | a small crown with several teal fireflies circling it |
| `beastmaster` | Дрессировщик | `Assets/SkillIcons/beastmaster.png` | a spirit wolf head in profile wearing a leather collar |
| `pack` | Стая | `Assets/SkillIcons/pack.png` | three small spirit wolf heads side by side |
| `thousand_bites` | Тысяча укусов | `Assets/SkillIcons/thousand_bites.png` | a ring of many small teal fang bite marks |
| `whip_master` | Мастер хлыста | `Assets/SkillIcons/whip_master.png` | a coiled leather whip with a glowing teal tip mid-crack |
| `bond` | Связь | `Assets/SkillIcons/bond.png` | a hand and a ghostly spirit paw linked by a glowing teal thread |
| `legion` | Легион | `Assets/SkillIcons/legion.png` | three rows of tiny ghostly spirit shields marching |
| `unity` | Единство | `Assets/SkillIcons/unity.png` | five small teal spirits in a circle around a shield |
| `hive` | Улей | `Assets/SkillIcons/hive.png` | a honeycomb hive with glowing teal cells |
| `devourer` | Пожиратель | `Assets/SkillIcons/devourer.png` | an open fanged spirit maw swallowing a glowing orb |
| `sic_em` | Команда «Фас» | `Assets/SkillIcons/sic_em.png` | a whip crack pointing at a red target mark with a small paw print |
| `pack_heal` | Забота о стае | `Assets/SkillIcons/pack_heal.png` | a teal paw print with a small green healing cross |
| `alpha` | Альфа | `Assets/SkillIcons/alpha.png` | a howling spirit wolf head wearing a small golden crown |
| `sacrifice` | Жертва | `Assets/SkillIcons/sacrifice.png` | a teal spirit wisp shattering into pieces around a red heart |

### Разбойник (v2, только с Calamity)

| ID | Название | Файл | Что нарисовано (Subject) |
| --- | --- | --- | --- |
| `rogue` | Разбойник | `Assets/SkillIcons/rogue.png` | a hooded figure's shadowed face with two glinting eyes above a pair of crossed throwing daggers |
| `assassin` | Убийца | `Assets/SkillIcons/assassin.png` | a curved silver dagger dripping a single rose-magenta drop |
| `trickster` | Ловкач | `Assets/SkillIcons/trickster.png` | a spinning playing card with a small dagger stuck through it |
| `ambush` | Засада | `Assets/SkillIcons/ambush.png` | a dagger thrusting out from a dark bush with two glowing eyes |
| `shadow` | Тень | `Assets/SkillIcons/shadow.png` | a dark plum silhouette of a crouching figure dissolving into smoke |
| `juggler` | Жонглёр | `Assets/SkillIcons/juggler.png` | three throwing knives arranged in a circular juggling arc |
| `evasion` | Уклонение | `Assets/SkillIcons/evasion.png` | a blurred afterimage of a running figure with motion lines |
| `executioner` | Палач | `Assets/SkillIcons/executioner.png` | a heavy hooded executioner's blade above a cracked skull |
| `first_blood` | Первая кровь | `Assets/SkillIcons/first_blood.png` | a dagger tip with three falling crimson blood drops |
| `vanish` | Исчезновение | `Assets/SkillIcons/vanish.png` | a puff of grey smoke with an empty hood fading inside it |
| `dusk_blade` | Сумеречный клинок | `Assets/SkillIcons/dusk_blade.png` | a dagger with a dark purple blade glowing at the edge against a setting crescent moon |
| `ricochet` | Рикошет | `Assets/SkillIcons/ricochet.png` | a throwing star bouncing off a wall with a zigzag silver trail |
| `bravado` | Кураж | `Assets/SkillIcons/bravado.png` | a tilted feathered rogue hat with a small dagger tucked in the band |
| `smoke_bomb` | Дымовая шашка | `Assets/SkillIcons/smoke_bomb.png` | a round black bomb with a lit fuse releasing grey-magenta smoke |
| `lucky_break` | Счастливый случай | `Assets/SkillIcons/lucky_break.png` | a gold coin flipping in the air with a four-leaf clover on its face |

### Пробуждения

| ID | Название | Файл | Что нарисовано (Subject) |
| --- | --- | --- | --- |
| `awaken_warrior` | Пробуждение: Аватар войны | `Assets/SkillIcons/awaken_warrior.png` | a flaming greatsword with small golden wings at the crossguard; class color crimson red |
| `awaken_ranger` | Пробуждение: Око бури | `Assets/SkillIcons/awaken_ranger.png` | a storm eye with a lightning arrow bursting out of its pupil; class color green |
| `awaken_mage` | Пробуждение: Архимаг | `Assets/SkillIcons/awaken_mage.png` | a floating crowned grimoire with an infinity rune on the cover; class color violet |
| `awaken_summoner` | Пробуждение: Владыка духов | `Assets/SkillIcons/awaken_summoner.png` | a spectral crown with several spirits circling it; class color teal |
| `awaken_rogue` | Пробуждение: Повелитель теней | `Assets/SkillIcons/awaken_rogue.png` | a radiant fan of three shadow daggers spreading from a glowing golden hood; class color rose magenta |

### Статы

| ID | Название | Файл | Что нарисовано (Subject) |
| --- | --- | --- | --- |
| `stat_str` | Сила | `Assets/StatIcons/stat_str.png` | a flexed muscular arm, crimson red |
| `stat_agi` | Ловкость | `Assets/StatIcons/stat_agi.png` | a winged boot, bright green |
| `stat_int` | Интеллект | `Assets/StatIcons/stat_int.png` | a glowing crystal ball on a small stand, violet |
| `stat_spi` | Дух | `Assets/StatIcons/stat_spi.png` | a ghostly spirit flame, teal |
| `stat_vit` | Выносливость | `Assets/StatIcons/stat_vit.png` | a heart with a small plus sign, orange-red |
| `stat_luck` | Удача | `Assets/StatIcons/stat_luck.png` | a four-leaf clover on a gold coin, golden yellow |
| `stat_cun` | Хитрость | `Assets/StatIcons/stat_cun.png` | a sly half-closed eye inside a diamond (single hue: rose magenta) |

### Предметы

| ID | Название | Файл | Что нарисовано (Subject) |
| --- | --- | --- | --- |
| `AwakeningEssence` | Эссенция пробуждения | `Content/Items/AwakeningEssence.png` | a floating orb of golden light with tiny sparkles around it (Terraria-like crafting essence) |
| `AwakeningStone` | Камень пробуждения | `Content/Items/AwakeningStone.png` | an ornate gold-and-violet gemstone set in a carved stone with glowing runes |
| `StatResetItem` | Сброс статов | `Content/Items/StatResetItem.png` | an hourglass with sand in six colors: red, green, violet, teal, orange, gold |
| `TreeResetItem` | Сброс древа | `Content/Items/TreeResetItem.png` | a small corked glass bottle with a silver sapling growing inside |

## Как вставить в мод

- Иконки узлов и пробуждений грузятся по ID: `ModContent.Request<Texture2D>("RPGMod/Assets/SkillIcons/" + node.Id)`.
- Иконки предметов лежат рядом с классом предмета (`Content/Items/StatResetItem.png` для `StatResetItem.cs`), tModLoader подхватывает их сам.
- В слоте 52 px иконка рисуется в натуральную величину 32 × 32 (как предмет в инвентаре), в подсказке можно ×2. Всегда `SamplerState.PointClamp`.
- Иконку, которая не понравилась, перегенерировать по её ID: промпт лежит в `icons.json`, исходник в `_raw/`.
