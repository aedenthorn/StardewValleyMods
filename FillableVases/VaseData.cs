using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Collections.Generic;

namespace FillableVases
{
    public class VaseData
    {
        public FlowerData[] Flowers {  get; set; }
        public string MaskTexture { get; set; }
    }

    public class FlowerData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Height { get; set; } = 0;
        public float Scale { get; set; } = 4;
        public float Rotation { get; set; } = 0;
        public Vector2 Origin { get; set; } = Vector2.Zero;
    }
    public class CachedFlowerData
    {
        public Texture2D Texture { get; set; }
        public bool SameIndex { get; set; }
        public Rectangle SourceRect { get; set; }
        public Rectangle ColorSourceRect { get; set; }
    }
}