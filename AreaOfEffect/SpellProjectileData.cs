using Microsoft.Xna.Framework;
using StardewValley.GameData.Weapons;

namespace AreaOfEffect
{
    public class SpellProjectileData : WeaponProjectile
    {
        public string Texture { get; set; }
        public Rectangle? SourceRect { get; set; } 
    }
}