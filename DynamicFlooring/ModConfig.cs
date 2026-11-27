using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace DynamicFlooring
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Consume { get; set; } = false;
        public SButton ModButton { get; set; } = SButton.LeftControl;
        public SButton IgnoreButton { get; set; } = SButton.LeftShift;
        public SButton RemoveButton { get; set; } = SButton.Delete;
        public SButton PlaceButton { get; set; } = SButton.MouseLeft;

    }
}
