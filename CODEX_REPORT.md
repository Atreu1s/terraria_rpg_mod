# RPGMod implementation report

Implemented stages 4 (Mage/Summoner), 5, and 7. All implementation files are new. Existing code, documentation, configuration, project/build files, and PNG files were not edited. The only existing files edited are the two localization files. No git commit was made.

## Stage 4: Mage

Implemented in `Common/Players/Nodes/MageNodePlayer.cs`, with class-guarded node checks and balance constants at the top.

| Node | Implementation |
| --- | --- |
| `overload` | Magic damage increases by the missing mana fraction, capped at +25%; never becomes a penalty when current mana temporarily exceeds maximum mana. |
| `elemental` | Magic hits have a 15% chance to apply one randomly selected debuff: On Fire, Frostburn, or Poisoned. |
| `flow` | Above 70% mana: +10% magic damage and +8% magic attack speed. |
| `meditation` | While stationary: doubles actual mana regeneration and adds 2 HP/s regeneration. Uses the mana-regeneration routine, so vanilla cannot overwrite an earlier change to `manaRegen`. |
| `blood_magic` | Supplies missing spell mana for 2 HP per missing mana; leaves existing mana to pay its part. Affordability probes do not spend HP. The actual cast pays the life cost. |
| `resonance` | Every fifth magic hit doubles final damage. |
| `catalyst` | Magic damage against an NPC with an active debuff increases by 15%. |
| `chain_reaction` | A magic killing hit spreads the victim's active debuffs, preserving remaining durations, to hostile targets within 160 pixels (10 tiles). |
| `mana_shield` | Diverts 20% of incoming final damage into available mana, capped by current mana. |
| `overflow` | Removes the mana-regeneration delay during attacks and reduces mana cost by another 10%. |
| `enlightenment` | Three stationary seconds charge the next spell for +40% damage. The bonus is applied when firing, so the spell's projectiles retain it after the charge is consumed. |
| `mana_blood` | Actual mana consumption accumulates; every 100 mana heals 10 HP. |

Mana fallback gives awakening priority over Blood Magic. Healing and debuff propagation run on the owning client. Transient node state is cleared when the relevant node/class is removed or the player dies.

## Stage 4: Summoner

Implemented in `Common/Players/Nodes/SummonerNodePlayer.cs`. Its per-NPC `SummonerStackNPC` uses `InstancePerEntity`, with separate stacks and marks for each player.

| Node | Implementation |
| --- | --- |
| `pack` | +2% minion damage per active slot-bearing minion. |
| `thousand_bites` | Minion and minion-shot hits add a target stack, up to 10; stacks refresh for 3 seconds and give +1% minion damage per stack to that player's attacks. |
| `whip_master` | +20% whip attack speed; a whip hit grants +10% minion damage for 5 seconds. |
| `bond` | At least one active minion grants +12% player damage reduction. |
| `legion` | +1 minion slot. |
| `unity` | Five or more active minions grant +10% player damage reduction. |
| `hive` | A target with 10 stacks takes +10% damage from all sources; multiple Hive owners do not multiply the same vulnerability. |
| `devourer` | A target dying with at least five of the player's stacks heals that player for 5 HP and grants a 5-second +5% minion-damage stack, up to three. The killing minion hit can supply the fifth stack. |
| `sic_em` | +15% minion damage to targets tagged by that player's whip; follows the active whip-tag duration rather than an invented fixed duration. |
| `pack_heal` | A whip hit heals 1 HP per active minion, at most once per second. |
| `alpha` | +20% minion damage when all active slot-bearing minions have one projectile type. |
| `sacrifice` | With minions present, a lethal hit dismisses all owned minion projectiles and leaves 1 HP, with a 3-minute cooldown. |

Minion counts exclude pets, sentries, minion shots, and zero-slot visual/body segments. Minion-shot damage still receives minion bonuses. Healing and sacrifice execute only on the owning client.

The same file contains `SummonerNodeNetwork`. It synchronizes stack/mark snapshots through a reserved mod-packet discriminator and relays them through the server. The documented `ModSystem.HijackGetData` hook handles only that discriminator and restores the reader for all other packets. This avoids changing the existing packet handler. Sparse `GlobalNPC.SendExtraAI` data supports late joiners without adding full player arrays to every NPC update.

