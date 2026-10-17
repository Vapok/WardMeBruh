# 1.0.2 - Localization Support & UI Fixes

### Localization & UI Architecture
* **Full 35-Language Localization (`Translations/*.json`)**:
  * Integrated comprehensive localization coverage across all 35 languages supported by Valheim.
  * Added tokens for attunement rank footer status (`$wmb_footer_attunement_label`, `$wmb_attunement_none`) and boss progression requirements (`$wmb_boss_defeated`, `$wmb_boss_defeat_req`).
* **Dynamic Re-Localization (`WardTextElement`, `WardUIFactory`)**:
  * Added localization key tracking (`localizationKey`, `toUpper`) and `UpdateLocalization()` to `WardTextElement`.
  * Updated `WardUIFactory.CreateCard`, `CreateSettingRow`, `CreateBadgeRow`, and `SetupButtonText` to register localization keys automatically.
  * Implemented `WardMainWindow.LocalizeHierarchy` and `UpdateTabButtonLabels` to re-evaluate and update all UI labels, card headers, and navigation tabs each time the Ward Hub is opened.
* **Role Migration & Display Fallbacks (`WardRoleManager`, `TabManagement`)**:
  * Extended ZDO role migration in `WardRoleManager.GetRoleDefinitions` to remap legacy literal strings (`Owner`, `Manager`, `Resident`) to their respective tokens (`$wmb_role_owner`, `$wmb_role_manager`, `$wmb_role_resident`).
  * Added fallback token resolution in `WardRoleManager.GetRoleDisplayName` and `TabManagement.BuildRoleList`.
* **Premature UI Instantiation Prevention**:
  * Replaced calls to `Instance != null && Instance.IsOpen` across patch hooks (`Patch_Menu`, `Patch_PlayerController`, `Patch_PlayerInput`, `Patch_Door`, `Patch_Container`, `Patch_TeleportWorld`, `Patch_PrivateArea`, `WardRegistryManager`) with `HasInstanceAndOpen` to prevent early instantiation during title screen ticks before game localization loads.

# 1.0.1 - Controller Improvements & Dependency Updates
* **Controller Navigation & UI Refinements**:
  * Refined management role readability, slider stepping, and modal escape navigation.
* **Dependency & Engine Alignment**:
  * Updated internalized dependency `Vapok.Valheim.Common` to 3.1020.1.
  * Aligned game assembly references to Valheim 1.0.20.

# 1.0.0 - Initial Release

### Architecture & Core Systems
* **Centralized Ward Management (`WardRegistryManager`, `WardNetworkRouter`)**:
  * Tracks and categorizes active base wards, extensions, and sub-wards across client and server runtimes.
  * ZDO persistence utilizing dedicated network keys (`WardZdoKeys`) for ward type, parent hierarchy, active power, attunement rank, and role assignments.
  * Synchronized peer updates via `WMB_SyncWardsToPeer` and RPC routing with change-gated delta broadcasts.
* **Role-Based Access Control (`WardRoleManager`, `WardPermissionIds`, `WardRole`)**:
  * Multi-tiered hierarchy supporting Owner, Manager, Contributor, Resident, and Guest roles with custom naming and color assignments.
  * Granular permission gates governing structure placement, demolition, container access, door operation, portal traversal, and ward administration.
  * Direct integration with server administration where admins retain Owner privileges while honoring configured rank progression gates.
* **Multi-Ward Compounds & Nested Sub-Wards (`WardType`, `WardPlacementChoiceModal`)**:
  * Base Extensions: Linking secondary ward stones to parent bases to share power, upkeep, defenses, and access roles across expanded boundaries.
  * Nested Sub-Wards: Independent enclosures situated within parent base boundaries supporting private containers, restricted doors, and distinct permissions.
  * Real-time placement modal (`WardPlacementChoiceModal`) prompting builders to select between Base Extension and Sub-Ward modes upon placing new ward stones.
* **Automated Base Upkeep Engine (`WardUpkeepService`, `Patch_Fireplace`, `Patch_Smelter`, `Patch_WearNTear`)**:
  * Automated refueling of fireplaces, braziers, and hearths within ward boundaries from local container stockpiles.
  * Autonomous smelter, kiln, and blast furnace feeding honoring configured ore and fuel ratios.
  * Periodic structural maintenance repairing damaged building components above configurable durability thresholds.
  * Autonomous livestock feeding from nearby forage containers.
* **Defensive Shields & Environmental Mitigation (`WardDefenseService`, `Patch_ShieldDome`, `Patch_SpawnSystem`)**:
  * Multi-stage protective shields featuring projectile deflection, hostile entity knockback, and electrical shock retaliation.
  * Environmental weather roof shielding preventing rain decay, structural dampness, and exposure damage.
  * Water damage mitigation shielding submerged and coastal wooden structures from decay.
  * Natural creature spawner suppression blocking creature spawns, nests, skeleton piles, and raid events inside active ward boundaries.
* **Runic Attunement Progression System (`WardProgressionService`, `KeystoneItemFactory`, `FloatingRune`)**:
  * 8-tier progression unlocking defensive, upkeep, and administrative features through Elder Futhark keystones (Fehu through Hagalaz).
  * 3D physical runestone pedestals orbiting ward stones in the world with procedural antialiased emissive glyphs (`RuneTextureFactory`) and floating bob animations.
  * 8 craftable Runic Keystones spanning Meadows to Ashlands tiers with embedded status lore chapters (`SE_KeystoneLore`).
  * Backpack inventory integration (`ABAPI_Client`) detecting and consuming keystones directly from worn packs during attunement rituals.
* **Gamepad & Controller Navigation (`WardGamepadNavHelper`, `WardDialogGamepadController`)**:
  * Analog stick and D-pad directional navigation with debounced auto-scrolling (`EnsureVisible`) across all menus, toggles, sliders, and buttons.
  * Tab switching via bumper controls ([LB] / [RB]) and section jumping via trigger controls ([LT] / [RT]).
  * Contextual controller legends, button shortcuts ([Y] Attunement, [X] Other Wards, [R3] Font Size, [B] Close), and in-world interaction prompts ([A] / [LT + A]).
* **Public Developer API Surface (`WardMeBruhAPI`, `WardPermissionRegistry`)**:
  * External mod registration for custom permissions, clearance overrides, and custom role extensions.
  * Dynamic Markdown API reference documentation generation on compilation (`WardMeBruhAPI.md`).
