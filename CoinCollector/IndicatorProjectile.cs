//using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;

namespace CoinCollector
{
    internal class IndicatorProjectile : BasicProjectile
    {
        public IndicatorProjectile(int v1, int v2, int v3, int v4, float v5, float v6, float v7, Vector2 vector2, string v8, string v9, bool v10, bool v11, GameLocation currentLocation, Farmer player, bool v12, BasicProjectile.onCollisionBehavior p) : base(v1, v2, v3, v4, v5, v6, v7, vector2, v8, v9, v10, v11, currentLocation, player, v12, p)
        {

        }

        public override void behaviorOnCollisionWithPlayer(GameLocation location, Farmer player)
        {
        }

        public override void behaviorOnCollisionWithTerrainFeature(TerrainFeature t, Vector2 tileLocation, GameLocation location)
        {
        }
        public override void behaviorOnCollisionWithMineWall(int tileX, int tileY)
        {
        }
        public override void behaviorOnCollisionWithOther(GameLocation location)
        {
        }
        public override void behaviorOnCollisionWithMonster(NPC n, GameLocation location)
        {
        }
    }
}