<div align="center">

# 🛡️ Ward Me *Bruh!*

### *Territorial fortress governance, base automation, and sanctuary perks for Valheim.*

[![GitHub Release](https://img.shields.io/github/v/release/Vapok/WardMeBruh?include_prereleases&logo=github&style=for-the-badge)](https://github.com/Vapok/WardMeBruh/releases)
[![Thunderstore Version](https://img.shields.io/thunderstore/v/Vapok/WardMeBruh?logo=thunderstore&style=for-the-badge)](https://thunderstore.io/c/valheim/p/Vapok/WardMeBruh/)
<br>
[![Discord](https://img.shields.io/badge/Discord-Join%20Community-7289da?logo=discord&logoColor=white&style=for-the-badge)](https://discord.gg/5YAJkRFBXt)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](https://opensource.org/licenses/MIT)

---

</div>

**Ward Me Bruh!** overhauls the vanilla Valheim Ward (`guard_stone`) into an administrative and territorial fortress system while maintaining complete compatibility with vanilla game mechanics and engine invariants.

---

<div align="center">

<br>

[![Survival Servers](https://raw.githubusercontent.com/Vapok/WardMeBruh/main/images/survivalservers_banner.png)](https://www.survivalservers.com/services/game_servers/valheim/?ref=vapok)

</div>

## 🛡️ Features

### Centralized Management GUI & Remote Overview
* **Ward Hub GUI**: Dedicated 5-tab interface opened directly at the ward stone (`[G]` or `<E>`).
* **"Your Ward's Bruh" Overview**: Inspect and remotely configure any ward owned by your character across the world, even in unloaded sectors.
* **Perimeter Adjustment**: Dynamic radius slider (32m up to server cap) with in-world boundary preview ring.

### 4-Tier Role Governance & Feature Clearance
* **Hierarchical Tiers**: Granular authorization across **Owner**, **Manager**, **Resident**, and **Guest**.
* **Dome Access Clearance**: Configurable minimum rank required to enter the dome and gain base access (defaults to Guest).
* **Per-Feature Role Gating**: Every defense and upkeep perk has an independent role requirement (`Guest` ➔ `Resident` ➔ `Manager` ➔ `Owner`), allowing fine-grained base access control.
* **Character Independence**: Admin auto-elevation is strictly scoped to dedicated servers; singleplayer and player-hosted characters on the same account are treated as separate individuals.

### Territorial Defense & 3D Airspace
* **3D Hemispherical Dome**: Deflects both ground attackers and flying siege mobs (Drakes, Gjall, Valkyries).
* **Pushback & Shock**: Configurable physical knockback impulse and retaliatory electrical shock damage to hostile intruders.
* **"Ghost Base" AI Camouflage**: Hides structures from enemy pathfinding so mobs don't aggro on walls.
* **Raid & Mob Spawn Suppression**: Cancels base raid events and suppresses mob spawns inside the dome.

### Base Automation & Upkeep
* **Free Area Repair**: Automatically repairs damaged structures with a configurable weathering threshold slider (20%–100%).
* **Auto-Fuel Supply Depots**: Refuels nearby torches, campfires, hearths, and smelters directly from designated chests.
* **Auto-Closing Doors**: Automatically shuts open doors and gates after a configurable delay (3s–30s).

### Sanctuary Perks & Comfort
* **Max Comfort Under Shield**: Calculates the highest comfort piece in the base and applies that max comfort level and full shelter everywhere under the active dome.
* **Base Territory Status Effect**: Displays the ward icon, base name, current role, coverage radius, and active comfort on your HUD.
* **Food Decay Freeze**: Pauses food timer decay while resting inside base boundaries.
* **Death Penalty Shield**: Prevents skill point loss on death within ward borders.
* **Regeneration Boost**: Boosts health, stamina, and eitr recovery inside the base.

### Access Control (Doors, Containers & Portals)
* **Custom Naming**: Name doors and chests directly in-world (`Shift + E`) with dynamic hover text and colored chest headers.
* **Role-Locked Doors & Chests**: Set individual clearance levels per door and container.
* **Portal Network Gating**: Restrict portal use by role, with bi-directional verification checking access at both departure and destination wards.

### Biome & Environmental Shields
* **Virtual Roof**: Eliminates rain weathering on unroofed wood and structures.
* **Ashlands Shield**: Grants immunity against falling ash, despawns cinders, and blocks lava damage on structures.
* **Deep North Shield**: Prevents structural collapse from heavy snow accumulation.

### Agricultural & World Security
* **Tamed Animal Immunity**: Tamed livestock take zero damage inside the active perimeter.
* **Harvesting & Terrain Locks**: Restricts crop harvesting and terraforming (hoe, cultivator, pickaxe) to Residents and above.
* **Vehicle Locking**: Secures ship rudders and carts against unauthorized guests.
* **Underwater Construction**: Displaces water inside the dome for dry seabed building.
* **Minimap Overlay & Fast Travel**: Ward map pins with circular coverage rings and optional double-click fast travel.

---

## 🕹️ Controls Summary

| Action | Input / Control | Description |
| :--- | :--- | :--- |
| **Open Ward Hub** | <kbd>G</kbd> or <kbd>E</kbd> | Opens the WardMeBruh Central Hub GUI when targeting a ward stone. |
| **Quick-Name Entity** | <kbd>Shift</kbd> + <kbd>E</kbd> | Opens in-world renaming dialog for targeted door or chest. |
| **Interact / Open** | <kbd>E</kbd> | Standard door or container interaction (evaluated against role clearance). |

---

## ⚙️ Configuration

Configuration settings are stored in `BepInEx/config/vapok.mods.wardmebruh.cfg`.

### Server Settings (Synced)
* **Enable Ward Me Bruh**: Master mod toggle (Default: `true`).
* **Max Ward Radius**: Maximum radius in meters a ward can be expanded to (Default: `400.0m`).
* **Ward Separation Distance**: Minimum buffer distance enforced between unowned ward perimeters (Default: `50.0m`).
* **Default Repair Threshold**: Damage fraction required before area repair triggers (Default: `0.20`).
* **Default Auto-Close Delay**: Seconds before an open door automatically closes (Default: `5.0s`).
* **Free Area Repair**: If enabled, area repair consumes zero resources (Default: `true`).
* **Allow Ore Fast-Travel**: Permits transporting raw ores/metals through ward fast travel (Default: `false`).
* **Fast-Travel Combat Lockout**: Seconds out of combat required before fast travel is permitted (Default: `10.0s`).

### Local Config
* **Open Ward Hub Keybind**: Key shortcut used to open the GUI (Default: `<kbd>G</kbd>`).
* **Show Splash on Startup**: Toggles display of the mod overview window upon game launch (Default: `true`).
* **Enable Anonymous Telemetry**: Toggles anonymous telemetry reporting to track active mod versions and crashes (Default: `true`).

---

## 🤝 Compatibility & Requirements

* **BepInEx**: 5.4.2350 or later.
* **Jotunn**: 2.30.2 or later.
* Compatible with solo, player-hosted, and headless dedicated servers.
* Non-destructive Harmony patches designed for seamless coexistence with ValheimPlus, Azu mods, and inventory utilities.

---

## 📦 Installation

### Mod Manager (Recommended)
1. Install via **Gale**, **r2modman**, or **Thunderstore Mod Manager**.
2. Dependencies (`BepInExPack_Valheim`, `Jotunn`, `YamlDotNet`) download automatically.

### Manual Installation
1. Extract the downloaded `.zip` archive.
2. Place the `WardMeBruh` folder into your `Valheim/BepInEx/plugins/` directory.
3. Launch the game.

---

## 💬 Feedback & Community

* Report issues or request features on [GitHub](https://github.com/Vapok/WardMeBruh/issues).
* Join the community on [Discord](https://discord.gg/5YAJkRFBXt) for discussions and support.
