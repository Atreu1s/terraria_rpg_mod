<p align="center">
  <img src="https://capsule-render.vercel.app/api?type=waving&color=0:e8604c,25:5cc461,50:a77bf0,75:3cc8d0,100:d46bb0&height=190&section=header&text=Terraria%20RPG%20Mod&fontSize=56&fontColor=ffffff&fontAlignY=37&desc=Level%20up%20%E2%80%A2%20Choose%20a%20class%20%E2%80%A2%20Build%20your%20skill%20tree&descSize=18&descAlignY=58" width="100%" alt="Terraria RPG Mod"/>
</p>

# Terraria RPG Mod 2.0

<p align="center">
  <img src="https://img.shields.io/badge/Terraria-1.4.4-2f3a96?style=for-the-badge" alt="Terraria 1.4.4"/>
  <img src="https://img.shields.io/badge/tModLoader-.NET_8-512bd4?style=for-the-badge" alt="tModLoader on .NET 8"/>
  <img src="https://img.shields.io/badge/Calamity-optional-d46bb0?style=for-the-badge" alt="Optional Calamity integration"/>
  <img src="https://img.shields.io/badge/Localization-EN_%2F_RU-5cc461?style=for-the-badge" alt="English and Russian localization"/>
</p>

<p align="center">
  <img src="docs/readme/banner.png" width="100%" alt="All 75 passive skill icons across Warrior, Ranger, Mage, Summoner, and Rogue"/>
</p>

Earn experience from enemies, spend points on stats, and choose a class at level 10. Boss milestones unlock three tiers of passive skills. Enable Calamity for the Rogue class, the Cunning stat, and post-Moon Lord class awakenings.

## What's new in 2.0

| Feature | Terraria + RPG Mod | With Calamity Mod enabled |
| --- | --- | --- |
| Classes | Warrior, Ranger, Mage, Summoner | All four plus **Rogue** |
| Stats | 6 | 7 with **Cunning** |
| Passive skill nodes | 60 | 75, including Rogue's 15-node tree |
| Class awakenings | Hidden | Available for all five classes |
| Awakening materials | Hidden | Awakening Essence and Awakening Stone |

- **Rogue** specializes as either an **Assassin**, with powerful opening hits and critical strikes, or a **Trickster**, with stacking attack speed and evasion.
- **In the Shadows** is a Rogue skill state. With the Shadow node selected, it becomes ready after two seconds without attacking. An icon appears beside the XP bar and the next eligible Rogue hit spends the effect. Calamity's stealth mechanic continues to work alongside it.
- **Lord of Shadows** adds a fan of homing daggers, with different bonuses for Assassin and Trickster.
- **17 new pixel-art icons** cover Rogue's 15 nodes, Cunning, and Lord of Shadows. Both English and Russian include the new content.

Version 1 characters carry over to 2.0. Disabling Calamity hides a saved Rogue tree and suspends its class effects and awakenings. The class, selected skills, and awakening return when Calamity is enabled again. Points spent on Cunning return to the unspent pool when the character loads without Calamity. Resetting an awakened tree in that mode defers the Awakening Stone refund until the character next enters with Calamity.

## Levels and experience

Enemy XP is based on base enemy stats. The server calculates XP in multiplayer, and progress is saved with the character. The default XP settings are:

```text
Enemy power     = 0.6 × sqrt(max life) + 0.8 × damage + 0.5 × defense
Enemy level     = max(1, round(enemy power / 3))
Enemy base XP   = 2 × enemy power ^ 1.5
XP to next level = 300 × current level ^ 2.5
```

The level-difference, event, and server XP multipliers apply to enemy base XP. By default, enemies more than five levels below you give 10% less XP per additional level, down to 10% of base XP. Stronger enemies grant up to 50% extra XP. Events such as moons, eclipses, and invasions halve XP. Bosses use a separate level table and reward up to half the XP needed at their assigned level before contribution and server modifiers. Statue spawns, town NPCs, and critters grant no XP.

| Milestone | Character level | Boss milestone |
| --- | ---: | --- |
| Choose a class | 10 | None |
| Tier I | 25 | Wall of Flesh |
| Tier II | 50 | All three mechanical bosses |
| Tier III | 75 | Moon Lord |

## Stats

<p align="center">
  <img src="docs/readme/stats.png" alt="Stat icons for Strength, Agility, Intellect, Spirit, Cunning, Vitality, and Luck"/>
</p>

