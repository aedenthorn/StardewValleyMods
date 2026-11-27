using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace ShippingBonuses
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool AllowMultSame { get; set; } = false;
        public int MinDaily { get; set; } = 0;
        public int MaxDaily { get; set; } = 3;
        public int StarWeight { get; set; } = 100;
        public int CategoryWeight { get; set; } = 100;
        public int VegetableWeight { get; set; } = 100;
        public int FruitWeight { get; set; } = 100;
        public int ArtisanWeight { get; set; } = 100;
        public int FlowerWeight { get; set; } = 100;
        public int MineralWeight { get; set; } = 100;
    }
}
