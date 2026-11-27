using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace CustomGiftLimits
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public int OrdinaryGiftsPerDay { get; set; } = 1;
        public int FriendGiftsPerDay { get; set; } = 2;
        public int DatingGiftsPerDay { get; set; } = 3;
        public int SpouseGiftsPerDay { get; set; } = 4;
        public int OrdinaryGiftsPerWeek { get; set; } = 2;
        public int FriendGiftsPerWeek { get; set; } = 4;
        public int DatingGiftsPerWeek { get; set; } = 6;
        public int SpouseGiftsPerWeek { get; set; } = 8;
        public Color DayColor { get; set; } = Color.DarkGreen;
        public Color WeekColor { get; set; } = Color.DarkBlue;
    }
}
