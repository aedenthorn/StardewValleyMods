using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using System;
using Object = StardewValley.Object;

namespace PaintingDisplay
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        private static bool Sign_draw_Prefix(Sign __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (!Config.EnableMod || __instance.displayItem.Value == null || !(__instance.displayItem.Value is Furniture) || (__instance.displayItem.Value as Furniture).furniture_type.Value != 6 || (SHelper.ModRegistry.IsLoaded("aedenthorn.CustomPictureFrames") && __instance.displayItem.Value.modData.ContainsKey("aedenthorn.CustomPictureFrames/index")))
                return true;
            var ptr = AccessTools.Method(typeof(Object), "draw", new System.Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }).MethodHandle.GetFunctionPointer();
            var baseMethod = (Action<SpriteBatch, int, int, float>)Activator.CreateInstance(typeof(Action<SpriteBatch, int, int, float>), __instance, ptr);
            baseMethod(spriteBatch, x, y, alpha);

            Rectangle drawn_source_rect = (__instance.displayItem.Value as Furniture).sourceRect.Value;
            spriteBatch.Draw(Furniture.furnitureTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 - (__instance.displayItem.Value as Furniture).sourceRect.Width * 2, y * 64 - 8 -(__instance.displayItem.Value as Furniture).sourceRect.Height * 2)), new Rectangle?(drawn_source_rect), Color.White * alpha, 0f, Vector2.Zero, 4f, (__instance.displayItem.Value as Furniture).Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.getBoundingBox(new Vector2(x, y)).Bottom + 1)/ 10000f);
            return false;
        }
    }
}