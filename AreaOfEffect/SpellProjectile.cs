using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Projectiles;

namespace AreaOfEffect
{
    public class SpellProjectile : BasicProjectile
    {
        public string Texture { get; set; }
        public Rectangle? SourceRect { get; set; }
        public SpellProjectile(int damage, int spriteIndex, int bounces, int tailLength, float num, float num2, float num3, Vector2 vector, string collisionSound, string bounceSound, string fireSound, bool explodes, bool v, GameLocation location, Farmer who, onCollisionBehavior onCollisionBehavior, string shotItemId, string texture, Rectangle? sourceRect) : base(damage, spriteIndex, bounces, tailLength, num, num2, num3, vector, collisionSound, bounceSound, fireSound, explodes, v, location, who, onCollisionBehavior, shotItemId)
        {
            Texture = texture;
            SourceRect = sourceRect;
        }


        public override bool update(GameTime time, GameLocation location)
        {
            bool result = base.update(time, location);
            if (this.travelDistance >= (float)this.maxTravelDistance.Value)
            {
                collisionBehavior(location, this.getBoundingBox().Center.X, this.getBoundingBox().Center.Y, this.GetPlayerWhoFiredMe(location));
            }
            return result;
        }
    }
}