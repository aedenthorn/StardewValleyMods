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
            if (__instance.bigCraftable && Game1.player.GetBoundingBox().Intersects(new Rectangle(64 * ((int)__instance.tileLocation.X) + 32 - (Config.TransparencyDiameter / 2), 64 * ((int)__instance.tileLocation.Y) - Config.TransparencyDiameter, Config.TransparencyDiameter, Config.TransparencyDiameter)))
            {
                alpha = 1f - (1f - Math.Min(1f, Math.Max(0, Config.ObjectAlpha))) * (Vector2.Distance(new Vector2(Game1.player.GetBoundingBox().Center.X *64f, Game1.player.GetBoundingBox().Center.Y * 64f), new Vector2(__instance.TileLocation.X * 64 + 32, __instance.TileLocation.Y * 64)) / Config.TransparencyDiameter);
            }
        }
    }
}
