using System.Collections.Generic;

namespace OverworldChests
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool AllowIndoorSpawns { get; set; } = true;
        public string OnlyAllowLocations { get; set; } = "";
        public string DisallowLocations { get; set; } = "";
        public bool RoundNumberOfChestsUp { get; set; } = true;
        public int RespawnInterval { get; set; } = 7;
        public float ChestDensity { get; set; } = 0.001f;
        public int Mult { get; set; } = 100;
        public int MaxItems { get; set; } = 5;
        public int ItemsBaseMaxValue { get; set; } = 100;
        public int MinItemValue { get; set; } = 20;
        public int MaxItemValue { get; set; } = -1;
        public int CoinBaseMin { get; set; } = 20;
        public int CoinBaseMax { get; set; } = 100;
        public float RarityChance { get; set; } = 0.1f;
        public float IncreaseRate { get; set; } = 0.3f;
        public List<string> ItemListTypes { get; set; } = new List<string>
        {
            "Weapon", "Shirt", "Pants", "Hat", "Boots", "BigCraftable", "Ring", "Seed", "Mineral", "Relic"
        };
        public List<string> ItemListAllTypesDoNotEditJustCopyFromHere { get; set; } = new List<string>
        {
            "Weapon", "Shirt", "Pants", "Hat", "Boots", "BigCraftable", "Ring", "Cooking", "Seed", "Mineral", "Fish", "Relic", "BasicObject"
        };
    }
}
