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
XP до след = 300 × L^2.5
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

<details>
<summary><b>🔴 Воин — все 15 узлов</b></summary>

| | Тир | Узел | Эффект |
| :---: | :---: | --- | --- |
| <img src="Assets/SkillIcons/warrior.png" width="32"/> | 🎓 | **Воин** | +5% урона ближнего боя, +20 HP |
| <img src="Assets/SkillIcons/berserker.png" width="32"/> | I | **Берсерк** | +12% урона и +8% скорости атаки ближнего боя; −10% снижения урона |
| <img src="Assets/SkillIcons/guardian.png" width="32"/> | I | **Страж** | +10% HP, +8% снижения урона; −8% урона ближнего боя |
| <img src="Assets/SkillIcons/blood_rage.png" width="32"/> | II | **Кровавая ярость** | Урон растёт по мере потери HP, до +25% при HP ниже 30% |
| <img src="Assets/SkillIcons/frenzy.png" width="32"/> | II | **Неистовство** | Каждый удар даёт стак +2% скорости атаки на 3 с, до 10 стаков |
| <img src="Assets/SkillIcons/iron_flesh.png" width="32"/> | II | **Железная плоть** | +1% урона ближнего боя за каждые 5 защиты, до +25% |
| <img src="Assets/SkillIcons/retribution.png" width="32"/> | II | **Возмездие** | После получения урона следующий удар в течение 3 с наносит +30% |
| <img src="Assets/SkillIcons/possessed.png" width="32"/> | III | **Одержимый** | При HP ниже 25%: вампиризм 3% нанесённого урона, не больше 10 HP/с |
| <img src="Assets/SkillIcons/unbreakable.png" width="32"/> | III | **Несокрушимый** | Раз в 5 минут смертельный удар оставляет 1 HP и даёт 3 с неуязвимости |
| <img src="Assets/SkillIcons/whirlwind.png" width="32"/> | III | **Вихрь** | На 10 стаках: +15% урона и +15% скорости передвижения |
| <img src="Assets/SkillIcons/iron_rhythm.png" width="32"/> | III | **Железный ритм** | Стаки держатся 6 с, максимум 6; каждый стак ещё +2% урона |
| <img src="Assets/SkillIcons/colossus.png" width="32"/> | III | **Колосс** | +15% макс. HP; каждые 100 макс. HP дают +2% снижения урона, до +10% |
| <img src="Assets/SkillIcons/will_shield.png" width="32"/> | III | **Щит воли** | 8 с без получения урона создают щит на 15% макс. HP; иммунитет к отбрасыванию |
| <img src="Assets/SkillIcons/eye_for_eye.png" width="32"/> | III | **Око за око** | Удар Возмездия лечит на 5% нанесённого урона |
| <img src="Assets/SkillIcons/counterstrike.png" width="32"/> | III | **Ответный удар** | При получении урона враги в 10 блоках получают 100% этого урона, перезарядка 2 с |

</details>

<details>
<summary><b>🟢 Стрелок — все 15 узлов</b></summary>

| | Тир | Узел | Эффект |
| :---: | :---: | --- | --- |
| <img src="Assets/SkillIcons/ranger.png" width="32"/> | 🎓 | **Стрелок** | +5% урона дальнего боя, +5% скорости передвижения |
| <img src="Assets/SkillIcons/sniper.png" width="32"/> | I | **Снайпер** | +10% урона дальнего боя, +8% крита; −12% скорости атаки |
| <img src="Assets/SkillIcons/gunner.png" width="32"/> | I | **Пулемётчик** | +15% скорости атаки дальнего боя; −6% крита, −5% урона |
| <img src="Assets/SkillIcons/cold_blood.png" width="32"/> | II | **Хладнокровие** | Стоя на месте 1 с: +15% крита и +10% урона |
| <img src="Assets/SkillIcons/long_range.png" width="32"/> | II | **Дальнобойность** | Урон растёт с расстоянием до цели, до +20% на 40+ блоках |
| <img src="Assets/SkillIcons/spin_up.png" width="32"/> | II | **Раскрутка** | Непрерывная стрельба: +2% скорости атаки в секунду, до +20% |
| <img src="Assets/SkillIcons/quartermaster.png" width="32"/> | II | **Запасливость** | +20% шанс не потратить патрон, +8% скорости передвижения |
| <img src="Assets/SkillIcons/deadshot.png" width="32"/> | III | **Смертельный выстрел** | Криты наносят ×2,5 урона вместо ×2 |
| <img src="Assets/SkillIcons/hunters_mark.png" width="32"/> | III | **Метка охотника** | Попадание помечает цель на 5 с: она получает +10% урона от всех |
| <img src="Assets/SkillIcons/explosive_rounds.png" width="32"/> | III | **Разрывные снаряды** | Криты взрываются: 35% урона по врагам в радиусе 3 блоков |
| <img src="Assets/SkillIcons/armor_breaker.png" width="32"/> | III | **Бронебойность** | Выстрелы игнорируют 20 защиты врага |
| <img src="Assets/SkillIcons/lead_rain.png" width="32"/> | III | **Свинцовый ливень** | На максимальной раскрутке 20% шанс дополнительного снаряда |
| <img src="Assets/SkillIcons/overheat.png" width="32"/> | III | **Перегрев** | На максимальной раскрутке снаряды поджигают цель |
| <img src="Assets/SkillIcons/run_and_gun.png" width="32"/> | III | **Бег и стрельба** | В движении +10% урона дальнего боя |
| <img src="Assets/SkillIcons/bottomless_mag.png" width="32"/> | III | **Бездонный магазин** | Ещё +30% шанс не потратить патрон; при HP выше 80% +8% урона |

