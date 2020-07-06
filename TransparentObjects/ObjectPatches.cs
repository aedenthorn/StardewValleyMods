using StardewModdingAPI;
using StardewValley;

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
            if (__instance.bigCraftable && Game1.player.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(64 * ((int)__instance.tileLocation.X) + 32 - (Config.TransparencyDiameter / 2), 64 * ((int)__instance.tileLocation.Y + 1) - Config.TransparencyDiameter, Config.TransparencyDiameter, Config.TransparencyDiameter)))
            {
                alpha = Config.ObjectAlpha;
            }
        }
    }
}
