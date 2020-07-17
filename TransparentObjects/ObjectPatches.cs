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
        public static bool XnaDisplayDevice_DrawTile_Prefix(XnaDisplayDevice __instance, Tile tile, Location location, float layerDepth, Dictionary<TileSheet, Texture2D> ___m_tileSheetTextures, ref Vector2 ___m_tilePosition, ref Microsoft.Xna.Framework.Rectangle ___m_sourceRectangle, ref SpriteBatch ___m_spriteBatchAlpha, ref Color ___m_modulationColour)
        {
            if (tile != null && (tile.Layer.Id == "Front" || tile.Layer.Id == "AlwaysFront"))
            {
                Vector2 playerLoc = Game1.player.getTileLocation();
                Monitor.Log($"Player {playerLoc} Tile {location} distance {Vector2.Distance(playerLoc, new Vector2(location.X, location.Y))}");
                if (Vector2.Distance(playerLoc, new Vector2(location.X, location.Y)) < 2)
                {
                    Monitor.Log(".25");
                    ___m_modulationColour.A = 255 / 4;
                }
                else if(Vector2.Distance(playerLoc, new Vector2(location.X, location.Y)) < 3)
                {
                    ___m_modulationColour.A = 255 / 2;
                }
                else if(Vector2.Distance(playerLoc, new Vector2(location.X, location.Y)) < 4)
                {
                    ___m_modulationColour.A = 255 * 3 / 4 ;
                }
                xTile.Dimensions.Rectangle sourceRectangle = tile.TileSheet.GetTileImageBounds(tile.TileIndex);
                Texture2D texture2D = ___m_tileSheetTextures[tile.TileSheet];
                if (!texture2D.IsDisposed)
                {
                    ___m_tilePosition.X = (float)location.X;
                    ___m_tilePosition.Y = (float)location.Y;
                    ___m_sourceRectangle.X = sourceRectangle.X;
                    ___m_sourceRectangle.Y = sourceRectangle.Y;
                    ___m_sourceRectangle.Width = sourceRectangle.Width;
                    ___m_sourceRectangle.Height = sourceRectangle.Height;
                    ___m_spriteBatchAlpha.Draw(texture2D, ___m_tilePosition, new Microsoft.Xna.Framework.Rectangle?(___m_sourceRectangle), ___m_modulationColour, 0f, Vector2.Zero, (float)Layer.zoom, SpriteEffects.None, layerDepth);
                }
                return false;
            }
            return true;
        }
    }
}
