using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Guns
{
    public class GunData
    {
        public string texturePath;
        public Texture2D texture;
        public string bulletTexture;
        public int bulletIndex;
        public int minDamage;
        public int maxDamage;
        public float rate;
        public string fireSound;
        public bool explosive;
        public float bulletRotation;
        public float bulletVelocity;
        public float bulletScale;
        public List<Point> bulletOffsets = new List<Point>()
        {
            new Point(24, -92),
            new Point(48, -48),
            new Point(-22, 4),
            new Point(-48, -48)
        };
    }
}