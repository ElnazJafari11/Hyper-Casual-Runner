# Idle Mechanics Matrix & Taxonomy

This document maps the top-tier idle games into a unified matrix of mechanics. It serves as the design blueprint for the Hyper-Casual Runner Toolkit's Idle Sandbox.

**Code map (components / events / UI verbs / smoke):** `docs/idle-toolkit-compose.md` — design columns here are intent; MVP bar is one core verb + one beat.

| Game | Core Verb | Prestige Model | Automation Pattern | Monetization Surface | Why Loved (Fun Factor) |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Cookie Clicker** | Click → Buy Generators | Heavenly Chips (Global Multiplier) | Buy Cursor/Grandma (CPS) | None (Browser) | The emotional arc of absurd numbers played straight; deep theorycrafting. |
| **Clicker Heroes** | Tap to Kill → Buy Heroes | Hero Souls & Ancients (Zone Resets) | Heroes auto-attack | Time skips / Premium currency | "The wall" becomes the content; clean, measurable prestige pacing. |
| **AdVenture Cap.** | Buy Businesses | Angel Investors (Planet Resets) | Hire Managers (Earned Auto) | Multipliers / Time Warps | Zero friction "number erupts" fantasy; codified the manager automation pattern. |
| **Univ. Paperclips** | Manufacture | Phase-Shifts (Total Loop Replacement) | Compute allocation | None (Browser) | Narrative via mechanics; total loop replacement as a progression beat. |
| **A Dark Room** | Stoke Fire → Explore | None (Narrative Completion) | Villagers gather resources | None | Mystery as the reward loop; minimalist atmosphere. |
| **Antimatter Dim.** | Buy Dimensions | Nested Meta-Layers (Eternity, Reality) | Dimension Autobuyers | QoL / Time | Long-arc pacing masterclass; meta-layers turn prior games into resources. |
| **Realm Grinder** | Build & Align (Factions) | Re-pick Alignment (Build Variation) | Faction-specific spells | Time warps / Rubies | Replay value from horizontally different strategies, not just bigger numbers. |
| **NGU Idle** | Allocate Energy | Rebirth (Stat retention) | Auto-allocators | QoL / Buffs | Feature-sprawl constant novelty; always a new system unlocking. |
| **Melvor Idle** | Grind Skills | None (Linear progression sandbox) | Total Offline Progression | Premium Unlock | Player-directed goals (RuneScape style); respects player time. |
| **Egg, Inc.** | Hatch (Tap Burst) | Soul Eggs (Permanent Research) | Habitats / Vehicles | Piggy Bank / Golden Eggs | Silky pacing; Co-op Contracts turned a solo genre social. |
| **Idle Miner Tycoon** | Upgrade Shafts/Elevators | Prestige Mines (Cash Multiplier) | Super-Managers | Live-ops / Gacha Managers | Spatial optimization; bottleneck-hunting is genuinely engaging. |
| **Tap Titans 2** | Tap / Hero DPS | Relics & Artifacts | Heroes attack passively | Gacha Pets / Tournaments | Respects active (tapping) and passive play; Clan raids add social stakes. |
| **Idle Heroes** | Auto-Combat | None (Gacha progression) | 100% Auto (AFK Chest) | Hero Dupe Gacha | Infinite chase depth via hero collection meta. |
| **AFK Arena** | Auto-Combat | None (Campaign progression) | 100% Auto (AFK Chest) | Hero Dupe Gacha | Gorgeous polish; true "progress while away" marketed fantasy. |
| **Legend of Mushroom**| Rub Lamp (Gacha Gear)| None (Linear stage push) | Auto-Lamp rubbing | Premium Passes / Mats | Collapsed idle progression and gacha pulls into a single compulsive verb. |
| **Capybara Go!** | Step-based Narrative | None (Run-based Roguelite) | Auto-advancing tiles | Pet/Gear Gacha | Progression feels like a story you watch; high shareability. |
| **Cats & Soup** | Assign Cats to Stations | None (Facility unlocking) | Cats auto-cook | Cosmetics / Ad-skips | ASMR audio; monetizes affection and decoration instead of power. |
| **Neko Atsume** | Place Food/Toys | None | Zero (Pure passive check-in) | Golden Fish | Zen endpoint; anticipation and surprise; absence as a mechanic. |
| **Fallout Shelter** | Assign Dwellers | None (Base survival) | Dwellers auto-produce | Lunchboxes (Gacha) | IP affection; "check in or risk losses" tension rarely seen in idles. |

## Matrix coverage gap (candidate #20)

All 19 rows above have EditMode-verified MVP slices. The next expansion slot (not implemented) fills a missing **second-axis** pattern:

| Candidate | Core Verb | Why it fills a gap |
| :--- | :--- | :--- |
| **Synergism** (deferred) | Buy upgrades → nested prestige (challenges / achievements as buyable) | Matrix already covers nested meta-layers (Antimatter) and factions (Realm Grinder), but not **achievement-gated buyables** that turn completionism into a generator. Compose path: new `IdleArchetype` + `IdleBuyGeneratorEvent` gated by `ProgressionLevel` flags — see `docs/idle-toolkit-compose.md`. |

Do not implement #20 until Play Mode smoke clears for Batch A on Hyper-Casual-Runner.

## 5 Pillars of Design DNA

1. **Prestige as Replay Acceleration**: Every reset must be measurably faster.
2. **Automation as an Earned Graduation**: Managers/Heroes remove labor you first performed manually.
3. **A Second Axis Beyond Bigger Numbers**: Builds, skills, narrative, or cosmetic collection.
4. **Absence Made Valuable**: Offline rewards framed as the fantasy, not a penalty.
5. **Monetization Aligned with Emotion**: E.g., Power gacha in combat idles, cosmetics in cozy idles (we focus strictly on gameplay mechanics).
