using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace QuestTimeLimits
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public float DailyQuestMult { get; set; } = 2;
        public float SpecialOrderMult { get; set; } = 2;
    }
}
