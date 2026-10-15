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

### ᚱ Runic Attunement (Progression)
WardMeBruh includes an optional **Runic Attunement** progression system that is **enabled by default**. When active, newly placed wards start at Rank 0 as basic boundary markers. Players unlock advanced features, base defenses, automated upkeep, and expanded boundary radii by attuning up to 8 Elder Futhark runes at the ward monument.

> **Optional System**: If you prefer vanilla-style instant access where every ward has all features and maximum radius unlocked from the start, you can disable progression by setting `Enable Runic Attunement Progression = false` in `vapok.mods.wardmebruh.cfg` (or via Configuration Manager). When disabled, the attunement tab and physical pedestals are hidden, and all wards operate with full functionality immediately.

* **Progression Modes** (`Progression Mode` in config):
  * **Hybrid** (Default): Requires both crafting the physical Keystone and having defeated the corresponding world boss.
  * **RunicKeystones**: Requires physical craftable keystone items only.
  * **WorldBossKeys**: Unlocks automatically based on defeated world boss keys without requiring physical keystones.
* **Physical Pedestals**: Attuning runes spawns stone pedestals in a circle around the ward monument with glowing, floating runestones. Automatically hidden when progression is disabled.
* **Keystones**: Crafted at designated crafting stations. Each weighs 200 units by default—plan your hauling accordingly (or use an Adventure Backpack, which the attunement interface supports directly).

#### The 8 Runes & Keystone Recipes

| Rune | Symbol | Keystone | Station | Default Recipe | Required Boss | Max Radius |
| :---: | :---: | :---: | :--- | :--- | :--- | :---: |
| **Fehu** | `ᚠ` | <img src="https://raw.githubusercontent.com/Vapok/WardMeBruh/main/images/keystones/keystone_fehu.png" width="32" height="32" alt="Fehu"/> | Workbench (Lv 2) | 1 Eikthyr Trophy, 20 Flint, 40 Fine Wood | Eikthyr | 25m |
| **Uruz** | `ᚢ` | <img src="https://raw.githubusercontent.com/Vapok/WardMeBruh/main/images/keystones/keystone_uruz.png" width="32" height="32" alt="Uruz"/> | Forge (Lv 1) | 1 Elder Trophy, 20 Bronze, 10 Surtling Core | The Elder | 40m |
| **Thurisaz** | `ᚦ` | <img src="https://raw.githubusercontent.com/Vapok/WardMeBruh/main/images/keystones/keystone_thurisaz.png" width="32" height="32" alt="Thurisaz"/> | Forge (Lv 3) | 1 Bonemass Trophy, 20 Iron, 10 Guck | Bonemass | 55m |
| **Isa** | `ᛁ` | <img src="https://raw.githubusercontent.com/Vapok/WardMeBruh/main/images/keystones/keystone_isa.png" width="32" height="32" alt="Isa"/> | Forge (Lv 4) | 1 Moder Trophy, 20 Silver, 10 Freeze Gland | Moder | 70m |
| **Sowilo** | `ᛋ` | <img src="https://raw.githubusercontent.com/Vapok/WardMeBruh/main/images/keystones/keystone_sowilo.png" width="32" height="32" alt="Sowilo"/> | Forge (Lv 6) | 1 Yagluth Trophy, 20 Black Metal, 20 Tar | Yagluth | 80m |
| **Laguz** | `ᛚ` | <img src="https://raw.githubusercontent.com/Vapok/WardMeBruh/main/images/keystones/keystone_laguz.png" width="32" height="32" alt="Laguz"/> | Galdr Table (Lv 1) | 1 Queen Trophy, 20 Refined Eitr, 20 Black Marble | The Queen | 90m |
| **Dagaz** | `ᛞ` | <img src="https://raw.githubusercontent.com/Vapok/WardMeBruh/main/images/keystones/keystone_dagaz.png" width="32" height="32" alt="Dagaz"/> | Black Forge (Lv 2) | 1 Fader Trophy, 20 Flametal, 20 Blackwood | Fader | 100m |
| **Hagalaz** | `ᚺ` | <img src="https://raw.githubusercontent.com/Vapok/WardMeBruh/main/images/keystones/keystone_hagalaz.png" width="32" height="32" alt="Hagalaz"/> | Black Forge (Lv 3) | 1 Frozen King Drop, 10 Frost Core, 20 Frostwood, 20 Gold Ore | The Frozen King | 120m |

