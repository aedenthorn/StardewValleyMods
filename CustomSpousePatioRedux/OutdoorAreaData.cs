using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;

namespace CustomSpousePatioRedux
{
    public class OutdoorAreaData
    {
        public Dictionary<string, Vector2> areas;
        public Dictionary<string, OutdoorArea> dict = new Dictionary<string, OutdoorArea>();
    }

    public class OutdoorArea
    {
        public Vector2 corner;
        public string location;
    }
}