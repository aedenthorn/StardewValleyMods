
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace OmniTools
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public SButton ModButton { get; set; } = SButton.LeftAlt;
        public SButton CycleButton { get; set; } = SButton.X;
        public SButton RemoveButton { get; set; } = SButton.Z;
        public bool ShowNumber { get; set; } = true;
        public Color NumberColor { get; set; } = Color.Pink;
        public bool SwitchForObjects { get; set; } = true;
        public bool SwitchForTrees { get; set; } = true;
        public bool SwitchForResourceClumps { get; set; } = true;
        public bool SwitchForGrass { get; set; } = true;
        public bool SwitchForPan { get; set; } = true;
        public bool SwitchForWateringCan { get; set; } = true;
        public bool SwitchForFishing { get; set; } = true;
        public bool SwitchForAnimals { get; set; } = true;
        public bool SwitchForMonsters { get; set; } = true;

    }
}