*(Note: Feature unlocks per rune are fully configurable in `vapok.mods.wardmebruh.cfg`. Defaults are left for you to discover in-game).*

#### Ward Features Overview

| In-Game Feature | Category | Description |
| :--- | :--- | :--- |
| **Piece Auto-Repair** | Upkeep | Scans and repairs damaged pieces in the ward down to a configurable health threshold (consumes chest resources or free via server config). |
| **Auto-Fueling** | Upkeep | Pulls wood, coal, resin, and fuels from linked chests to keep fires, kilns, smelters, and torches lit. |
| **Auto-Closing Doors** | Upkeep | Closes open doors and gates inside the ward after a configurable delay (default 5s). |
| **Water Drain** | Upkeep | Displaces water inside the shield dome so you can build dry basements below sea level. |
| **Sanctuary Perks** | Upkeep | Max base comfort under the dome, freezes food timers while resting, and prevents skill loss on death. |
| **Regen Boost** | Upkeep | +50% regeneration bonus for health, stamina, and eitr while inside active ward coverage. |
| **Crop Protection** | Defenses | Prevents unauthorized players or wandering mobs from trampling farm crops. |
| **Pickable Protection** | Defenses | Locks wild pickable resources (berries, mushrooms, flowers) inside the perimeter from unauthorized harvesting. |
| **Livestock Protection** | Defenses | Shields tamed animals from enemy damage and friendly fire inside the ward area. |
| **Weather Roof** | Defenses | Prevents rain from decaying exposed wood and structure pieces without requiring a physical roof. |
| **Pushback Force** | Defenses | Repels intruders backward when they attempt to penetrate the shield dome. |
| **Creature Shock** | Defenses | Zaps hostile mobs that breach the boundary with electrical lightning damage. |
| **Player Shock** | Defenses | Electrocutes unauthorized players attempting to enter without clearance. |
| **Suppress Monster Spawns** | Defenses | Blocks all creature spawning inside the ward boundary, including natural world spawns, greydwarf nests, and draugr piles. |
| **Suppress Raids** | Defenses | Automatically cancels incoming base raid events targeting the area. |
| **Ghost Base (Camouflage)** | Defenses | Masks structures so enemy AI does not aggro or target base buildings. |
| **Environmental Shields** | Defenses | Blocks Ashlands cinder rain, lava fire, and Deep North freezing snow buildup. |
| **Portal Access Control** | Access | Restricts portal usage by player role, checked at both departure and destination wards. |
| **Fast Travel** | Access | Teleports players directly between discovered wards from the ward interface. |
| **Base Extensions & Sub-Wards** | Structure | Compound linking for expanding base coverage or creating nested private rooms with independent access rosters. |

### Ward Management & Remote Menu
* **Ward Interface**: Accessed directly at any ward stone (<kbd>E</kbd> or <kbd>G</kbd>). Tabs: Overview, Access, Defenses, Upkeep, Roles, and Management.
* **Remote Ward Menu**: View and configure all wards you own across the world from one menu. Shows bases in a hierarchy with child extensions and sub-wards grouped underneath.
* **Font Scaling**: Toggle between Small, Medium, and Large text sizes directly in the window header.
* **Full Controller Support**: Complete gamepad navigation across the Ward Hub, entity configuration dialogs, and modals with contextual on-screen keyhints.

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
* **Server Admins**: Recognized with Owner authority across all wards on the server for administration and moderation.
* **Granular Permissions**: Controls building, farming, harvesting crops, entering the dome, adjusting ward radius, and managing base settings.
* **Object Access**: Lock individual doors and chests to specific roles or character names. Rename doors and containers in-world (<kbd>Shift</kbd> + <kbd>E</kbd>).
* **Portal Gating**: Restrict portal usage by role with checks at both departure and destination wards.
* **Persistent Roster**: Tracks players who have visited the server so offline players can be assigned roles.

---

## 🕹️ Controls

WardMeBruh includes native gamepad support across all interfaces, configuration dialogs, and in-world interactions. Controls and button hints dynamically switch between keyboard/mouse and controller layouts based on active input.

### In-World Controls

