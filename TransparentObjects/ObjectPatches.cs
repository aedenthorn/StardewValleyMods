using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;

namespace TransparentObjects
{
    public class ObjectPatches
    {
        private static IMonitor Monitor;
        private static ModConfig Config;
        private static IModHelper Helper;

        public static void Initialize(IMonitor monitor, IModHelper helper, ModConfig config)
        {
            Monitor = monitor;
            Config = config;
            Helper = helper;
        }
        public static void Object_draw_Prefix(StardewValley.Object __instance, ref float alpha)
        {
            float maxDistance = Config.TransparencyMaxDistance;
            float minAlpha = Math.Min(1f, Math.Max(0, Config.MinTransparency));
            Vector2 playerCenter = new Vector2(Game1.player.position.X + 32, Game1.player.position.Y + 32);
            Vector2 objectCenter = new Vector2(__instance.TileLocation.X * 64 + 32, __instance.TileLocation.Y * 64);
            float distance = Vector2.Distance(playerCenter, objectCenter);
            if (__instance.bigCraftable && distance < maxDistance)
            {
                float fraction = (Math.Max(0,distance)) / maxDistance;
                alpha = minAlpha + (1 - minAlpha) * fraction;
            }
        }
    }
}
