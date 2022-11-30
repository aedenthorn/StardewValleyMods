using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace HelpWanted
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool MustLikeItem { get; set; } = true;
        public bool MustLoveItem { get; set; } = false;

        public int IconScale { get; set; } = 8;
        public int MaxQuests { get; set; } = 10;
        public float ResourceCollectionWeight { get; set; } = 0.08f;
        public float SlayMonstersWeight { get; set; } = 0.1f;
        public float FishingWeight { get; set; } = 0.07f;
        public float ItemDeliveryWeight { get; set; } = 0.4f;
    }
}
