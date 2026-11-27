using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;
using static UtilityGrid.ModEntry;

namespace UtilityGrid
{
    public class UtilityObjectInstance
    {
        public UtilityObjectInstance(UtilityObject template, Object obj)
        {
            Template = template;
            WorldObject = obj;
        }

        public Vector2 CurrentPowerVector { get; set; }
        public PipeGroup Group { get; set; }
        public UtilityObject Template { get; set; }
        public Object WorldObject { get; set; }
    }
}