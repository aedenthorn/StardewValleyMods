using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace LogUploader
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool OpenBehind { get; set; } = false;
        public bool SendOnFatalError { get; set; } = true;
        public bool ShowButton { get; set; } = true;
        public SButton SendButton { get; set; } = SButton.F15;
        public SButton ShowButtonButton { get; set; } = SButton.LeftAlt;

    }
}
