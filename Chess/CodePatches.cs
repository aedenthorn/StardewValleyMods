using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using Object = StardewValley.Object;

namespace Chess
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public class Object_draw_Patch
        {
            public static bool Prefix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
            {
                if (!Config.EnableMod || !__instance.modData.TryGetValue(pieceKey, out string piece))
                    return true;
                if (heldPiece is not null && __instance == heldPiece)
                    return false;
                Vector2 scaleFactor = __instance.getScale();
                Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
                Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
                float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
                spriteBatch.Draw(piecesSheet, destination, new Rectangle(GetSourceRectForPiece(piece), new Point(64, 128)), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
                return false;
            }
        }
        
        [HarmonyPatch(typeof(Object), nameof(Object.updateWhenCurrentLocation))]
        public class Object_updateWhenCurrentLocation_Patch
        {
            public static bool Prefix(Object __instance)
            {
                return !Config.EnableMod || !__instance.modData.ContainsKey(pieceKey);
            }
        }
    }
}