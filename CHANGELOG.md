# 0.0.6 - Full Controller Support & Gamepad Navigation
* Added complete controller support for managing wards, permissions, defenses, and upkeep from a gamepad.
* Added bumper tab navigation ([LB] / [RB]) and trigger section jumping ([LT] / [RT]) to navigate the Ward Hub.
* Added direct controller shortcuts: [Y] for Runic Attunement, [X] for Other Wards, [R3] for Font Size, and [B] for Back/Close.
* Added glowing focus highlights and auto-scrolling so selected settings and controls stay centered in view.
* Added controller keyhints and on-screen button legend bars across all menus and dialogs.
* Added in-world gamepad prompts: [A] to open the Ward Hub and [LT + A] to configure doors, chests, and portals.
* Added full gamepad navigation and card selection to the Remote Ward selector.
* Added controller navigation, button shortcuts ([A] Extend, [Y] Sub-Ward, [B] Cancel), and input blocking to the territory placement prompt.
* Added portal configuration prompts showing permitted access roles with in-world [LT + A] configuration support.

# 0.0.5 - Runic Attunement Progression System & Network Optimizations
* Added Runic Attunement progression: Wards now start at Rank 0 and unlock features across 8 rune ranks using craftable Runic Keystones.
* Added physical runestone pedestals that orbit the ward stone in the world and light up as each rune is attuned.
* Added 8 craftable Runic Keystones with custom models, glow effects, Norse lore stories, and recipe progression from Meadows to Ashlands.
* Added Adventure Backpacks integration so keystones carried in worn backpacks are detected and used during attunement.
* Added spawner suppression to stop creature spawns from natural spawners, creature nests, and skeleton piles inside ward boundaries.
* Updated server admin handling so admins always have Owner authority on all wards while still following ward rank locks.
* Optimized multiplayer networking to eliminate duplicate sync broadcasts and lower network traffic on busy servers.
* Cached chest and door hover text to remove micro-stutters and improve framerates in large bases.
* Fixed chest and door hover text to show custom role names and colors instead of internal numbers.
* Connected chest and door configuration windows to the font size toggle (Small, Medium, Large).
* Fixed territory status effect tooltip to show custom role display names.

# 0.0.4 - Nested Sub-Wards, Base Extensions & Territory Hierarchy
* Added Nested Sub-Wards: Place smaller personal or specialized wards inside a larger base territory with independent roles, permissions, and security access.
* Added Placement Selection: When placing a ward stone inside an existing base, choose between expanding the settlement as an Extension Ward or creating an independent Sub-Ward.
* Added Customizable Dome Colors: Sub-wards can customize their forcefield shield dome, perimeter boundary ring, and ground lighting with an interactive color wheel in the Defenses tab.
* Added Ward Hierarchy: The "Other Wards" territory menu now displays bases hierarchically, showing main wards at the top with their extension and sub-wards grouped cleanly underneath.
* Added Base Extension Defenses: Extension wards now properly inherit defensive protections (enemy knockback, shock damage, mob suppression, and raid blocking) managed from the main base.
* Added Extension Power Controls: Added an independent power toggle on extension wards so individual areas can be switched on or off as needed.
* Improved Boundary Calculations: Sub-ward boundaries now calculate against the entire combined area of the main base and all attached extensions.
* Improved Permission Checks: In the territory menu, main base wards now display for reference if you own a sub-ward inside them, while management buttons remain restricted to wards you have permission to manage.

# 0.0.3 - Weather Protection, Territory Management & UI Refinements
* Added Remote Ward Territory Management via the "Other Wards" modal to inspect and switch active management across player-owned wards.
* Converted entire UI and all dialogs to TextMeshPro for crisp text rendering and consistent font scaling.
* Fixed Weather Roof shield to properly guard players from rain and prevent getting wet under active wards.
* Fixed an issue where the resting indicator could repeatedly pulse when rain occurred.
* Separated Weather Roof rain protection from Sanctuary Max Comfort so both features operate independently.
* Moved Biome Hazard Shields controls from Upkeep to the Defenses tab to match permission categories.
* Fixed a game exit issue where shield generators could trigger an error during logout.
* Fixed territory list location reporting so the player's physical ward remains accurately marked as Current Location when remotely managing other wards.
* Fixed ward placement race condition where the territory menu could open blank immediately after placing a new stone.
* Fixed ward dismantling synchronization so destroyed wards immediately unregister from the territory list.

# 0.0.2 - API Extensibility, Role Refinements & Quality of Life
* Added developer API supporting custom roles and external mod permission registrations.
* Automatic API documentation generated directly in repository documentation on build.
* Streamlined member enrollment on the Roster tab with a single Add button and interactive role toggle.
* Adjusted default permissions so configuring doors and chests access requires Manager or Owner rank.
* Resolved door closing race conditions to prevent doors from accidentally reopening.
* Fixed UI layout and font scaling consistency across modal windows, popups, and tab navigation.
* Fixed structural repair threshold scaling and persistent saving for container and door access settings.
* Restored projectile deflection under active forcefield shields.
* Optimized performance across shields, water physics, base repairs, and player checks to improve frame rates and eliminate micro-stutters in large bases.

# 0.0.1 - Initial Release
* **Centralized Ward Hub**: Interactive management window at the ward stone featuring Overview, Defenses, Upkeep, Access, Roster, and Management tabs.
* **Base Extensions**: Placing an additional ward stone inside an active shield links it to the main base, sharing power, roles, and settings while expanding the total protected perimeter.
* **Role-Based Governance**: Assign roles (Guest, Resident, Manager, Owner) or create custom roles with customizable permissions for building, farming, crop harvesting, and settlement management.
* **Access Control**: Set clearance requirements for individual doors, storage chests, and portal travel.
* **Territory & Dome Defenses**: Notched dome size slider up to 100m, enemy knockback, shock damage, raid cancellation, and mob spawn blocking.
* **Base Upkeep & Automation**: Free area repair for damaged pieces, auto-refueling torches and smelters from designated fuel chests, and auto-closing doors.
* **Sanctuary Perks**: Maximum base comfort everywhere under active shields, food timer freeze, death skill loss protection, and health/stamina/eitr recovery boost.
* **HUD Status Display**: Territory status icon visible on the HUD for all players in ward territory showing current role clearance and shield power status.
* **Font Scaling**: Toggle button to cycle between Small, Medium, and Large font sizes.
* **Persistent Roster**: Tracks players who have visited the world so offline players remain assignable.

# 0.0.0 - Development Inception
* **Development Inception**: Initial unreleased development boilerplate for WardMeBruh.
* **Ward Management**: Foundational quality-of-life framework for ward interaction and protection.
* **Server Compatibility**: Full support for solo play, player-hosted co-op, and dedicated servers.
* **Dependencies**: Built against Jotunn 2.30.2 and modern Valheim.
