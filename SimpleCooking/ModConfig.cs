
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SimpleCooking
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public float GrilledEdibilityMult { get; set; } = 1.25f;
        public float GrilledPriceMult { get; set; } = 1.5f;
        public int GrillTime { get; set; } = 10;
        public float BurntAt { get; set; } = 3;
        public string PlacedSound { get; set; } = "cut";
        public string CookedSound { get; set; } = "fireball";
        public string BurntSound { get; set; } = "furnace";
        public Color GrilledColor { get; set; } = Color.DarkGoldenrod;
        public Color BurnedColor { get; set; } = Color.DarkSlateGray;
        public List<int> GrillableCategories = new()
        {
            -75,
            -4
        };
        public List<string> GrillableItems = new()
        {

        };
        public bool Debug { get; set; } = false;
    }
}
