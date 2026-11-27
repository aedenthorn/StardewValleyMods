using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SimpleCookingAutomate
{
    public class CookableData
    {
        public string ProductID { get; set; }
        public string BurntID { get; set; }
        public string PlacedSound { get; set; }
        public string CookedSound { get; set; }
        public string BurntSound { get; set; }
        public bool ShowSmoke { get; set; }
        public int CookTime { get; set; }
        public float Burned { get; set; } = 2f;
    }

}