using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace FriendshipTweaks
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public int MaxHearts { get; set; } = 20;
        public float IncreaseModifier { get; set; } = 1f;
        public float DecreaseModifier { get; set; } = 1f;
    }
}
