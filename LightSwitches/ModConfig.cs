using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace LightSwitches
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool IndoorsOnly { get; set; } = false;
        public int ShopPrice { get; set; } = 1000;
        public string OnSound { get; set; } = "button_tap";
        public string OffSound { get; set; } = "button_tap";
        public KeybindList ColorButton { get; set; } = new(SButton.C);
    }
}
