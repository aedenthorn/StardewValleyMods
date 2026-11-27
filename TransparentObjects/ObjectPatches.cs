using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Linq;

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

            if (!ModEntry.IsAllowed(__instance.name) || ModEntry.IsOff() || !__instance.bigCraftable.Value)
                return;

            float maxDistance = Config.TransparencyMaxDistance;
            Vector2 playerCenter = new Vector2(Game1.player.position.X + 32, Game1.player.position.Y + 32);
            Vector2 objectCenter = new Vector2(__instance.TileLocation.X * 64 + 32, __instance.TileLocation.Y * 64);
            float distance = Vector2.Distance(playerCenter, objectCenter);
            if (distance < maxDistance)
            {
                float minAlpha = Math.Min(1f, Math.Max(0, Config.MinTransparency));
                float fraction = (Math.Max(0,distance)) / maxDistance;
                alpha = minAlpha + (1 - minAlpha) * fraction;
            }
        }
        public static void Crop_draw_Prefix(Crop __instance, Vector2 tileLocation, Color toTint, float rotation)
        {
            __instance.tintColor.A = 255;
            if (!__instance.raisedSeeds.Value || !ModEntry.IsAllowed("Crop"+__instance.indexOfHarvest.Value) || ModEntry.IsOff())
                return;

            float maxDistance = Config.TransparencyMaxDistance;
            Vector2 playerCenter = new Vector2(Game1.player.position.X + 32, Game1.player.position.Y + 32);
            Vector2 objectCenter = new Vector2(tileLocation.X * 64 + 32, tileLocation.Y * 64);
            float distance = Vector2.Distance(playerCenter, objectCenter);
            if (distance < maxDistance)
            {
                float minAlpha = Math.Min(1f, Math.Max(0, Config.MinTransparency));
                float fraction = (Math.Max(0,distance)) / maxDistance;
                byte alpha = (byte)Math.Round((minAlpha + (1 - minAlpha) * fraction) * 255);
                if(__instance.tintColor.Value != Color.White)
                {
                    __instance.tintColor.A = alpha;
                }
                toTint.A = alpha;
            }
        }
    }
}
