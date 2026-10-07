<div align="center">

# 🛡️ Ward Me *Bruh!*

### *Base management, role-based access, and defense controls for Valheim.*

[![GitHub Release](https://img.shields.io/github/v/release/Vapok/WardMeBruh?include_prereleases&logo=github&style=for-the-badge)](https://github.com/Vapok/WardMeBruh/releases)
[![Thunderstore Version](https://img.shields.io/thunderstore/v/Vapok/WardMeBruh?logo=thunderstore&style=for-the-badge)](https://thunderstore.io/c/valheim/p/Vapok/WardMeBruh/)
<br>
[![Discord](https://img.shields.io/badge/Discord-Join%20Community-7289da?logo=discord&logoColor=white&style=for-the-badge)](https://discord.gg/5YAJkRFBXt)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](https://opensource.org/licenses/MIT)

---

</div>

**Ward Me Bruh!** expands the vanilla Valheim ward (`guard_stone`) with role-based permissions, multi-ward compound linking, nested sub-wards, automated base upkeep, and configurable defense shields.

---

<div align="center">

<br>

[![Survival Servers](https://raw.githubusercontent.com/Vapok/WardMeBruh/main/images/survivalservers_banner.png)](https://www.survivalservers.com/services/game_servers/valheim/?ref=vapok)

</div>

## 🛡️ Features

### Ward Management & Remote Menu
* **Ward Interface**: Accessed directly at any ward stone (<kbd>E</kbd> or <kbd>G</kbd>). Tabs: Overview, Access, Defenses, Upkeep, Roles, and Management.
* **Remote Ward Menu**: View and configure all wards you own across the world from one menu. Shows bases in a hierarchy with child extensions and sub-wards grouped underneath.
* **Font Scaling**: Toggle between Small, Medium, and Large text sizes directly in the window header.

### Base Extensions & Sub-Wards
* **Placement Prompt**: Placing a new ward stone inside an active ward prompts you to choose between an **Extension Ward** or a **Sub-Ward**.
* **Extension Wards**:
  * Expands the main base's coverage area by projecting an additional shield dome.
  * Inherits roles, permissions, settings, and defenses from the main base.
  * Has its own power toggle to disable individual sectors without taking down the entire base.
  * Shared access scanner finds chests, doors, and portals across the full base complex.
* **Nested Sub-Wards**:
  * Smaller wards placed inside a larger base for private rooms, personal houses, or vaults.
  * Independent roles, member rosters, permissions, and door/chest locks.
  * Boundary radius automatically conforms to the parent base and attached extensions, preventing overlap with sibling sub-wards.
  * Custom dome colors via an interactive color wheel in the Defenses tab (changes dome, perimeter ring, and base lights).

### Roles & Access Control
* **Role Hierarchy**: Default roles are **Guest**, **Resident**, **Manager**, and **Owner**. Custom roles can be created with custom names and colors.
* **Granular Permissions**: Controls building, farming, harvesting crops, entering the dome, adjusting ward radius, and managing base settings.
* **Object Access**: Lock individual doors and chests to specific roles or character names. Rename doors and containers in-world (<kbd>Shift</kbd> + <kbd>E</kbd>).
* **Portal Gating**: Restrict portal usage by role with checks at both departure and destination wards.
* **Persistent Roster**: Tracks players who have visited the server so offline players can be assigned roles.

### Defenses & Shield Domes
* **Adjustable Radius**: Stepped slider from 10m to 100m with live area readout and visual boundary ring.
* **Intruder Defense**: Configurable knockback impulse and electrical shock damage against hostile creatures.
* **Mob & Raid Suppression**: Blocks mob spawns within the dome and cancels incoming base raids.
* **AI Camouflage**: Prevents enemies from targeting structures inside the ward area.
* **Hazard Shields**: Protects against rain weathering (virtual roof), Ashlands cinders and lava, Deep North snow buildup, and water displacement for dry building.
* **Power Deactivation**: Toggling a ward off disables shield defenses and perks while keeping access locks, auto-door, auto-fuel, and repair active.

### Base Upkeep & Perks
* **Area Repair**: Automatically repairs damaged structures within the ward area (free or resource-based).
* **Auto-Fuel**: Automatically pulls wood, coal, and fuel from designated chests to refill nearby fires, torches, and smelters.
* **Auto-Closing Doors**: Closes open doors and gates after a configurable delay (5s–30s).
* **Sanctuary Perks**: Max comfort under active shields, food timer freeze while resting, skill loss protection on death, and increased health/stamina/eitr regeneration.
* **Livestock Protection**: Prevents damage to tamed animals within the ward area.

---

## 🕹️ Controls

| Action | Control | Description |
| :--- | :--- | :--- |
| **Open Ward Menu** | <kbd>E</kbd> or <kbd>G</kbd> | Opens the ward interface at a ward stone. |
| **Quick-Name Entity** | <kbd>Shift</kbd> + <kbd>E</kbd> | Renames targeted door or chest in-world. |
| **Interact** | <kbd>E</kbd> | Standard door/chest/portal interaction (checked against role clearance). |

---

## ⚙️ Configuration

Settings are saved to `BepInEx/config/vapok.mods.wardmebruh.cfg`.

### Server Settings (Synced)
* `Enable Ward Me Bruh`: Master mod toggle (default: `true`).
* `Max Ward Radius`: Maximum allowed radius in meters (default: `100.0`).
* `Ward Separation Distance`: Minimum buffer distance between unowned wards (default: `50.0`).
* `Default Repair Threshold`: Damage percentage before area repair runs (default: `0.20`).
* `Default Auto-Close Delay`: Seconds before open doors close (default: `5.0`).
* `Free Area Repair`: When true, repairs cost no resources (default: `true`).
* `Allow Ore Fast-Travel`: Allow raw ores through ward fast travel (default: `false`).
* `Fast-Travel Combat Lockout`: Seconds out of combat required before fast travel (default: `10.0`).

### Client Settings
* `Open Ward Hub Keybind`: Shortcut key to open the interface (default: `<kbd>E</kbd>`).
* `Show Splash on Startup`: Shows mod summary window on game launch (default: `true`).
* `Enable Anonymous Telemetry`: Sends crash and version reports (default: `true`).

---

## 🔌 Developer API (`WardMeBruhAPI`)

Third-party mods can integrate directly using either:
* **Assembly Reference**: Reference `WardMeBruhAPI.dll` at compile time and bundle via `ILRepack`.
* **Reflection Client**: Drop [Docs/WardMeBruhAPI_Client.cs](https://github.com/Vapok/WardMeBruh/blob/main/Docs/WardMeBruhAPI_Client.cs) into your project for zero-dependency reflection with cached delegates.

See the [API Integration Guide](https://github.com/Vapok/WardMeBruh/blob/main/Docs/API_INTEGRATION.md) and [Class Reference](https://github.com/Vapok/WardMeBruh/blob/main/Docs/WardMeBruhAPI.md) for details.

---

## 🤝 Requirements & Compatibility

* **BepInEx**: 5.4.2350 or later
* **Jotunn**: 2.30.2 or later
* Compatible with singleplayer, local co-op, and dedicated servers.
* Non-destructive Harmony patches designed to coexist with ValheimPlus, Azu mods, and inventory management mods.

---

## 📦 Installation

* **Mod Manager (Recommended)**: Install via Gale, r2modman, or Thunderstore Mod Manager.
* **Manual**: Extract the `.zip` archive and move the `WardMeBruh` folder into `Valheim/BepInEx/plugins/`.

---

## 💬 Community

* Bug reports and feature requests: [GitHub Issues](https://github.com/Vapok/WardMeBruh/issues)
* Community discussion: [Discord](https://discord.gg/5YAJkRFBXt)
