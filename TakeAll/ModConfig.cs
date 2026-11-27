using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace TakeAll
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool TakeSameByDefault { get; set; } = false;
        public bool CloseAfterTake { get; set; } = false;
        public SButton TakeButton { get; set; } = SButton.Tab;
        public SButton ModButton { get; set; } = SButton.LeftControl;
    }
}