## Stage 5: respec items

| Item | Implementation |
| --- | --- |
| `StatResetItem` | Consumable with potion-style use; calls `ResetStats()` locally and prints a localized message. Work Bench recipe: 1 Wood + 1 Fallen Star. |
| `TreeResetItem` | Consumable with potion-style use; calls `ResetTree()` locally. If it returns true, refunds exactly one `AwakeningStone` using `QuickSpawnItem`, including inventory-overflow handling. Prints a localized message. Work Bench recipe: 1 Wood + 2 Fallen Stars. |

## Stage 7: awakening

Added `Common/Players/AwakeningPlayer.cs`, `Common/NPCs/AwakeningDropNPC.cs`, both awakening items, `Content/Projectiles/AwakeningBolt.cs`, and `UI/AwakeningHud.cs`.

- `AwakeningEssence`: a stackable material. The server sends a private instanced item to every active player after an eligible boss kill. The item carries the boss source through `ModItem.NetSend/NetReceive`. Each receiving client accepts only the first drop for that source and removes duplicates before pickup. Single-player skips already claimed sources before spawning.
- Source history is saved as stable boss names in the character's `AwakeningPlayer` TagCompound. Missing/malformed tags, unknown names, and duplicates are tolerated. History does not require a new player-state packet because acceptance is performed where the character save lives.
- Once at least one source is recorded, the essence tooltip displays the complete stone recipe. Crafting also requires three recorded boss sources, preventing three essences from one source/trading alone from replacing the different-boss requirement.
- `AwakeningStone`: 3 essences + 10 Divine Geode + 3 Armored Shell + 3 Dark Plasma + 3 Twisting Nether at an Ancient Manipulator. All Calamity content is resolved with `TryGetMod`/`TryFind`; no Calamity assembly reference is used. Without Calamity, drops and this recipe are absent.
- Stone use requires a selected tier-3 node belonging to the current class and an unawakened player. It sets `RPGPlayer.Awakened`, creates a golden dust flash/light, plays a sound, prints the localized awakening name, and consumes the stone. Existing RPGPlayer synchronization handles the flag.

| Class | Awakening implementation |
| --- | --- |
| Warrior | Melee hits build Fervor. Full Fervor activates 8 seconds of +30% melee damage and +20% melee attack speed. During the effect, Berserker heals for 2% of damage and Guardian receives +15% damage reduction. The small bar under the player uses #1A2148 / #4656A8 / #F2C14E and shows charge or remaining active duration. |
| Ranger | Every tenth weapon firing event launches a homing bolt at 300% shot damage. Sniper changes the interval to six; Gunner launches three bolts at 100% each. Bolts use a vanilla texture and do not recursively trigger weapon-shot counting. |
| Mage | Exhausting mana, or attempting a spell without enough mana, activates 5 seconds of free magic casts with a 45-second cooldown measured from activation. Arcanist gains +25% magic damage during it; Mystic heals 20% maximum HP once on activation. |
| Summoner | +1 minion slot, plus another for Swarm Lord. Minion/minion-shot hits can crit using the player's generic crit chance (including Luck); Beastmaster's critical multiplier is ×2.5. Whips do not receive this awakening crit effect. |

## Verified Calamity names

Checked the actual class declarations on `CalamityTeam/CalamityModPublic`, branch `1.4.4`:

| Internal name | Source path |
| --- | --- |
| `DivineGeode` | [Items/Materials/DivineGeode.cs](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/Items/Materials/DivineGeode.cs) |
| `ArmoredShell` | [Items/Materials/ArmoredShell.cs](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/Items/Materials/ArmoredShell.cs) |
| `DarkPlasma` | [Items/Materials/DarkPlasma.cs](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/Items/Materials/DarkPlasma.cs) |
| `TwistingNether` | [Items/Materials/TwistingNether.cs](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/Items/Materials/TwistingNether.cs) |
| `Providence` | [NPCs/Providence/Providence.cs](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/NPCs/Providence/Providence.cs) |
| `CeaselessVoid` | [NPCs/CeaselessVoid/CeaselessVoid.cs](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/NPCs/CeaselessVoid/CeaselessVoid.cs) |
| `StormWeaverHead` | [NPCs/StormWeaver/StormWeaverHead.cs](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/NPCs/StormWeaver/StormWeaverHead.cs) |
| `Signus` | [NPCs/Signus/Signus.cs](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/NPCs/Signus/Signus.cs) |
| `Polterghast` | [NPCs/Polterghast/Polterghast.cs](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/NPCs/Polterghast/Polterghast.cs) |

