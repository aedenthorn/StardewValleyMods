using StardewModdingAPI;
using System.Collections.Generic;

namespace StardewOpenWorld
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool CreateEntrance { get; set; } = true;
        public bool NewMapDaily { get; set; } = false;
        public bool DrawMap { get; set; } = true;
        public float MapScale { get; set; } = 0.25f;
        public int MapTilesDimension { get; set; } = 6400;
        public int OpenWorldSize { get; set; } = 10000;
        public int MaxOutcropLevel { get; set; } = 1000;
        public int TilesForestMin { get; set; } = 500;
        public int TilesForestMax { get; set; } = 1000;
        public int TilesLakeMin { get; set; } = 2000;
        public int TilesLakeMax { get; set; } = 4000;
        public int TilesOutcropMin { get; set; } = 600;
        public int TilesOutcropMax { get; set; } = 1200;
        public int TilesMonsterMin { get; set; } = 800;
        public int TilesMonsterMax { get; set; } = 1500;
        public int TilesChestMin { get; set; } = 2000;
        public int TilesChestMax { get; set; } = 20000;
        public int TilesGrassMin { get; set; } = 500;
        public int TilesGrassMax { get; set; } = 1000;
        public int TilesBushMin { get; set; } = 600;
        public int TilesBushMax { get; set; } = 1200;
        public int TilesArtifactMin { get; set; } = 1200;
        public int TilesArtifactMax { get; set; } = 2400;
        public int TilesForageMin { get; set; } = 1500;
        public int TilesForageMax { get; set; } = 3000;
        public int TilesClumpMin { get; set; } = 8000;
        public int TilesClumpMax { get; set; } = 16000;
        public int MinLakeSize { get; set; } = 36;
        public int MaxLakeSize { get; set; } = 300;
        public int MinTreesPerForest { get; set; } = 30;
        public int MaxTreesPerForest { get; set; } = 80;
        public int MinRocksPerOutcrop { get; set; } = 5;
        public int MaxRocksPerOutcrop { get; set; } = 40;
        public int MinGrassPerField { get; set; } = 5;
        public int MaxGrassPerField { get; set; } = 40;
        public double MonsterDensity { get; set; } = 0.2;
        public double TreeDensity { get; set; } = 0.3;
        public double RockDensity { get; set; } = 0.3;
        public double GrassDensity { get; set; } = 0.3;
        public string BackgroundMusic { get; set; } = "desolate";
        public int ChestMaxItems { get; set; } = 5;
        public int ChestItemsBaseMaxValue { get; set; } = 100;
        public int ChestMinItemValue { get; set; } = 20;
        public int ChestMaxItemValue { get; set; } = -1;
        public int CoinBaseMin { get; set; } = 20;
        public int CoinBaseMax { get; set; } = 100;
        public float ChestRarityBias { get; set; } = 1f;
        public float ChestValueIncreaseRate { get; set; } = 0.3f;
        public Dictionary<string, int> ItemListChances { get; set; } = new Dictionary<string, int>
        {
            {"MeleeWeapon", 100},
            {"Shirt", 50},
            {"Pants", 50},
            {"Hat", 50},
            {"Boots", 100},
            {"BigCraftable", 50},
            {"Ring", 100},
            {"Seed", 100},
            {"Mineral", 100},
            {"Relic", 100},
            {"Cooking", 50},
            {"Fish", 0},
            {"BasicObject", 50}
        };
    }
}
