using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace DynamicFlooring
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public SButton ModButton { get; set; } = SButton.LeftControl;
        public SButton RemoveButton { get; set; } = SButton.Delete;
        public SButton PlaceButton { get; set; } = SButton.MouseLeft;

    }
}
