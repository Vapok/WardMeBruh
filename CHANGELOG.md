# 1.0.3 - API Patcher Resolution & Mod Interop
* **API Patcher Compatibility**: Resolved an issue in the internal API patcher where downstream mods embedding the developer API stub could encounter an error during startup.
* **Downstream Mod Interoperability**: Ensured smooth API linking for companion mods that integrate with WardMeBruh.

# 1.0.2 - Localization Support & UI Fixes
* **35 Language Support**: Added complete localization support across all 35 languages supported by Valheim.
* **In-Game Translation Fixes**: Fixed an issue where static menu headers, tab buttons, and row labels remained in English when playing in other languages.
* **Role Names**: Default role names now translate properly into your selected language.
* **Interface Improvements**: Resolved premature window loading on the main menu.

# 1.0.1 - Controller Improvements & Dependency Updates
* **Controller Support**: Improved controller navigation, responsiveness, and interaction across ward interfaces.
* **Compatibility**: Updated internal dependencies for stability and Valheim 1.0.20 compatibility.

# 1.0.0 - Initial Release
* **Interactive Ward Hub**: Centralized management window at the ward stone featuring Overview, Defenses, Upkeep, Access, Roster, and Attunement tabs.
* **Base Extensions**: Place additional ward stones inside active boundaries to link them to the main base, sharing power, roles, and settings while expanding total coverage.
* **Nested Sub-Wards**: Create independent personal areas within larger bases featuring isolated role permissions, private containers, and customizable dome forcefield colors.
* **Role-Based Access Control**: Assign custom roles (Owner, Manager, Contributor, Resident, Guest) with granular permissions for building, opening doors, accessing chests, and using portals.
* **Door, Chest & Portal Clearance**: In-world configuration prompts to assign clearance levels directly to doors, containers, and fast-travel portals.
* **Automated Base Upkeep**: Toggle autonomous campfire refueling, smelter and kiln loading, structural repairs, and livestock feeding within ward boundaries.
* **Configurable Base Defenses**: Configurable protective forcefields, enemy knockback, shock damage, weather roof shields, water damage suppression, and creature spawner blocking.
* **Runic Attunement Progression**: Multi-tiered progression system where wards unlock capabilities across 8 runic ranks using craftable Elder Futhark keystones.
* **Physical Floating Runestone Pedestals**: Orbiting runestones surrounding the ward monument that light up and project glowing runes as rituals are completed.
* **Remote Ward Management**: Manage distant bases and sub-wards remotely through the Other Wards window.
* **Full Gamepad & Controller Support**: Complete controller navigation with bumper tab switching, trigger card jumping, contextual keybind hints, and in-world interaction prompts.
* **Developer API**: Public API surface for external mod registration, custom permission flags, and role integrations.
* Updated Jotunn to 2.30.2 and internal dependencies for stability.