| Action | Keyboard / Mouse | Gamepad | Description |
| :--- | :--- | :--- | :--- |
| **Open Ward Hub** | <kbd>E</kbd> or <kbd>G</kbd> | <kbd>A</kbd> | Opens the ward management interface at a ward monument. |
| **Toggle Ward Power** | — | <kbd>LT</kbd> + <kbd>A</kbd> | Toggles ward activation directly in-world without opening the menu. |
| **Configure Door / Chest** | <kbd>Shift</kbd> + <kbd>E</kbd> | <kbd>LT</kbd> + <kbd>A</kbd> | Opens the role access and renaming configuration dialog. |
| **Configure Portal** | <kbd>Shift</kbd> + <kbd>E</kbd> | <kbd>LT</kbd> + <kbd>A</kbd> | Opens the portal role access configuration dialog. |
| **Standard Interact** | <kbd>E</kbd> | <kbd>A</kbd> | Standard door/chest/portal interaction (checked against role clearance). |

### Ward Hub Dialog Navigation (Gamepad)

| Action | Gamepad Control | Description |
| :--- | :--- | :--- |
| **Switch Tabs** | <kbd>LB</kbd> / <kbd>RB</kbd> | Cycles forward and backward through Ward Hub tabs. |
| **Jump Sections** | <kbd>LT</kbd> / <kbd>RT</kbd> | Jumps the cursor to the previous or next card/section. |
| **Navigate Elements** | <kbd>D-Pad</kbd> or Left Stick | Moves focus between buttons, toggles, and sliders. |
| **Select / Toggle** | <kbd>A</kbd> | Activates the focused button or toggles a checkbox. |
| **Adjust Sliders** | <kbd>D-Pad Left</kbd> / <kbd>Right</kbd> | Steps slider values (e.g. ward radius, repair threshold). |
| **Runic Attunement** | <kbd>Y</kbd> | Direct shortcut to open the Runic Attunement progression menu. |
| **Other Wards** | <kbd>X</kbd> | Direct shortcut to open the Remote Ward territory registry. |
| **Cycle Font Size** | <kbd>R3</kbd> (Right Stick Click) | Cycles UI text scaling between Small, Medium, and Large. |
| **Scroll Viewport** | Right Stick (Up / Down) | Smoothly scrolls the active tab or modal content. |
| **Close / Back** | <kbd>B</kbd> | Closes the open modal or exits the Ward Hub. |

---

## ⚙️ Configuration

Settings are saved to `BepInEx/config/vapok.mods.wardmebruh.cfg`.

### Server Settings (Synced)
* `Enable Ward Me Bruh`: Master mod toggle (default: `true`).
* `Enable Runic Attunement Progression`: Master progression toggle (default: `true`).
* `Progression Mode`: Criteria for unlocking runes (`Hybrid`, `RunicKeystones`, `WorldBossKeys`).
* `Enforce Ward Placement Clearance`: Prevents placing a new ward within 15 meters of an existing ward perimeter (default: `true`).
* `Keystone Weight`: Inventory weight in units for Runic Keystones (default: `200.0`).
* `Max Ward Radius`: Maximum allowed radius in meters (default: `100.0`).
* `Ward Separation Distance`: Minimum buffer distance between unowned wards (default: `50.0`).
* `Default Repair Threshold`: Damage percentage before area repair runs (default: `0.70`).
* `Default Auto-Close Delay`: Seconds before open doors close (default: `5.0`).
* `Free Area Repair`: When true, repairs cost no resources (default: `false`).
* `Allow Ore Fast-Travel`: Allow raw ores through ward fast travel (default: `false`).
* `Fast-Travel Combat Lockout`: Seconds out of combat required before fast travel (default: `10.0`).

### Client Settings
* `Open Ward Hub Keybind`: Shortcut key to open the interface (default: `<kbd>E</kbd>`).
* `Show Physical Attunement Stones`: Renders physical glowing pedestal runestones around attuned ward monuments (default: `true`).
* `Show Splash on Startup`: Shows mod summary window on game launch (default: `true`).
* `Enable Anonymous Telemetry`: Sends anonymous version adoption and session launch metrics (default: `false`).
* `Send Error Reports`: Sends sanitized crash and exception reports (default: `true`).

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
* Non-destructive Harmony patches designed to coexist with ValheimPlus, remote crafting, and inventory management mods.

### Incompatibilities

Due to the nature of shared features and overlapping territory control, WardMeBruh cannot run concurrently with other total-conversion ward overhauls:

