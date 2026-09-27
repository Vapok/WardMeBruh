# 0.0.2 - API Extensibility, Role Refinements & Quality of Life
* **Developer API & Extensibility Architecture**:
  * Implemented `WardMeBruh.API.WardMeBruhAPI` facilitating external mod integration.
  * Added `RegisterRoleFeature(...)` allowing third-party mods to dynamically inject custom capability permissions into the WardMeBruh permission matrix under `WardCapabilityCategory.External` or custom category definitions.
  * Added `RegisterRoleTemplate(...)` to inject custom role presets into newly created ward hierarchies.
  * Provided spatial and instance queries: `HasFeatureAccess(...)`, `GetPlayerRole(...)`, `GetPlayerRoleName(...)`, `IsInsideWard(...)`, `IsWardShieldActive(...)`, `GetWardRadius(...)`, `GetWardName(...)`, and `GetCoveringWard(...)`.
  * Added `Docs/WardMeBruhAPI_Client.cs` providing a zero-dependency reflection client wrapper for external mod developers.
* **Automated API Documentation Build Pipeline**:
  * Integrated `Vsxmd` (1.4.6) NuGet build task and target to compile XML doc comments directly into GitHub-flavored Markdown.
  * Configured `WardMeBruh.csproj` with `<GenerateDocumentationFile>true</GenerateDocumentationFile>` and output target `Docs/WardMeBruhAPI.md`.
  * Generated documentation automatically updates on every project build (`dotnet build`).
* **UI & UX Refinements (`TabRoles`, `TabAccess`, `TabUpkeep`, `WardMainWindow`)**:
  * Replaced dual `+ RESIDENT` and `+ MANAGER` buttons with a single `+ ADD` button on Known Players and Manual Entry rows.
  * Replaced static role badges and separate promote/demote buttons with an interactive `[ Role: <RoleName> ]` cycling button matching `TabAccess`.
  * Moved `AccessDoors` and `AccessChests` default permissions from `WardRole.Resident` to `WardRole.Manager`.
  * Added automatic migration in `WardRoleManager.GetRoleDefinitions` to seamlessly migrate existing saved ZDO definitions.
  * Fixed `TabUpkeep` repair threshold normalization between slider percentage (0–100) and game float (0.0–1.0).
  * Fixed ZDO persistence on container and door role updates in `TabAccess` and `EntityRenameDialog`.
  * Preserved tab header button padding and layout dimensions across font size changes.
* **Shield Defenses & Projectile Deflection**:
  * Added `Patch_ShieldGenerator` for projectile deflection within active forcefield dome radius.
* **Door Closing Race Condition Resolution**:
  * Refactored `AutoDoorService` to track and cancel active coroutines per door.
  * Added player proximity detection in `AutoDoorService` (`IsPlayerNearDoor`) preventing automatic door closure while any player is within 3.5m of the doorway.
  * Added `Patch_Door.RPC_UseDoorPrefix` debounce guard tracking close timestamps (`_doorCloseTimestamps`), suppressing duplicate/competing close RPC requests on recently closed doors within a 1.2s window to prevent vanilla toggle inversion from re-opening doors.
  * Updated `InteractPostfix` to cancel pending auto-close timers when a door is manually closed.
* **Engine Performance & Frame Rate Optimizations**:
  * **Shield Dome Caching (`WardForcefieldDome`)**: Throttled camera dome effect resolution (`FindFirstObjectByType`) to check at most once every 2 seconds if missing. Added dirty-state tracking for `ShieldDomeImageEffect.SetShieldData` to prevent redundant per-frame GPU buffer re-uploads. Applied perimeter ring aether tint once on wake instead of creating heap copies every second.
  * **Per-Frame Query Caching (`WardQueryService`)**: Added per-frame cache for `GetCoveringWard` and `GetCoveringWardZdo` matching exact frame and coordinates, eliminating redundant spatial ward searches per frame per player.
  * **Water Physics Postfix Fast-Path (`Patch_WaterVolume`)**: Added `HasAnyActiveWaterDrain` 1-second cached check to exit immediately in 1 CPU cycle when no active wards have water drain enabled, eliminating massive spatial distance loops during water surface calculation.
  * **AI Static Target Perception Fast-Path (`Patch_BaseAI`)**: Added `HasAnyActiveGhostBase` 1-second cached check to exit immediately when no active wards have ghost base enabled.
  * **WearNTear Batching (`Patch_WearNTear`)**: Collapsed 4 separate spatial queries (`WeatherRoof`, `AshShield`, `LavaShield`, `SnowShield`) into a single `GetCoveringWardZdo` resolution on piece wear updates.
  * **Food Timer Pre-Validation (`Patch_PlayerFood`)**: Relocated `timer < 1f` check prior to spatial ward checks, reducing food query frequency from per-frame to once per second.
  * **Territory Status Effect Throttling (`SE_WardTerritory`)**: Throttled territory information updates to 0.5s intervals instead of executing on every frame of `Character.Update()`.
  * **Background Service Fast-Guards (`AreaRepairService`, `AutoFuelService`, `WardRepulsionService`)**: Filter active owned wards requiring processing before instantiating or querying heavy scene lists.

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