Each level grants a stat point by default. Most bonuses have diminishing returns. The formula is `maximum × points / (points + 100)`, so 25 points give 20% of the listed maximum and 100 points give 50%.

| Icon | Stat | Primary effect | Secondary effect |
| :---: | --- | --- | --- |
| <img src="Assets/StatIcons/stat_str.png" width="32" alt="Strength"/> | **Strength** | Up to +60% melee damage | Up to +20% melee attack speed |
| <img src="Assets/StatIcons/stat_agi.png" width="32" alt="Agility"/> | **Agility** | Up to +60% ranged damage | Up to +25% movement speed and 25% ammo conservation |
| <img src="Assets/StatIcons/stat_int.png" width="32" alt="Intellect"/> | **Intellect** | Up to +60% magic damage | +1 mana per point, up to +200, and up to 25% less mana cost |
| <img src="Assets/StatIcons/stat_spi.png" width="32" alt="Spirit"/> | **Spirit** | Up to +60% minion damage | +1 minion slot at 30, 80, and 150 points, and up to +30% whip speed and range |
| <img src="Assets/StatIcons/stat_cun.png" width="32" alt="Cunning"/> | **Cunning** (Calamity) | Up to +60% Rogue damage | Up to +20% Rogue attack speed |
| <img src="Assets/StatIcons/stat_vit.png" width="32" alt="Vitality"/> | **Vitality** | Up to +200 max life | Up to 15% damage reduction and +1 life regeneration per 25 points |
| <img src="Assets/StatIcons/stat_luck.png" width="32" alt="Luck"/> | **Luck** | Up to +15 percentage points of critical strike chance across classes | Up to +0.3 Terraria luck |

## Classes

<p align="center">
  <img src="docs/readme/classes.png" alt="Class icons for Warrior, Ranger, Mage, Summoner, and Rogue"/>
</p>

Classes grant a starting bonus and automatic stat growth. Growth is calculated from the current level and rounded down, so choosing a class later gives you the growth for levels you already earned.

| Icon | Class | Starting bonus | Growth per level |
| :---: | --- | --- | --- |
| <img src="Assets/SkillIcons/warrior.png" width="32" alt="Warrior"/> | **Warrior** | +5% melee damage, +20 max life | Strength 0.50 · Vitality 0.35 |
| <img src="Assets/SkillIcons/ranger.png" width="32" alt="Ranger"/> | **Ranger** | +5% ranged damage, +5% movement speed | Agility 0.60 · Vitality 0.25 |
| <img src="Assets/SkillIcons/mage.png" width="32" alt="Mage"/> | **Mage** | +5% magic damage, +20 max mana | Intellect 0.60 · Spirit 0.10 · Vitality 0.15 |
| <img src="Assets/SkillIcons/summoner.png" width="32" alt="Summoner"/> | **Summoner** | +5% minion damage, +5% whip speed | Spirit 0.55 · Intellect 0.10 · Vitality 0.20 |
| <img src="Assets/SkillIcons/rogue.png" width="32" alt="Rogue"/> | **Rogue** (Calamity) | +5% Rogue damage, +5 percentage points of Rogue critical strike chance | Cunning 0.55 · Agility 0.10 · Vitality 0.20 |

## Skill trees

Each class has 15 nodes arranged as **1 → 2 → 4 → 8**. Pick a specialization in Tier I, a defining mechanic in Tier II, and a finishing passive in Tier III. Choosing a node closes its sibling branch until you reset the tree. The illustrations highlight sample builds. Gold marks the awakened node and class awakening.

<table>
  <tr>
    <td align="center" width="50%"><b>Warrior</b><br/><img src="docs/readme/tree_warrior.png" width="100%" alt="Warrior skill tree with the Berserker path highlighted"/></td>
    <td align="center" width="50%"><b>Ranger</b><br/><img src="docs/readme/tree_ranger.png" width="100%" alt="Ranger skill tree with the Sniper path highlighted"/></td>
  </tr>
  <tr>
    <td align="center"><b>Mage</b><br/><img src="docs/readme/tree_mage.png" width="100%" alt="Mage skill tree with the Mystic path highlighted"/></td>
    <td align="center"><b>Summoner</b><br/><img src="docs/readme/tree_summoner.png" width="100%" alt="Summoner skill tree with the Beastmaster path highlighted"/></td>
  </tr>
  <tr>
    <td align="center"><b>Rogue · Assassin</b> (Calamity)<br/><img src="docs/readme/tree_rogue.png" width="100%" alt="Rogue skill tree showing Assassin, Shadow, Dusk Blade, and Lord of Shadows"/></td>
    <td align="center"><b>Rogue · Trickster</b> (Calamity)<br/><img src="docs/readme/tree_rogue_trickster.png" width="100%" alt="Rogue skill tree showing Trickster, Juggler, Ricochet, and Lord of Shadows"/></td>
  </tr>