The instanced-drop implementation follows the server placeholder/private-message/removal pattern in tML's `NPC.DropItemInstanced` and `CommonCode.DropItemLocalPerClientAndSetNPCMoneyTo0`. Relevant primary sources:

- [NPC.cs.patch](https://github.com/tModLoader/tModLoader/blob/1.4.4/patches/tModLoader/Terraria/NPC.cs.patch)
- [CommonCode.cs.patch](https://github.com/tModLoader/tModLoader/blob/1.4.4/patches/tModLoader/Terraria/GameContent/ItemDropRules/CommonCode.cs.patch)
- [Item.cs.patch: instanced-item documentation](https://github.com/tModLoader/tModLoader/blob/1.4.4/patches/tModLoader/Terraria/Item.cs.patch)
- [NetMessage.cs.patch](https://github.com/tModLoader/tModLoader/blob/1.4.4/patches/tModLoader/Terraria/NetMessage.cs.patch) and [MessageBuffer.cs.patch](https://github.com/tModLoader/tModLoader/blob/1.4.4/patches/tModLoader/Terraria/MessageBuffer.cs.patch): mod item data is included in item messages.
- [Player.TML.cs](https://github.com/tModLoader/tModLoader/blob/1.4.4/patches/tModLoader/Terraria/Player.TML.cs): `CheckMana` distinguishes affordability checks from actual consumption.
- [ModNet.cs](https://github.com/tModLoader/tModLoader/blob/1.4.4/patches/tModLoader/Terraria/ModLoader/ModNet.cs) and [Mod.cs](https://github.com/tModLoader/tModLoader/blob/1.4.4/patches/tModLoader/Terraria/ModLoader/Mod.cs): verified packet header encoding for the isolated Summoner receiver.

## Validation and implementation choices

- Ran the requested tModLoader compiler after stage 4, after stage 5, and repeatedly through stage 7 and integration fixes. Final build output:

```text
Reading properties: RPGMod
Building: RPGMod
Compiling RPGMod.dll
Compilation finished with 0 errors and 0 warnings
Packaging: RPGMod
```

- Parsed both localization files using the installed tModLoader Hjson library. Each contains 320 flattened keys; their key sets are identical. The stone tooltip's newline escape was verified. No existing localization key was renamed or removed.
- No in-game playthrough or live multiplayer test was performed. Compilation verifies the installed 1.4.4 APIs, not gameplay timing or interactions with every Calamity weapon/minion.
- DESIGN does not specify Fervor gain/threshold or elemental duration. Chosen constants: 5 Fervor per hit, 100 maximum (20 hits), and 3-second elemental debuffs. Fervor does not decay outside its active effect. Fractional lifesteal is accumulated.
- Blood Magic refuses casts that would leave less than 1 HP. Mana Shield retains at least 1 point of damage because Terraria's final hurt damage cannot be zero through that modifier.
- Numbers remain constants at the top of the new classes, matching the requested Warrior/Ranger pattern. Exposing them through the existing `RPGConfig` would require an expressly forbidden edit; that remains a follow-up if desired.
- Essence sources are claimed when the private world drop is first processed, not when picked up. Leaving a personal essence behind does not allow farming the same boss again. Source history survives respec, death, world changes, and `/rpg reset`; the existing debug command resets RPGPlayer only. Use a fresh character to repeat first-drop tests.
- The Summoner packet receiver is deliberately scoped to tML 1.4.4's verified header format. No existing networking code needed modification.

## In-game test checklist

Enable **Debug commands** in the RPG mod config. Use `/rpg level 50` and `/rpg downed mech` for tier 2; `/rpg level 75` and `/rpg downed all` for all tiers. `/rpg downed wof`, `/rpg downed moonlord`, and `/rpg downed none` let you check individual gates. `/rpg xp 1000` exercises normal XP progression. `/rpg reset` resets existing RPG progression; it does not erase essence source history. Use Tree Reset between mutually exclusive routes.

### Stage 4 — Mage

- Arcanist → Overload → Blood Magic / Resonance: compare full/low-mana damage; cast with partial mana and verify exactly 2 HP per missing mana and no HP loss merely from a failed affordability check; verify every fifth magic hit doubles damage.
- Arcanist → Elemental → Catalyst / Chain Reaction: observe random debuffs, increased damage against debuffed enemies, and spread to nearby enemies on a magic kill. Check a target beyond 10 tiles remains unaffected.
- Mystic → Flow → Mana Shield / Overflow: compare above/below 70% mana; take damage with full, partial, and empty mana; verify continuous mana regeneration while attacking with Overflow.
- Mystic → Meditation → Enlightenment / Mana Blood: compare stationary/moving mana regeneration and +2 HP/s; charge for 3 seconds and cast a multi-projectile spell; spend 100 and 200 mana to check healing thresholds.

### Stage 4 — Summoner

- Swarm Lord → Pack → Legion / Unity: summon 1, 4, and 5 minions; check damage, extra slot, and the five-minion reduction threshold. Include fractional-slot minions and a segmented minion to check counting.
- Swarm Lord → Thousand Bites → Hive / Devourer: reach 10 target stacks, wait more than 3 seconds, and compare damage. Kill targets at 4/5 stacks, including a killing hit that adds the fifth; check 5 HP healing and three damage stacks expiring after 5 seconds.
- Beastmaster → Whip Master → Sic ’Em / Pack Heal: verify whip speed, the 5-second minion bonus, tag-based target damage, and at most one pack heal per second.
- Beastmaster → Bond → Alpha / Sacrifice: compare no minions, one minion type, and mixed types. Take a lethal hit with minions: verify 1 HP, all minions dismissed, and no second rescue during the next 3 minutes.
- In two-player multiplayer, let one player build Hive stacks and the other attack with another damage class. Check the vulnerability, stack expiration, late joining, and Devourer healing when a teammate supplies the killing hit. Confirm heals occur once on the correct player.

### Stage 5

- Craft both reset items at a Work Bench with the exact stated ingredients. Assign manual stats, use Stat Reset, and confirm refunds while level/class growth remain.
- Pick a full tree and use Tree Reset: class and selections disappear, awakening clears, and an awakened character receives exactly one stone. Repeat while unawakened: no stone refund. Check a full inventory and multiplayer synchronization.

### Stage 7

- With Calamity enabled, kill each of the five listed bosses. Confirm one private essence per active player, no repeat drop from the same boss, and persistence after reconnecting/changing worlds. In multiplayer, include players with different prior source histories and a player who did not damage the boss.
- On a fresh character, confirm the recipe hint appears after the first source; crafting remains unavailable until three different sources have been recorded. Craft at an Ancient Manipulator and verify all ingredient counts. The `/rpg` command opens tier gates but does not grant essences or summon these bosses.
- Attempt stone use before picking tier 3, then after picking tier 3, and again while awakened. Verify consumption only on valid use, golden flash/sound/chat, save/reload persistence, and a stone refund on tree reset.
- Warrior: land 20 melee hits, verify the 8-second buff/bar; repeat with Berserker and Guardian for their respective bonuses.
- Ranger: count six Sniper firing events and ten Gunner firing events; verify one 300% homing bolt versus three 100% bolts, with no recursive extra firing.
- Mage: exhaust mana and cast for 5 seconds; verify the 45-second cooldown, Arcanist damage, Mystic's single 20% heal, and awakening taking precedence over Blood Magic.
- Summoner: check awakening slots for both subclasses; increase generic crit chance through Luck/equipment and verify minion crits and Beastmaster's ×2.5 multiplier.
- Disable Calamity and reload: RPGMod should load with no awakening drops/stone recipe. Existing essence history and the respec items should remain valid. Check both English and Russian item names, messages, tooltips, and the Fervor label.
