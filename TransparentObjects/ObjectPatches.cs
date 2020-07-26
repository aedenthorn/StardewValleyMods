using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;

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

        private static Color m_modulationColour;

        public static void DisplayDevice_DrawTile_Prefix(Tile tile, Location location, ref Color ___m_modulationColour)
        {
            Monitor.Log("Test");
            m_modulationColour = ___m_modulationColour;
            if (tile != null && (tile.Layer.Id == "Front" || tile.Layer.Id == "AlwaysFront"))
            {
                Vector2 playerLoc = Game1.player.getTileLocation();
                if (Vector2.Distance(playerLoc, new Vector2(location.X, location.Y)) < 3)
                {
                    Monitor.Log("1");
                    ___m_modulationColour *= 1 / 4;
                }
                else if (Vector2.Distance(playerLoc, new Vector2(location.X, location.Y)) < 4)
                {
                    Monitor.Log("2");
                    ___m_modulationColour *= 1 / 2;
                }
                else if (Vector2.Distance(playerLoc, new Vector2(location.X, location.Y)) < 5)
                {
                    Monitor.Log("3");
                    ___m_modulationColour *= 3 / 4;
                }
            }
        }
        public static void DisplayDevice_DrawTile_Postfix(ref Color ___m_modulationColour)
        {
            Monitor.Log("test");
            ___m_modulationColour = m_modulationColour;
        }
        public static void Layer_DrawNormal_Prefix(IDisplayDevice displayDevice, xTile.Dimensions.Rectangle mapViewport, Location displayOffset)
        {
            //Monitor.Log(displayDevice.GetType().ToString());
        }
    }
}
