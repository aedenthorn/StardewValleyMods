
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace CraftFromContainers
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool EnableEverywhere { get; set; } = false;
        public bool EnableForCrafting { get; set; } = true;
        public bool EnableForBuilding { get; set; } = true;
        public SButton ToggleButton { get; set; } = SButton.None;

    }
}
