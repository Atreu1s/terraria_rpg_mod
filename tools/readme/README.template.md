<p align="center">
  <img src="https://capsule-render.vercel.app/api?type=waving&color=0:e8604c,35:5cc461,70:a77bf0,100:3cc8d0&height=190&section=header&text=Terraria%20RPG%20Mod&fontSize=56&fontColor=ffffff&fontAlignY=36&desc=Levels%20%E2%80%A2%20Stats%20%E2%80%A2%20Classes%20%E2%80%A2%20Skill%20Tree%20%E2%80%A2%20Awakening&descSize=18&descAlignY=58&animation=fadeIn" width="100%"/>
</p>

<p align="center">
  <a href="https://github.com/Atreu1s/terraria_rpg_mod">
    <img src="https://readme-typing-svg.demolab.com?font=Fira+Code&weight=600&size=24&duration=2600&pause=900&color=F2C14E&center=true&vCenter=true&width=720&height=50&lines=Kill+monsters.+Gain+levels.;6+stats+with+diminishing+returns;4+classes+%E2%80%A2+60+passive+nodes;Awaken+your+class+after+the+Moon+Lord;Calamity-ready+%E2%80%A2+Multiplayer-ready" alt="Typing"/>
  </a>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/tModLoader-1.4.4-2f3a96?style=for-the-badge&logo=steam&logoColor=white"/>
  <img src="https://img.shields.io/badge/C%23-.NET_8-512BD4?style=for-the-badge&logo=dotnet&logoColor=white"/>
  <img src="https://img.shields.io/badge/Calamity-compatible-e8604c?style=for-the-badge"/>
  <img src="https://img.shields.io/badge/Multiplayer-ready-5cc461?style=for-the-badge"/>
</p>
<p align="center">
  <img src="https://img.shields.io/badge/🌍_Languages-RU_%7C_EN-000000?style=flat-square"/>
  <img src="https://img.shields.io/badge/🎨_Icons-74_pixel--art-000000?style=flat-square"/>
  <img src="https://img.shields.io/badge/🤖_Built_with-Claude_Code_+_Codex-000000?style=flat-square"/>
</p>

<p align="center">
  <img src="docs/readme/banner.png" width="100%" alt="Все 60 узлов древа"/>
</p>

<p align="center">
  <img src="https://user-images.githubusercontent.com/73097560/115834477-dbab4500-a447-11eb-908a-139a6edaec5c.gif" width="100%"/>
</p>

## ⚔️ Что это

**RPG-прогрессия для Terraria + Calamity.** Каждый убитый монстр даёт опыт, каждый уровень — очко стата. С 10 уровня выбираешь класс, а победы над ключевыми боссами открывают новые тиры древа пассивных навыков: **4 → 8 → 16 → 32**. После Лунного лорда собираешь эссенции и **пробуждаешь** свой класс.

> 🎯 Никаких активных способностей и масштабирования боссов — только пассивные и условные бонусы. Сложность по-прежнему регулирует сам Calamity.

| | |
| --- | --- |
| 📈 **Уровни** | Опыт по «силе» монстра, без потолка уровня |
| 💪 **6 статов** | Сила, Ловкость, Интеллект, Дух, Выносливость, Удача — с убывающей отдачей |
| 🛡️ **4 класса** | Воин, Стрелок, Маг, Призыватель + классовый прирост статов |
| 🌳 **Древо** | 60 пассивных узлов, на каждом тире — выбор одной из двух веток |
| ✨ **Пробуждение** | Финальная сила класса из эссенций боссов Calamity |
| 🔄 **Респек** | Предметы «Сброс статов» и «Сброс древа» |
| 🌐 **Мультиплеер** | Опыт считает сервер, прогресс хранится в персонаже |
| ⚙️ **Конфиг** | Все формулы опыта и статов настраиваются в игре |

<p align="center">
  <img src="https://user-images.githubusercontent.com/73097560/115834477-dbab4500-a447-11eb-908a-139a6edaec5c.gif" width="100%"/>
</p>

