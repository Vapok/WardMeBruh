# 0.0.4 - Nested Sub-Wards, Base Extensions & Territory Hierarchy
* **Nested Sub-Ward Architecture (`WardType.SubWard`, `WardPlacementChoiceModal`)**:
  * Added `WardType.SubWard` supporting smaller self-contained sub-wards placed inside parent base territory.
  * Implemented `WardPlacementChoiceModal` displaying an interactive choice dialog upon placing an overlapping ward stone, allowing players to choose between expanding the settlement (`ConfigureAsExtension`) or establishing an independent sub-ward (`ConfigureAsSubWard`).
  * Sub-wards maintain independent roles, custom names, permissions, and container/door security access while linking to `WardZdoKeys.ParentWard` for base territory relationship.
* **Geometric Union Sub-Ward Boundary Ray-Marching (`WardQueryService`, `WardCoverageCircle`, `CoverageInterval`)**:
  * Replaced single-ward boundary clamping with 72-angle (5° step) radial ray-marching across the geometric union of the parent base ward and all linked extension wards.
  * Dynamically merges overlapping 1D coverage intervals along each angle ray to locate continuous coverage exit points, enabling sub-wards to expand across and inside extension wards without artificial boundary choking.
  * Enforces clearance against sibling sub-wards to prevent overlapping inner sub-ward boundaries.
* **Custom Sub-Ward Shield Dome Shaders & Color Wheel (`WardColorWheelPicker`, `Patch_ShieldDomeImageEffect`, `WardForcefieldDome`)**:
  * Implemented `WardColorWheelPicker` featuring procedural 160x160 circular antialiased HSV color wheel texture, pointer down/drag event handling, brightness slider, and preset swatches.
  * Added `wmb_dome_color` ZDO key synchronizing custom hex colors over network RPC.
  * Patched `ShieldDomeImageEffect.OnRenderImage` to generate and cache 256x1 gradient textures for the `Hidden/ShieldDomePass` shader per custom color, blitting sub-ward compute buffers with individual gradients while preserving vanilla gradients for main bases.
  * Synchronized perimeter boundary line renderer and base lighting colors in `WardForcefieldDome.UpdatePerimeterTint`.
* **Hierarchical Ward Selector & Management Checks (`RemoteWardSelectorModal`, `WardRegistryManager`)**:
  * Refactored `RemoteWardSelectorModal.PopulateWards` to render a 2-level hierarchy: Main base wards display at the root, with child Extension Wards and Sub-Wards indented with visual tree indicators (`└─`) and type badges.
  * Added permission validation gating: If a player owns a sub-ward in an unmanaged base, the main base is displayed for spatial hierarchy context while suppressing the "Manage" button.
  * Added auto-parent repair in `RemoteWardSelectorModal` and `WardRegistryManager` restoring orphaned sub-wards and extensions to their spatial parent bases.
* **Extension Ward Defenses & Power Decoupling (`WardQueryService`, `TabDefenses`)**:
  * Implemented `GetEffectiveDefenseWardZdo` routing defense inquiries (pushback, mob shock damage, mob spawn block, raid cancellation) to the parent base while respecting local extension power state.
  * Added independent power toggle on extension wards allowing individual perimeter sections to be powered on or off.

# 0.0.3 - Weather Protection, Territory Management & UI Refinements
* **Remote Ward Management & UI Architecture (`RemoteWardSelectorModal`, `WardMainWindow`, `UIFonts`)**:
  * Added `RemoteWardSelectorModal` allowing remote inspection and switching of managed wards across player-owned territory.
  * Converted all legacy uGUI `Text` components to `TextMeshProUGUI` with dynamic font resolution via `UIFonts.GetPrimaryFont()`.
  * Tracked physical interaction origin via `WardMainWindow._physicalWard` (`PhysicalWardId`), decoupling physical player location from the currently active inspected ward (`_activeWard`).
  * Converted territory collection enumeration loops in `WardRegistryManager` (`GetPlayerOwnedWards`, `GetWardsForPlayer`, `GetWardCoveringPoint`, `ValidateWardPlacement`, `PruneDeadWards`) to snapshot lists to prevent collection modification exceptions.
  * Added `Patch_PrivateArea.Setup` Postfix to immediately register newly placed ward stones with creator ID and name upon placement.
  * Added `Patch_WearNTear.Prefix_Destroy`, `Patch_ZNetScene.Prefix_Destroy`, and `Patch_ZDOMan.Prefix_DestroyZDO` hooks to immediately unregister dismantled or destroyed wards and broadcast unregistration over network RPC (`WMB_UnregisterWard`).
* **Shield Generator Proxy & Teardown Architecture (`Patch_ShieldGenerator_Start`, `Patch_ShieldGenerator_OnDestroy`)**:
  * Added `Patch_ShieldGenerator_Start` Prefix on `ShieldGenerator.Start` setting `m_isPlacementGhost = true` and `enabled = false` for proxy components on `WardForcefieldDome`, preventing unwanted registration to `ShieldGenerator.m_instances` and avoiding binding shield RPCs to `PrivateArea`.
  * Added `Patch_ShieldGenerator_OnDestroy` Prefix intercepting `ShieldGenerator.OnDestroy` to completely bypass teardown on proxy generators and null-guard `ShieldGenerator.m_shieldDomeEffect` on real shield generators, eliminating `NullReferenceException` crashes during world unload when the camera is destroyed first.
* **Weather Roof & Environmental Cover Architecture (`Patch_Cover`, `Patch_PlayerComfort`)**:
  * Restored `Patch_Cover` on `Cover.IsUnderRoof` returning `true` whenever covering ward has `WeatherRoof` enabled, restoring virtual roof coverage for `Player.UpdateCover`, `Fireplace.UpdateFireplace` (protecting unroofed fires), `Bed`, `Smelter`, and crafting stations.
  * Added `s_underRoofRef` in `Patch_PlayerComfort.UpdateEnvStatusEffectsPrefix` to immediately synchronize `Player.m_underRoof` with `Cover.IsUnderRoof` on state changes, preventing polling delays from allowing rain ticks through.
  * Decoupled `MaxComfort` from `Cover.IsUnderRoof` so rain penetration is strictly governed by `WeatherRoof`.
  * Restored `InShelter` resolution in `Patch_PlayerComfort.InShelterPostfix` to satisfy shelter conditions when either `WeatherRoof` or `MaxComfort` is enabled.
  * Guarded `Resting` application in `Patch_PlayerComfort.UpdateEnvStatusEffectsPostfix` against `SEMan.s_statusEffectWet`, resolving rapid state oscillation and HUD text pulsing during rain.
* **UI Tab Hierarchy Alignment (`TabUpkeep`, `TabDefenses`)**:
  * Relocated Biome Hazard Shields card (`WeatherRoof`, `AshShield`, `LavaShield`, `SnowShield`, `WaterDrain`) from `TabUpkeep` to `TabDefenses`.
  * Aligned UI placement with underlying `defenses.biome` permission category.

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
