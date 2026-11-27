using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace StardewVN
{
    public class VNObject
    {
        public Rectangle Bounds { get; set; }
        public List<VNOptionUnique> ClickAction { get; set; }
        public List<VNOptionUnique> HoverAction { get; set; }
    }
}