<p align="center">
  <img src="https://readme-typing-svg.demolab.com?font=Fira+Code&size=28&duration=3500&pause=4000&color=F2C14E&center=true&width=600&height=50&lines=Levels+%26+Experience" alt="Levels"/>
</p>

Опыт зависит от базовой силы монстра, поэтому режим сложности не меняет его уровень:

```text
power      = 0.6 × √lifeMax + 0.8 × damage + 0.5 × defense
level      = power / 3
XP за моба = 2 × power^1.5 × разница уровней × события × множитель конфига
XP до след = 60 × L^2.5
```

- 🐌 **Разница уровней:** на 5 уровней выше монстра — без штрафа, дальше −10% за уровень (минимум 10%). За сильного монстра — до +50%.
- 🎃 **События** (луны, затмение, вторжения) дают ×0.5, чтобы не перепрыгивать этапы.
- 👑 **Боссы** — по таблице уровней (ваниль + Calamity), награда — половина опыта их уровня.
- 🚫 Статуи, городские NPC и мелкие существа опыта не дают.

| Открытие | Уровень | Условие |
| :---: | :---: | --- |
| 🎓 Класс | **10** | — |
| 🥉 Тир I | **25** | Стена плоти |
| 🥈 Тир II | **50** | Все три механических босса |
| 🥇 Тир III | **75** | Лунный лорд |

<p align="center">
  <img src="https://readme-typing-svg.demolab.com?font=Fira+Code&size=28&duration=3500&pause=4000&color=F2C14E&center=true&width=600&height=50&lines=Stats" alt="Stats"/>
</p>

<p align="center">
  <img src="docs/readme/stats.png" alt="Иконки статов"/>
</p>

Убывающая отдача: `bonus(n) = X × n / (n + 100)` — 25 очков дают 20% от предела, 100 очков — 50%.

| | Стат | Основной эффект | Вторичный эффект |
| :---: | --- | --- | --- |
| <img src="Assets/StatIcons/stat_str.png" width="32"/> | **Сила** | Урон ближнего боя, до +60% | Скорость атаки ближнего боя, до +20% |
| <img src="Assets/StatIcons/stat_agi.png" width="32"/> | **Ловкость** | Урон дальнего боя, до +60% | Скорость бега до +25%, экономия патронов до 25% |
| <img src="Assets/StatIcons/stat_int.png" width="32"/> | **Интеллект** | Урон магии, до +60% | +1 мана за очко (до +200), расход маны до −25% |
| <img src="Assets/StatIcons/stat_spi.png" width="32"/> | **Дух** | Урон миньонов, до +60% | +1 слот на 30/80/150, хлысты до +30% |
| <img src="Assets/StatIcons/stat_vit.png" width="32"/> | **Выносливость** | Макс. HP, до +200 | Снижение урона до 15%, +1 регенерации за 25 очков |
| <img src="Assets/StatIcons/stat_luck.png" width="32"/> | **Удача** | Крит всех классов (и Разбойника), до +15% | `Player.luck`, до +0.3 |

<p align="center">
  <img src="https://readme-typing-svg.demolab.com?font=Fira+Code&size=28&duration=3500&pause=4000&color=F2C14E&center=true&width=600&height=50&lines=Classes" alt="Classes"/>
</p>

<p align="center">
  <img src="docs/readme/classes.png" alt="Классы"/>
</p>

Класс определяет автоматический прирост статов за уровень: `floor(уровень × коэффициент)`. Поздний выбор класса не наказывается — прирост считается от текущего уровня.

| Класс | Бонус класса | Прирост за уровень |
| --- | --- | --- |
| 🔴 **Воин** | +5% урона ближнего боя, +20 HP | Сила 0.5 · Выносливость 0.35 |
| 🟢 **Стрелок** | +5% урона дальнего боя, +5% скорости | Ловкость 0.6 · Выносливость 0.25 |
| 🟣 **Маг** | +5% урона магии, +20 маны | Интеллект 0.6 · Дух 0.1 · Выносливость 0.15 |
| 🔵 **Призыватель** | +5% урона миньонов, +5% скорости хлыстов | Дух 0.55 · Интеллект 0.1 · Выносливость 0.2 |

