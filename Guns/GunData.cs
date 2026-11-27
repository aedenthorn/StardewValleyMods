using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Guns
{
    public class GunData
    {
        public bool gunAlwaysShow;
        public string gunTexturePath;
        public Texture2D gunTexture;
        public float gunTextureScale;
        public int gunTileWidth;
        public int gunTileHeight;
        public Point[] gunOffsets = 
        {
            new Point(0, 0),
            new Point(0, 0),
            new Point(0, 0),
            new Point(0, 0)
        };
        public string bulletTexturePath;
        public Texture2D bulletTexture;
        public int bulletIndex;
        public bool bulletFromSpringObjects;
        public float bulletRotation;
        public float bulletVelocity;
        public float bulletScale;
        public Point[] bulletOffsets =
        {
            new Point(0, 0),
            new Point(0, 0),
            new Point(0, 0),
            new Point(0, 0)
        };

        public int minDamage;
        public int maxDamage;
        public int fireRate;
        public int fireTicks;
        public string fireSound;
        public string collisionSound;
        public bool explosive;
        public int explosionRadius;
        public int explosionDamage = -1;
        public bool explosionDamagesFarmers;
    }
}