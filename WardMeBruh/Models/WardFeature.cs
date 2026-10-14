using System;

namespace WardMeBruh.Models;

[Flags]
public enum WardFeature : ulong
{
    None = 0UL,
    AutoCloseDoors = 1UL << 0,
    ProtectTames = 1UL << 1,
    PortalAccess = 1UL << 2,
    ProtectCrops = 1UL << 3,
    WeatherRoof = 1UL << 4,
    AutoFuel = 1UL << 5,
    VehicleLocking = 1UL << 6,
    AreaRepair = 1UL << 7,
    PushbackForce = 1UL << 8,
    CreatureShock = 1UL << 9,
    MaxComfort = 1UL << 10,
    SnowShield = 1UL << 11,
    FoodFreeze = 1UL << 12,
    GhostBase = 1UL << 13,
    RaidSuppression = 1UL << 14,
    SpawnSuppression = 1UL << 15,
    PlayerShock = 1UL << 16,
    WaterDrain = 1UL << 17,
    DeathSkillShield = 1UL << 18,
    RegenBoost = 1UL << 19,
    StaminaRegenBoost = 1UL << 20,
    EitrRegenBoost = 1UL << 21,
    AshShield = 1UL << 22,
    LavaShield = 1UL << 23,
    FastTravel = 1UL << 24,
    ProtectTerrain = 1UL << 25,
    ArcticHearth = 1UL << 26,
    PackIceShield = 1UL << 27,
    PassiveBubble = 1UL << 28,
    PickableHarvestProtection = 1UL << 29,
    WardExtensions = 1UL << 30
}
