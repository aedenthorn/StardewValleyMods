
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace AdvancedSmoking
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public string PlaceSound { get; set; } = "furnace";
        public string WorkSound { get; set; } = "fireball";
        public float WorkSoundChance { get; set; } = 0.165f;
        public KeybindList ToggleButton { get; set; } = new KeybindList(SButton.R);
        public float TimeMultCopper { get; set; } = 0.5f;
        public float TimeMultIron { get; set; } = 0.7f;
        public float TimeMultGold { get; set; } = 1.0f;
        public float TimeMultIridium { get; set; } = 1.5f;
        public string SkillCopper { get; set; } = "s Mining 2";
        public string SkillIron { get; set; } = "s Mining 4";
        public string SkillGold { get; set; } = "s Mining 6";
        public string SkillIridium { get; set; } = "s Mining 8";
    }
}
