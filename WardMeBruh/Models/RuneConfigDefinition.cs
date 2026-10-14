using System.Collections.Generic;
using BepInEx.Configuration;
using CraftingTable = ItemManager.CraftingTable;

namespace WardMeBruh.Models;

public class RuneConfigDefinition
{
    public WardRune Rune { get; set; }
    public string SectionName { get; set; }
    public ConfigEntry<string> RecipeConfig { get; set; }
    public ConfigEntry<CraftingTable> StationConfig { get; set; }
    public ConfigEntry<int> StationLevelConfig { get; set; }
    public ConfigEntry<float> MaxRadiusConfig { get; set; }
    public ConfigEntry<string> BossKeyConfig { get; set; }
    public ConfigEntry<WardFeature> FeaturesConfig { get; set; }

    public List<Piece.Requirement> ParsedRequirements { get; } = new();
    public HashSet<WardFeature> ParsedFeatures { get; } = new();
    public HashSet<string> ParsedExternalFeatureIds { get; } = new();
}
