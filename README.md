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
* **Ward Hub GUI**: Dedicated 6-tab interface opened directly at the ward stone (`[G]` or `<E>`) featuring **Overview**, **Access**, **Defenses**, **Upkeep**, **Roles**, and **Management**.
* **Player Capability Summary**: Overview tab displays current character clearance tier alongside a comprehensive, real-time list of all functional capabilities granted to that role.
* **Dynamic Font Size Scaling**: Instant toggle button in the window header to cycle between **Small** (default), **Medium**, and **Large** font sizes across all tabs and modal inspectors.
* **"Your Ward's Bruh" Overview**: Inspect and remotely configure any ward owned by your character across the world, even in unloaded sectors.

### Seamless Base Extensions (Multi-Ward Linking)
* **Satellite & Extension Wards**: Building an additional ward stone inside an active shield automatically links it as a **Base Extension** rather than creating a separate ward.
* **Unified Base Territory**: Extension wards project their own forcefield dome, boundary, and in-range effects, expanding the total protected perimeter of the base.
* **Shared Governance & Power**: Extension stones share the parent base's settings, custom roles, permissions, name, and power state. Toggling power or interacting with any ward stone in the complex opens and operates the single unified base matrix.
* **Consolidated Access Scanner**: The Access tab scans and manages chests, doors, and portals across the parent base and all of its connected extensions.
* **Clean Remote Registry**: Extension wards do not clutter the "Your Ward's Bruh" remote selector, keeping base navigation clean and organized.


### Extensible Role-Based Governance & Capability Management
* **Dedicated MANAGEMENT Tab**: Separate administrative governance from gameplay settings. Control who has the authority to configure which systems, add custom roles, or edit settlement parameters.
* **Custom & Per-Ward Roles**: Define custom roles per ward stone with customized display names, hierarchy levels, and color badges on top of the default slate (**Guest**, **Resident**, **Manager**, **Owner**).
* **Hierarchical Permission Inheritance**: Higher-tier roles automatically inherit all capabilities granted to subordinate roles. The Management matrix displays active grants (`[ ✔ ]`) alongside greyed-out inherited grants (`[ ✔ ] (Inherited from [LowerRole])`).
* **Operational vs. Administrative Separation**: Door, container, and portal clearance requirements remain strictly per-object, while the Management tab governs which roles are authorized to configure those objects or activate fortress defenses.
* **Granular Core & Defense Clearances**:
  * **Dome Access Clearance** (`core.dome_access`, default: Guest): Controls who can cross the forcefield perimeter and enter the base without being repelled or shocked.
  * **Shield Sanctuary Benefits** (`core.benefits`, default: Resident): Controls who receives base territory perks (HUD status display, max comfort, food decay pause, death skill shield, and health/stamina/eitr regen boost).
  * **Building & Construction** (`core.building`, default: Resident): Controls who is authorized to construct, place, repair, and deconstruct pieces, structures, and furniture within the protected perimeter.
  * **Farming & Cultivation** (`core.farming`, default: Resident): Controls who is authorized to cultivate soil, replant grass, and plant seeds, crops, and saplings using the Cultivator.
  * **Harvest Crops & Pickables** (`core.harvest`, default: Resident): Controls who is authorized to harvest mature crops, berry bushes, mushrooms, and pickable resources within the protected perimeter.
  * **Perimeter Dome Size** (`defenses.dome_size`, default: Owner): Controls who is authorized to adjust the ward shield perimeter and coverage radius.
* **Persistent World Player Registry**: Keeps an active, world-level tally of all characters that have joined the world so that offline players remain available in the Roster enrollment dropdown across sessions.
* **Character Independence**: Admin auto-elevation is strictly scoped to dedicated servers; singleplayer and player-hosted characters on the same account are treated as separate individuals.

