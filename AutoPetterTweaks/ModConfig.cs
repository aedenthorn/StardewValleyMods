using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace AutoPetterTweaks
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool PetAtEndOfDay { get; set; } = true;
        public int FriendshipReduction { get; set; } = 7;
    }
}