* **WardIsLove (`Azumatt.WardIsLove`)**: Incompatible. Both mods overhaul native `PrivateArea` lifecycle, boundary calculations, and territory permissions. Run one or the other.
* **ProtectiveWards (`shudnal.ProtectiveWards`)**: Incompatible. Direct conflict due to duplicate ward access and perimeter control implementations.

---

## 📦 Installation & Server Setup

### Mod Manager (Recommended)
1. Install via **Gale**, **r2modman**, or **Thunderstore Mod Manager**.
2. Dependencies are resolved and installed automatically: `BepInExPack`, `Jotunn`, and `YamlDotNet`.

### Manual Installation
* Extract the `.zip` archive and move the `WardMeBruh` folder into your `Valheim/BepInEx/plugins/` directory.

### Dedicated Servers & Multiplayer
* **Required on Both Client & Server**: WardMeBruh must be installed on both the server and all connecting clients. The mod relies on custom network RPCs and ZDO data synchronization for territory access, role permissions, and runic progression.
* **ServerSync Configuration**: Synced configuration settings (progression toggles, feature locks, recipes) defined on the dedicated server lock down and synchronize to all connecting clients automatically.
* **Dedicated Server Deployment**: Extract the mod into the dedicated server's `BepInEx/plugins/` directory. The mod detects headless server execution cleanly and skips all UI registration.

---

## 💬 Community

* Bug reports and feature requests: [GitHub Issues](https://github.com/Vapok/WardMeBruh/issues)
* Community discussion: [Discord](https://discord.gg/5YAJkRFBXt)

---

## 🔌 For Mod Developers & AI Coding Agents

Looking to integrate your mod with WardMeBruh or build compatibility?

* 📖 **[Integration Guide & API Reference](Docs/WardMeBruhAPI.md)**: Full walkthrough for connecting your mod.
* 🤖 **[AI Agent Rules & Invariants](AGENTS.md)**: Mandatory guidelines for AI assistants (Cursor, Claude, Copilot, ChatGPT, Gemini).
* 📦 **[Zero-Dependency Client Wrapper](Docs/WardMeBruhAPI_Client.cs)**: Drop-in C# file to safely integrate with zero external DLL dependencies.
* ⚠️ **Strict Policy**: Do not use reflection or Harmony patches on internal classes; always use the public API.
* 💡 **Need New API Features?**: Open a [Feature Request](https://github.com/Vapok/WardMeBruh/issues) or discuss it in our [Discord](https://discord.gg/5YAJkRFBXt).

---

## 🔒 Anonymous Telemetry, Error Reporting & Privacy

WardMeBruh includes lightweight, privacy-first telemetry and error reporting to help monitor mod stability, diagnose unhandled bugs, and track active version adoption across game updates.

* **100% Anonymous**: We never collect personal data, Steam IDs, IP addresses, character/world names, or file system paths. Stack traces from errors are automatically sanitized to strip local user directories.
* **Granular Player Control**:
  * **Anonymous Telemetry (Opt-In)**: Tracks version adoption and session launches. Defaults to **unchecked / disabled** when first loaded (`Enable Anonymous Telemetry = false`).
  * **Error Reporting (Opt-Out)**: Captures sanitized mod crash diagnostics to rapidly identify and fix bugs. Defaults to **enabled** (`Send Error Reports = true`) with one-click opt-out.
  * **Data Disclaimers**: Hover over any toggle in the startup modal for interactive tooltip disclaimers detailing exactly what data is transmitted.
* **In-Game & Online Privacy Policy**: The full privacy policy can be viewed directly in-game by clicking **`[ PRIVACY POLICY ]`** on the startup splash modal, or online at [vapok.io/privacy-policy](https://vapok.io/privacy-policy/).
* **Configuration Files**: Settings can be managed in-game via the startup modal, through the BepInEx Configuration Manager, or under `[Local Config]` in `BepInEx/config/vapok.mods.wardmebruh.cfg`.

---

<div align="center">

### 👨‍💻 Created by Vapok Gaming

[![Vapok Gaming](https://avatars.githubusercontent.com/u/1264136?s=120&v=4)](https://github.com/Vapok)

**Author**: [Vapok](https://github.com/Vapok)  
**Source Code**: [GitHub Repository](https://github.com/Vapok/WardMeBruh)  
**Community & Support**: [Discord Server](https://discord.gg/5YAJkRFBXt)  
**Changelog**: [Release Notes](https://github.com/Vapok/WardMeBruh/blob/main/CHANGELOG.md)

</div>