### Territorial Defense & 3D Airspace
* **Territorial Domain & Coverage**: Stepped dome radius slider (10m–100m in 5m notched increments with visual hashmarks) with real-time square meter land coverage readout and in-world boundary marker.
* **3D Hemispherical Dome**: Deflects both ground attackers and flying siege mobs (Drakes, Gjall, Valkyries).
* **Pushback & Shock**: Configurable physical knockback impulse and retaliatory electrical shock damage to hostile intruders.
* **"Ghost Base" AI Camouflage**: Hides structures from enemy pathfinding so mobs don't aggro on walls.
* **Raid & Mob Spawn Suppression**: Cancels base raid events and suppresses mob spawns inside the dome.
* **Dome Power Deactivation**: Toggling the ward dome off disables all shield defenses (pushback, shock, camouflage, raid/spawn suppression, hazard shields) and dome status perks (faked fire/shelter, forced max comfort, food decay pause, skill shield, regen boost). Standard vanilla rules apply for fire, shelter, and resting.
* **Continuous Upkeep & Access Protection**: Base automations (Area Repair, 🔥 Auto-Fuel, Auto-Door) and security access settings (Door clearances, Chest locks, Portal gating, Building permissions, and Harvesting/Farming restrictions) remain active and enforced even when the dome is powered down.

### Base Automation & Upkeep
* **Free Area Repair**: Automatically repairs damaged structures with a configurable weathering threshold slider (20%–100% in 5% notched increments).
* **🔥 Auto-Fuel Supply Depots**: Refuels nearby torches, campfires, hearths, and smelters directly from designated chests.
* **Auto-Closing Doors**: Automatically shuts open doors and gates after a configurable delay (5s–30s in 5s notched increments).

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
* **Virtual Roof**: Eliminates rain weathering on unroofed wood and structures, shields players from rain exposure, and prevents unroofed fires from extinguishing.
* **Ashlands Shield**: Grants immunity against falling ash, despawns cinders, and blocks lava damage on structures.
* **Deep North Shield**: Prevents structural collapse from heavy snow accumulation.

### Agricultural & World Security
* **Tamed Animal Immunity**: Tamed livestock take zero damage inside the active perimeter.
* **Harvesting & Farming Clearances**: Restricts crop harvesting and cultivating/planting seeds to authorized roles via granular role capabilities.
* **Vehicle Locking**: Secures ship rudders and carts against unauthorized guests.
* **Underwater Construction**: Displaces water inside the dome for dry seabed building.
* **Minimap Overlay & Fast Travel**: Ward map pins with circular coverage rings and optional double-click fast travel.

### 🔌 Extensibility & Developer API (`WardMeBruhAPI`)
* **Register External Role Features**: Third-party mods can register custom capabilities using `WardMeBruhAPI.RegisterRoleFeature(...)`. Registered features appear dynamically in the **MANAGEMENT** tab under *External Mod Integrations*, allowing ward administrators to toggle them on or off per role (Guest, Resident, Manager, Owner, or custom roles).
* **Player Capability Reflection**: Granted external capabilities display automatically under *Granted Role Capabilities* in the **OVERVIEW** tab for players possessing that clearance.
* **Permission & Territory Queries**: Fast spatial queries via `WardMeBruhAPI.HasFeatureAccess(...)`, `GetPlayerRole(...)`, `IsInsideWard(...)`, and `IsWardShieldActive(...)`.
* **External Role Templates**: Register custom role templates with `WardMeBruhAPI.RegisterRoleTemplate(...)` to populate initial ward hierarchies.
* **Zero Hard Dependencies**: External developers can either reference `WardMeBruh.dll` directly or drop the reflection wrapper [Docs/WardMeBruhAPI_Client.cs](file:///home/vapok/Modding/Valheim/WardMeBruh/Docs/WardMeBruhAPI_Client.cs) into their project.
* **API Documentation**: Detailed method reference, parameters, and return types are documented in [Docs/WardMeBruhAPI.md](file:///home/vapok/Modding/Valheim/WardMeBruh/Docs/WardMeBruhAPI.md).

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
* **Max Ward Radius**: Maximum radius in meters a ward can be expanded to (Default: `100.0m`).
* **Ward Separation Distance**: Minimum buffer distance enforced between unowned ward perimeters (Default: `50.0m`).
* **Default Repair Threshold**: Damage fraction required before area repair triggers (Default: `0.20`).
* **Default Auto-Close Delay**: Seconds before an open door automatically closes (Default: `5.0s`).
* **Free Area Repair**: If enabled, area repair consumes zero resources (Default: `true`).
* **Allow Ore Fast-Travel**: Permits transporting raw ores/metals through ward fast travel (Default: `false`).
* **Fast-Travel Combat Lockout**: Seconds out of combat required before fast travel is permitted (Default: `10.0s`).

### Local Config
* **Open Ward Hub Keybind**: Key shortcut used to open the GUI (Default: `<kbd>E</kbd>`).
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