<p align="center">
  <img src="https://readme-typing-svg.demolab.com?font=Fira+Code&size=28&duration=3500&pause=4000&color=F2C14E&center=true&width=600&height=50&lines=Skill+Tree" alt="Skill Tree"/>
</p>

Тир I — роль с плюсом и минусом, тир II — механика, тир III — условный эффект. Выбрал ветку — соседняя закрывается. 👇 Пример выбранного пути для каждого класса (золотая рамка — пробуждённый узел):

<table>
  <tr>
    <td align="center"><b>🔴 Воин</b><br/><img src="docs/readme/tree_warrior.png" width="100%"/></td>
    <td align="center"><b>🟢 Стрелок</b><br/><img src="docs/readme/tree_ranger.png" width="100%"/></td>
  </tr>
  <tr>
    <td align="center"><b>🟣 Маг</b><br/><img src="docs/readme/tree_mage.png" width="100%"/></td>
    <td align="center"><b>🔵 Призыватель</b><br/><img src="docs/readme/tree_summoner.png" width="100%"/></td>
  </tr>
</table>

{{NODES}}

<p align="center">
  <img src="https://readme-typing-svg.demolab.com?font=Fira+Code&size=28&duration=3500&pause=4000&color=F2C14E&center=true&width=600&height=50&lines=Awakening" alt="Awakening"/>
</p>

<p align="center">
  <img src="docs/readme/awakening.png" alt="Пробуждения"/>
</p>

1. 🌙 **Подсказка.** После Лунного лорда в древе появляется слот «???»: *«Сила спит в сердцах тех, кто пал после Луны»*.
2. 💀 **Эссенции.** Providence, Ceaseless Void, Storm Weaver, Signus и Polterghast роняют личную Эссенцию пробуждения. Нужны 3 с разных боссов.
3. ⚒️ **Крафт.** Камень = 3 эссенции + 10 Divine Geode + 3 Armored Shell + 3 Dark Plasma + 3 Twisting Nether на Древнем манипуляторе.
4. ✨ **Пробуждение.** Требует узел тира III. Работает уже на Devourer of Gods, Yharon, Exo Mechs и Supreme Calamitas.

| | Пробуждение | Эффект | Надбавка подкласса |
| :---: | --- | --- | --- |
| <img src="Assets/SkillIcons/awaken_warrior.png" width="32"/> | **Аватар войны** | Удары копят Пыл; на полной шкале 8 с: +30% урона, +20% скорости атаки | Берсерк: вампиризм 2% · Страж: +15% снижения урона |
| <img src="Assets/SkillIcons/awaken_ranger.png" width="32"/> | **Око бури** | Каждый 10-й выстрел — самонаводящийся снаряд на 300% | Снайпер: каждый 6-й · Пулемётчик: 3 снаряда по 100% |
| <img src="Assets/SkillIcons/awaken_mage.png" width="32"/> | **Архимаг** | Мана кончилась — 5 с бесконечной маны, перезарядка 45 с | Арканист: +25% урона · Мистик: лечит 20% HP |
| <img src="Assets/SkillIcons/awaken_summoner.png" width="32"/> | **Владыка духов** | +1 слот; миньоны критуют с шансом крита игрока | Повелитель роя: ещё +1 слот · Дрессировщик: криты ×2.5 |

<p align="center">
  <img src="https://readme-typing-svg.demolab.com?font=Fira+Code&size=28&duration=3500&pause=4000&color=F2C14E&center=true&width=600&height=50&lines=Items" alt="Items"/>
</p>

<p align="center">
  <img src="docs/readme/items.png" alt="Предметы"/>
</p>

| | Предмет | Рецепт | Что делает |
| :---: | --- | --- | --- |
| <img src="Content/Items/StatResetItem.png" width="32"/> | **Сброс статов** | 1 Дерево + 1 Упавшая звезда · Верстак | Возвращает все вложенные очки статов |
| <img src="Content/Items/TreeResetItem.png" width="32"/> | **Сброс древа** | 1 Дерево + 2 Упавшие звезды · Верстак | Сбрасывает класс, узлы и пробуждение (Камень возвращается) |
| <img src="Content/Items/AwakeningEssence.png" width="32"/> | **Эссенция пробуждения** | Дроп с 5 боссов Calamity | Материал для Камня |
| <img src="Content/Items/AwakeningStone.png" width="32"/> | **Камень пробуждения** | См. выше · Древний манипулятор | Пробуждает класс |

