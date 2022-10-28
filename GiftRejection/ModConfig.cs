using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace GiftRejection
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool RejectDisliked { get; set; } = true;
        public bool RejectHated { get; set; } = true;
        public float DislikedThrowDistance { get; set; } = 2;
        public float HatedThrowDistance { get; set; } = 3;
    }
}
