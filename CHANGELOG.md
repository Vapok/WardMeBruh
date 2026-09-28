# 0.0.3
* Fixed Weather Roof shield to properly guard players from rain and prevent getting wet under active wards.
* Fixed an issue where the resting indicator could repeatedly pulse when rain occurred.
* Separated Weather Roof rain protection from Sanctuary Max Comfort so both features operate independently.
* Moved Biome Hazard Shields controls from Upkeep to the Defenses tab to match permission categories.

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
