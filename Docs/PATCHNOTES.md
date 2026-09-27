# 0.0.1 - Initial Release
* **Base Extension Architecture**:
  * Implemented hierarchical parent-child ward linking via `WardZdoKeys.ParentWard` and `KnownExtensionWard`.
  * Multi-ward compound bases unify power state, custom roles, permissions, and name under the root parent ZDO while projecting distinct coverage spheres.
* **Role Governance & Capability System**:
  * Implemented `WardRoleManager`, `WardRoleDefinition`, and `FeaturePermission` covering 20 granular permissions categorized into Core, Access, Defenses, Upkeep, and Personnel.
  * Supported hierarchical permission inheritance and dynamic role capability synchronization across ZDO network boundaries.
  * Added `TabManagement` to configure role permissions and create custom ward roles.
* **Access Control & Placement Hooks**:
  * Added Harmony prefix on `PrivateArea.CheckAccess` to maintain building (`core.building`) and cultivating/planting (`core.farming`) permissions regardless of dome power state.
  * Patched `Pickable.Interact` to enforce `core.harvest` clearance.
  * Patched `Door`, `Container`, and `TeleportWorld` to enforce per-object minimum role clearances with bidirectional destination portal checks.
* **Sanctuary & Environmental Systems**:
  * Separated dome power state from base settlement boundary: deactivated domes disable shield defenses, pushback, creature shock damage, raid suppression, spawn suppression, and virtual shelter/comfort perks, leaving vanilla engine mechanics in effect.
  * Upkeep automations (`AreaRepairService`, `AutoFuelService`, auto-closing doors) and security access settings persist continuously across power toggles.
  * Implemented `SE_WardTerritory` status effect rendering character role badge and shield status for all players within ward territory.
* **UI & UX Engine**:
  * Created dynamic font scaling system (`WardFontSizeMode.Small`, `Medium`, `Large`) with automatic hierarchy font recalculation.
  * Stepped dome radius slider with 5m notched increments, visual major/minor tick marks, and 100m maximum perimeter cap.
  * Implemented `PlayerRegistryService` saving world player lists to `player_registry.json` for persistent offline roster management.

# 0.0.0 - Development Inception
* **Initial Project Architecture**:
  * Established modular BepInEx plugin architecture utilizing `Vapok.Common` 3.21.1015 and `JotunnLib` 2.30.2.
  * Implemented synchronized configuration pipeline inheriting from `ConfigSyncBase`.
  * Configured `ILRepack` MSBuild task to internalize `Vapok.Valheim.Common.dll` into target assembly `WardMeBruh.dll`.
* **Dedicated Server Safety**:
  * Isolated client UI and game startup hooks using `GUIManager.IsHeadless()`.
* **Game Reference Alignment**:
  * Built against Valheim 1.0.16 publicized assemblies and Unity 6 (6000.0.75f1) engine runtime.
