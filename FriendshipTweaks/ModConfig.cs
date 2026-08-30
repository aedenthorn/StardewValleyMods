using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace FriendshipTweaks
{
    public class ModConfig
    {
        //by Xen0nex
        public bool ModEnabled { get; set; } = true;
        public int MaxHearts { get; set; } = 14;
        public float IncreaseModifier { get; set; } = 1f;
        public float DecreaseModifier { get; set; } = 1f;
        public float BirthdayMultiplier { get; set; } = 8f;
        public float WinterStarMultiplier { get; set; } = 5f;
        public float StardropTeaMultiplier { get; set; } = 3f;
    }
}
