namespace WardMeBruh.API;

/// <summary>
/// Canonical string identifiers for all built-in WardMeBruh role feature permissions.
/// Pass these constants to WardMeBruhAPI methods such as HasFeatureAccess.
/// </summary>
public static class WardPermissionIds
{
    /// <summary>
    /// Clearance to pass through the forcefield dome without repulsion or damage. Default role: Guest.
    /// </summary>
    public const string CoreDomeAccess = "core.dome_access";

    /// <summary>
    /// Clearance to receive territory status, maximum comfort, food decay pause, and death skill protection. Default role: Resident.
    /// </summary>
    public const string CoreBenefits = "core.benefits";

    /// <summary>
    /// Clearance to place, repair, and deconstruct build pieces and furniture within the protected perimeter. Default role: Resident.
    /// </summary>
    public const string CoreBuilding = "core.building";

    /// <summary>
    /// Clearance to cultivate soil, plant seeds, plant crops, and plant saplings using the Cultivator. Default role: Resident.
    /// </summary>
    public const string CoreFarming = "core.farming";

    /// <summary>
    /// Clearance to harvest mature crops, berry bushes, mushrooms, and pickable resources. Default role: Resident.
    /// </summary>
    public const string CoreHarvest = "core.harvest";

    /// <summary>
    /// Clearance to open and inspect the WardMeBruh Central Hub GUI at a ward stone. Default role: Resident.
    /// </summary>
    public const string CoreViewGui = "core.view_gui";

    /// <summary>
    /// Clearance to toggle the ward shield power state on or off. Default role: Manager.
    /// </summary>
    public const string CorePower = "core.power";

    /// <summary>
    /// Clearance to rename the ward settlement. Default role: Manager.
    /// </summary>
    public const string CoreRename = "core.rename";

    /// <summary>
    /// Clearance to place an Extension Ward to expand base territory. Default role: Owner.
    /// </summary>
    public const string CoreExtendWard = "core.extend_ward";

    /// <summary>
    /// Clearance to place a Sub-Ward inside this ward territory. Default role: Manager.
    /// </summary>
    public const string CoreSubWard = "core.sub_ward";

    /// <summary>
    /// Clearance to configure individual door permissions, auto-close timers, and custom names. Default role: Manager.
    /// </summary>
    public const string AccessDoors = "access.doors";

    /// <summary>
    /// Clearance to configure storage container clearances, toggle Fuel Depots, and rename chests. Default role: Manager.
    /// </summary>
    public const string AccessChests = "access.chests";

    /// <summary>
    /// Clearance to configure portal transit clearances. Default role: Manager.
    /// </summary>
    public const string AccessPortals = "access.portals";

    /// <summary>
    /// Clearance to adjust the ward dome size slider up to 100m. Default role: Owner.
    /// </summary>
    public const string DefensesDomeSize = "defenses.dome_size";

    /// <summary>
    /// Clearance to adjust pushback force, retaliatory shock damage, and shield visuals. Default role: Manager.
    /// </summary>
    public const string DefensesCombat = "defenses.combat";

    /// <summary>
    /// Clearance to configure Ghost Base, Raid Suppression, and Mob Spawn Suppression. Default role: Manager.
    /// </summary>
    public const string DefensesCamouflage = "defenses.camouflage";

    /// <summary>
    /// Clearance to configure Virtual Roof, Ashlands Cinder Shield, and Deep North Snow Shield. Default role: Manager.
    /// </summary>
    public const string DefensesBiome = "defenses.biome";

    /// <summary>
    /// Clearance to toggle damage immunity for tamed animals and livestock. Default role: Manager.
    /// </summary>
    public const string DefensesTames = "defenses.tames";

    /// <summary>
    /// Clearance to configure Area Piece Repair and weathering repair thresholds. Default role: Manager.
    /// </summary>
    public const string UpkeepRepair = "upkeep.repair";

    /// <summary>
    /// Clearance to toggle automated refueling from fuel depots for fireplaces and smelters. Default role: Manager.
    /// </summary>
    public const string UpkeepAutoFuel = "upkeep.autofuel";

    /// <summary>
    /// Clearance to toggle automated closing of doors and gates. Default role: Manager.
    /// </summary>
    public const string UpkeepAutoDoors = "upkeep.autodoors";

    /// <summary>
    /// Clearance to toggle sanctuary perks (Max Comfort, Food Decay Freeze, Death Shield, Regen Boost). Default role: Manager.
    /// </summary>
    public const string UpkeepPerks = "upkeep.perks";

    /// <summary>
    /// Clearance to toggle underwater seabed water displacement. Default role: Manager.
    /// </summary>
    public const string UpkeepWaterDrain = "upkeep.waterdrain";

    /// <summary>
    /// Clearance to toggle agricultural crop protection, terrain modification locking, and cart/boat locking. Default role: Manager.
    /// </summary>
    public const string UpkeepSecurity = "upkeep.security";

    /// <summary>
    /// Clearance to promote, demote, and assign players on the personnel roster. Default role: Manager.
    /// </summary>
    public const string PersonnelRoster = "personnel.roster";

    /// <summary>
    /// Clearance to access the Management matrix and create custom roles. Default role: Owner.
    /// </summary>
    public const string PersonnelGovernance = "personnel.governance";
}
