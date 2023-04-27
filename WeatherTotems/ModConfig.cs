using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace WeatherTotems
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public string InvokeSound { get; set; } = "debuffSpell";
        public string SunnySound { get; set; } = "yoba";
        public string ThunderSound{ get; set; } = "thunder";
        public string RainSound { get; set; } = "rainsound";
        public string CloudySound { get; set; } = "ghost";
        public string SnowSound { get; set; } = "coldSpell";
    }
}