</table>

{{NODES}}

## Class awakening

<p align="center">
  <img src="docs/readme/awakening.png" alt="Five class awakening icons, including Lord of Shadows"/>
</p>

Awakening requires Calamity and a selected Tier III skill.

1. Defeat Moon Lord to reveal the awakening slot in the skill tree.
2. Collect personal Awakening Essences from at least three different bosses. Eligible bosses are Providence, Ceaseless Void, Storm Weaver, Signus, and Polterghast.
3. At an Ancient Manipulator, combine **3 Awakening Essences**, **10 Divine Geodes**, **3 Armored Shells**, **3 Dark Plasma**, and **3 Twisting Nether** into an Awakening Stone.
4. Use the stone to awaken the selected class.

{{AWAKENINGS}}

Rogue daggers created by Lord of Shadows or Ricochet do not feed the awakening counter or trigger another round of Rogue skill effects.

## Items and respec

<p align="center">
  <img src="docs/readme/items.png" alt="Stat Reset, Tree Reset, Awakening Essence, and Awakening Stone"/>
</p>

| Icon | Item | Source | Effect |
| :---: | --- | --- | --- |
| <img src="Content/Items/StatResetItem.png" width="32" alt="Stat Reset"/> | **Stat Reset** | 1 Wood + 1 Fallen Star at a Work Bench | Refunds manually allocated stat points. |
| <img src="Content/Items/TreeResetItem.png" width="32" alt="Tree Reset"/> | **Tree Reset** | 1 Wood + 2 Fallen Stars at a Work Bench | Resets the class, selected skills, and awakening. Returns the Awakening Stone immediately if Calamity is loaded, or on the next entry with Calamity. |
| <img src="Content/Items/AwakeningEssence.png" width="32" alt="Awakening Essence"/> | **Awakening Essence** | Personal drop from eligible Calamity bosses | Material for the Awakening Stone. |
| <img src="Content/Items/AwakeningStone.png" width="32" alt="Awakening Stone"/> | **Awakening Stone** | Ancient Manipulator recipe above | Awakens the selected class. |

## Installation

1. Install [tModLoader for Terraria 1.4.4](https://store.steampowered.com/app/1281930/tModLoader/). Enable Calamity Mod if you want Rogue, Cunning, and class awakening.
2. Clone this repository into the tModLoader `ModSources` folder, usually under `Documents/My Games/Terraria/tModLoader`.

   ```bash
   git clone https://github.com/Atreu1s/terraria_rpg_mod.git RPGMod
   ```

3. In tModLoader, go to **Workshop → Develop Mods → RPGMod → Build + Reload**.
4. Open your inventory and select **Status** to see the character panel. The optional **Character window** keybind can be set in Controls.

The server config controls XP, stat scaling, unlock levels, and Rogue balance. Client options cover the character UI and HUD.

## Debug commands

Enable **Debug commands** in the server-side mod config before using these in chat.

| Command | Effect |
| --- | --- |
| `/rpg` or `/rpg points` | Show character progression and boss flags. |
| `/rpg mode` | Show whether Calamity is loaded and Rogue is available. |
| `/rpg level 75` | Set the character level. |
| `/rpg xp 10000` | Add experience. |
| `/rpg downed wof`, `mech`, `moonlord`, `all`, or `none` | Set the world's progression flags. |
| `/rpg reset` | Reset the character's RPG progression. |

## Project and documentation

- `Common/` contains progression, combat effects, persistence, networking, and Calamity compatibility.
- `Content/`, `UI/`, and `Assets/` contain in-game items, projectiles, screens, and sprites.
- `Localization/` contains the English and Russian game text.
- The [game design](docs/DESIGN.md), [2.0 design](docs/v2/DESIGN_V2.md), [UI specification](docs/UI_SPEC.md), and [3.0 roadmap](docs/ROADMAP_V3.md) contain the design details.
- To regenerate this README and its illustrations, run `npm ci --prefix tools/icons` and `node docs/readme/build.js` from the repository root. The skill and awakening tables use the English game localization and the skill registry.

Built by [Atreu1s](https://github.com/Atreu1s) with Claude Code and Codex.
