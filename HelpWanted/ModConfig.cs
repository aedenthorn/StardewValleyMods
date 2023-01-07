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
        public bool AllowArtisanGoods { get; set; } = true;
        public bool IgnoreVanillaItemSelection { get; set; } = true;
        public bool OneQuestPerVillager { get; set; } = true;
        public bool AvoidMaxHearts { get; set; } = true;
        public int MaxPrice { get; set; } = -1;
        public int QuestDays { get; set; } = 2;
        public int MaxQuests { get; set; } = 10;
        public float NoteScale { get; set; } = 2;
        public float PortraitScale { get; set; } = 1f;
        public float XOverlapBoundary { get; set; } = 0.5f;
        public float YOverlapBoundary { get; set; } = 0.25f;
        public int PortraitOffsetX { get; set; } = 32;
        public int PortraitOffsetY { get; set; } = 64;
        public int RandomColorMin { get; set; } = 150;
        public int RandomColorMax { get; set; } = 255;
        public int PortraitTintR { get; set; } = 150;
        public int PortraitTintG { get; set; } = 150;
        public int PortraitTintB { get; set; } = 150;
        public int PortraitTintA { get; set; } = 150;
        public float ResourceCollectionWeight { get; set; } = 0.08f;
        public float SlayMonstersWeight { get; set; } = 0.1f;
        public float FishingWeight { get; set; } = 0.07f;
        public float ItemDeliveryWeight { get; set; } = 0.4f;
    }
}
