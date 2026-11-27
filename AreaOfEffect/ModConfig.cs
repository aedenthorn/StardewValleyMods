
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace AreaOfEffect
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public KeybindList RechargeButton { get; set; } = new(SButton.MouseMiddle);
        public KeybindList CastButton { get; set; } = new(new Keybind(SButton.LeftControl, SButton.MouseLeft));
        public string SetEffectSound { get; set; } = "cowboy_explosion";
        public bool AutoOpenUI { get; set; } = true;
        public bool ForceRecast { get; set; } = true;
        public bool ExtraDrawEffects { get; set; } = true;
        public int UpgradeDays { get; set; } = 2;
        public bool Debug { get; set; } = false;
    }
}
