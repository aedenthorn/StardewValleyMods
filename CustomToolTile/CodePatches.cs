using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;

namespace CustomToolTile
{
    public partial class ModEntry
    {
        public static bool getting = false;
        [HarmonyPatch(typeof(Game1), nameof(Game1.getSourceRectForStandardTileSheet))]
        public static class Game1_getSourceRectForStandardTileSheet_Patch
        {
            public static void Prefix(Texture2D tileSheet, int tilePosition)
            {
                if (!Config.ModEnabled || tileSheet != Game1.mouseCursors || tilePosition != 29 || !Context.IsWorldReady || Game1.activeClickableMenu != null || Game1.eventUp)
                    return;
                getting = true;
            }
        }
        [HarmonyPatch(typeof(SpriteBatch), nameof(SpriteBatch.Draw), new Type[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float) })]
        public static class SpriteBatch_Draw_Patch
        {
            public static bool Prefix(SpriteBatch __instance, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
            {
                if (!getting || !Config.ModEnabled ||  Game1.currentLocation is not GameLocation location)
                    return true;
                getting = false;
                if (Config.Shadow)
                {
                    __instance.Draw(SHelper.GameContent.Load<Texture2D>(squarePath), position + new Vector2(-1, 1) * Config.ShadowOffset, null, Color.Black * 0.5f, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
                }
                if (Config.ShowToolIcon && Game1.player.CurrentTool is Tool tool)
                {
                    tool.drawInMenu(__instance, position, Config.ToolScale, 1f, (Game1.viewport.Y + position.Y + 32f + (((Game1.viewport.Y + position.Y) * 11f + (Game1.viewport.X + position.X) * 7f) % 10f - 5f) + 50) / 10000f);
                }
                __instance.Draw(SHelper.GameContent.Load<Texture2D>(squarePath), position, null, Config.Prismatic ? Utility.GetPrismaticColor(0, Config.PhaseSpeed) : (Config.PhaseColor != Color.Transparent ? Utility.Get2PhaseColor(Config.Color, Config.PhaseColor, 0, Config.PhaseSpeed) : Config.Color), 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
                return false;
            }
        }
    }
}