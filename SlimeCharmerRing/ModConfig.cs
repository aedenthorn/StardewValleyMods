
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace SlimeCharmerRing
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public KeybindList EffectKey { get; set; } = new KeybindList(SButton.MouseMiddle);

    }
}
