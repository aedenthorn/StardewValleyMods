using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace PersonalJukeBox
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool LimitToKnown { get; set; } = false;
        public KeybindList FavoriteKey { get; set; } = new(new Keybind(SButton.LeftControl, SButton.F));
        public KeybindList ClearFavoriteKey { get; set; } = new(new Keybind(SButton.LeftShift, SButton.LeftControl, SButton.F));
        public KeybindList LastKey { get; set; } = new(SButton.Left);
        public KeybindList NextKey { get; set; } = new(SButton.Right);
        public KeybindList RandomKey { get; set; } = new(SButton.Up);
        public KeybindList PauseKey { get; set; } = new(SButton.Down);
        public KeybindList StopKey { get; set; } = new(SButton.End);
        public KeybindList MenuKey { get; set; } = new(SButton.NumPad0);
    }
}