</details>

<details>
<summary><b>🟣 Маг — все 15 узлов</b></summary>

| | Тир | Узел | Эффект |
| :---: | :---: | --- | --- |
| <img src="Assets/SkillIcons/mage.png" width="32"/> | 🎓 | **Маг** | +5% урона магии, +20 маны |
| <img src="Assets/SkillIcons/arcanist.png" width="32"/> | I | **Арканист** | +14% урона магии; −15% макс. маны |
| <img src="Assets/SkillIcons/mystic.png" width="32"/> | I | **Мистик** | −20% расхода маны, +60 маны, быстрее регенерация; −8% урона магии |
| <img src="Assets/SkillIcons/overload.png" width="32"/> | II | **Перегрузка** | +1% урона магии за каждый 1% недостающей маны, до +25% |
| <img src="Assets/SkillIcons/elemental.png" width="32"/> | II | **Стихийная мощь** | 15% шанс наложить огонь, мороз или яд |
| <img src="Assets/SkillIcons/flow.png" width="32"/> | II | **Поток** | При мане выше 70%: +10% урона и +8% скорости атаки магии |
| <img src="Assets/SkillIcons/meditation.png" width="32"/> | II | **Медитация** | Стоя на месте: регенерация маны ×2 и +2 HP/с |
| <img src="Assets/SkillIcons/blood_magic.png" width="32"/> | III | **Магия крови** | При нехватке маны заклинание тратит HP: 2 HP за 1 ману |
| <img src="Assets/SkillIcons/resonance.png" width="32"/> | III | **Резонанс** | Каждое 5-е попадание магией наносит двойной урон |
| <img src="Assets/SkillIcons/catalyst.png" width="32"/> | III | **Катализ** | +15% урона по врагам под любым дебаффом |
| <img src="Assets/SkillIcons/chain_reaction.png" width="32"/> | III | **Цепная реакция** | Убийство врага под дебаффом переносит дебаффы на врагов в 10 блоках |
| <img src="Assets/SkillIcons/mana_shield.png" width="32"/> | III | **Мана-щит** | 20% полученного урона снимается с маны вместо HP |
| <img src="Assets/SkillIcons/overflow.png" width="32"/> | III | **Переполнение** | Регенерация маны не останавливается при атаке; ещё −10% расхода |
| <img src="Assets/SkillIcons/enlightenment.png" width="32"/> | III | **Просветление** | После 3 с на месте следующее заклинание наносит +40% |
| <img src="Assets/SkillIcons/mana_blood.png" width="32"/> | III | **Мана-кровь** | Каждые 100 потраченной маны восстанавливают 10 HP |

</details>

<details>
<summary><b>🔵 Призыватель — все 15 узлов</b></summary>

| | Тир | Узел | Эффект |
| :---: | :---: | --- | --- |
| <img src="Assets/SkillIcons/summoner.png" width="32"/> | 🎓 | **Призыватель** | +5% урона миньонов, +5% скорости хлыстов |
| <img src="Assets/SkillIcons/swarm_lord.png" width="32"/> | I | **Повелитель роя** | +1 слот миньонов; −8% урона миньонов |
| <img src="Assets/SkillIcons/beastmaster.png" width="32"/> | I | **Дрессировщик** | +14% урона и отбрасывания миньонов; −10% HP |
| <img src="Assets/SkillIcons/pack.png" width="32"/> | II | **Стая** | +2% урона миньонов за каждого активного миньона |
| <img src="Assets/SkillIcons/thousand_bites.png" width="32"/> | II | **Тысяча укусов** | Попадание миньона даёт стак на враге (до 10): +1% урона миньонов по цели за стак, стаки держатся 3 с |
| <img src="Assets/SkillIcons/whip_master.png" width="32"/> | II | **Мастер хлыста** | +20% скорости хлыстов; удар хлыстом даёт миньонам +10% урона на 5 с |
| <img src="Assets/SkillIcons/bond.png" width="32"/> | II | **Связь** | Пока есть хотя бы 1 миньон: +12% снижения урона |
| <img src="Assets/SkillIcons/legion.png" width="32"/> | III | **Легион** | +1 слот миньонов |
| <img src="Assets/SkillIcons/unity.png" width="32"/> | III | **Единство** | При 5+ миньонах: +10% снижения урона игроку |
| <img src="Assets/SkillIcons/hive.png" width="32"/> | III | **Улей** | На 10 стаках цель получает +10% урона от всех источников |
| <img src="Assets/SkillIcons/devourer.png" width="32"/> | III | **Пожиратель** | Убийство цели с 5+ стаками лечит на 5 HP и даёт +5% урона миньонов на 5 с, до 3 раз |
| <img src="Assets/SkillIcons/sic_em.png" width="32"/> | III | **Команда «Фас»** | Цель, отмеченная хлыстом, получает +15% урона от миньонов |
| <img src="Assets/SkillIcons/pack_heal.png" width="32"/> | III | **Забота о стае** | Удар хлыстом лечит 1 HP за каждого миньона, не чаще раза в секунду |
| <img src="Assets/SkillIcons/alpha.png" width="32"/> | III | **Альфа** | Если призван только один тип миньонов, +20% их урона |
| <img src="Assets/SkillIcons/sacrifice.png" width="32"/> | III | **Жертва** | Раз в 3 минуты смертельный удар убирает всех миньонов и оставляет 1 HP |

</details>

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
