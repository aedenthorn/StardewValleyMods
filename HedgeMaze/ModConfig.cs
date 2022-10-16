using StardewModdingAPI;
using System.Collections.Generic;

namespace HedgeMaze
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool HideMaze { get; set; } = true;
        public Dictionary<string, int> ItemListChances { get; set; } = new Dictionary<string, int>
        {
            {"MeleeWeapon", 100},
            {"Shirt", 100},
            {"Pants", 100},
            {"Hat", 100},
            {"Boots", 100},
            {"BigCraftable", 100},
            {"Ring", 100},
            {"Seed", 100},
            {"Mineral", 100},
            {"Relic", 100},
            {"Cooking", 100},
            {"Fish", 0},
            {"BasicObject", 100}
        };
        public int MinItemValue { get; set; } = 20;
        public int MaxItemValue { get; set; } = -1;
        public int MaxItems { get; set; } = 5;
        public int ItemsBaseMaxValue { get; set; } = 100;
        public int Mult { get; set; } = 100;
        public float RarityChance { get; set; } = 0.2f;
        public float IncreaseRate { get; set; } = 0.3f;
        public int CoinBaseMin { get; set; } = 20;
        public int CoinBaseMax { get; set; } = 100;
        public int MineLevelMin { get; set; } = 10;
        public int MineLevelMax { get; set; } = 100;
        public int FairiesMin { get; set; } = 2;
        public int FairiesMax { get; set; } = 5;
        public int TreasureMin { get; set; } = 3;
        public int TreasureMax { get; set; } = 5;
        public int ForageMin { get; set; } = 5;
        public int ForageMax { get; set; } = 15;
        public int SlimeMin { get; set; } = 3;
        public int SlimeMax { get; set; } = 6;
        public int SerpentMin { get; set; } = 1;
        public int SerpentMax { get; set; } = 3;
        public int BatMin { get; set; } = 3;
        public int BatMax { get; set; } = 6;
        public int ShadowBruteMin { get; set; } = 2;
        public int ShadowBruteMax { get; set; } = 5;
        public int ShadowShamanMin { get; set; } = 1;
        public int ShadowShamanMax { get; set; } = 3;
        public int SquidMin { get; set; } = 0;
        public int SquidMax { get; set; } = 2;
        public int SkeletonMin { get; set; } = 2;
        public int SkeletonMax { get; set; } = 4;
        public int DustSpriteMin { get; set; } = 5;
        public int DustSpriteMax { get; set; } = 10;

    }
}