<p align="center">
  <img src="https://user-images.githubusercontent.com/73097560/115834477-dbab4500-a447-11eb-908a-139a6edaec5c.gif" width="100%"/>
</p>

## 🚀 Установка

1. Установи [tModLoader](https://store.steampowered.com/app/1281930/tModLoader/) (1.4.4). Calamity — по желанию: без него мод работает, только без пробуждения.
2. Склонируй репозиторий в папку `ModSources`:
   ```bash
   cd "Documents/My Games/Terraria/tModLoader/ModSources"
   git clone https://github.com/Atreu1s/terraria_rpg_mod.git RPGMod
   ```
3. В игре: **Workshop → Develop Mods → RPGMod → Build + Reload**.
4. Открой инвентарь и нажми кнопку **«Статус»** — откроется окно персонажа.

> 💡 Позицию кнопки «Статус» можно поменять в конфиге **«Интерфейс RPG»**, а клавишу открытия окна — в настройках управления.

## 🛠️ Отладка

Включи **«Отладочные команды»** в конфиге мода, и в чате станет доступна команда `/rpg`:

| Команда | Что делает |
| --- | --- |
| `/rpg` | Показать уровень, опыт, класс и флаги боссов |
| `/rpg level 50` | Установить уровень |
| `/rpg xp 10000` | Начислить опыт (с повышением уровня) |
| `/rpg downed wof` · `mech` · `moonlord` · `all` · `none` | Выставить флаги боссов мира |
| `/rpg reset` | Сбросить прогресс персонажа |

## 🗂️ Структура

```text
RPGMod/
├── Common/
│   ├── Players/        RPGPlayer (уровень, статы, древо, сохранение, сеть) + узлы по классам
│   ├── NPCs/           начисление опыта, дроп эссенций
│   ├── Data/           реестр 60 узлов, уровни боссов, прирост классов
│   ├── Config/         серверный и клиентский конфиг
│   └── StatMath.cs     все формулы
├── Content/Items/      сбросы, эссенция, камень
├── UI/                 HUD уровня, окно статов, панель древа
├── Assets/             иконки узлов и статов (32×32)
├── Localization/       ru-RU + en-US
└── docs/               дизайн-документ, UI-спецификация, макеты
```

📖 Полные механики — в [docs/DESIGN.md](docs/DESIGN.md), внешний вид — в [docs/UI_SPEC.md](docs/UI_SPEC.md).

## 🤖 Как сделан

[![Claude Code](https://img.shields.io/badge/Claude_Code-D97757?style=for-the-badge&logo=anthropic&logoColor=white)](https://claude.com/claude-code)
[![Codex](https://img.shields.io/badge/Codex-000000?style=for-the-badge&logo=openai&logoColor=white)](https://openai.com/codex)
[![tModLoader](https://img.shields.io/badge/tModLoader-2f3a96?style=for-the-badge&logo=steam&logoColor=white)](https://github.com/tModLoader/tModLoader)

Мод собран двумя ИИ-агентами параллельно по общему дизайн-документу:

- 🧠 **Claude Code** — ядро: опыт, статы, классы, узлы Воина и Стрелка, сеть, сохранение, весь интерфейс.
- 🎨 **Codex** — 74 пиксельные иконки, узлы Мага и Призывателя, предметы сброса, пробуждение.

<p align="center">
  <img src="https://capsule-render.vercel.app/api?type=waving&color=0:3cc8d0,35:a77bf0,70:5cc461,100:e8604c&height=100&section=footer&reversal=true" width="100%"/>
</p>

<p align="center">
  <img src="https://readme-typing-svg.demolab.com?font=Fira+Code&size=20&duration=3500&pause=4000&color=F2C14E&center=true&width=600&height=50&lines=Made+by+Atreu1s+%E2%80%A2+v0.1" alt="Footer"/>
</p>
