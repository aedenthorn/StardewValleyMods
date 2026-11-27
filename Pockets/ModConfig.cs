using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace Pockets
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool HoverForHotkey { get; set; } = true;
        public bool GlobalDefaultPockets { get; set; } = false;
        public float PocketAlpha { get; set; } = 0.25f;
        public Color PocketColor { get; set; } = Color.White;
        public Dictionary<string, PocketData> DefaultPockets { get; set; } = new()
        {
            {
                "DefaultShirt",
                new PocketData()
                {
                    ClothesType = StardewValley.Objects.Clothing.ClothesType.SHIRT,
                    HotKey = new Keybind(SButton.LeftControl, SButton.S),
                    StartX = 16,
                    StartY = 36,
                    Width = 32,
                    Height = 24,
                    PocketRows = 3,
                    PocketSlots = 9
                }
            },
            {
                "DefaultRightPants",
                new PocketData()
                {
                    ClothesType = StardewValley.Objects.Clothing.ClothesType.PANTS,
                    HotKey = new Keybind(SButton.LeftControl, SButton.L),
                    StartX = 16,
                    StartY = 60,
                    Width = 16,
                    Height = 20,
                    PocketRows = 3,
                    PocketSlots = 9
                }
            },
            {
                "DefaultLeftPants",
                new PocketData()
                {
                    ClothesType = StardewValley.Objects.Clothing.ClothesType.PANTS,
                    HotKey = new Keybind(SButton.LeftControl, SButton.R),
                    StartX = 32,
                    StartY = 60,
                    Width = 16,
                    Height = 20,
                    PocketRows = 3,
                    PocketSlots = 9
                }
            }
        };
    }
}
