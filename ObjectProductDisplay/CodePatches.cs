using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using System;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ObjectProductDisplay
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public class Object_draw_Patch
        {
            public static void Postfix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
            {
                if (!Config.ModEnabled || (Config.RequireKeyPress && !Config.PressKeys.IsDown()) || !__instance.bigCraftable.Value || __instance.readyForHarvest.Value || __instance.heldObject.Value is null || __instance.MinutesUntilReady <= 0)
                    return;
                float done = GetDoneFraction(__instance);
                float base_sort = (float)((y + 1) * 64) / 10000f + __instance.TileLocation.X / 50000f;
                Rectangle source = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, __instance.heldObject.Value.ParentSheetIndex, 16, 16);
                float scale = 3f * Config.SizePercent / 100f;
                alpha = Config.OpacityPercent / 100f;
                var backSource = source;
                var frontSource = source;
                var offset = (int)Math.Ceiling(16 * (1 - done));
                backSource.Height = offset;
                frontSource.Height = (int)Math.Floor(16 * done);
                frontSource.Offset(0, 16 - frontSource.Height);
                var frontSourceColor = frontSource;
                frontSourceColor.Offset(16, 0);
                var doneOffset = new Vector2(0, (16 -  frontSource.Height) * scale);
                Vector2 pos = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32), (float)(y * 64 + 32)));
                spriteBatch.Draw(Game1.objectSpriteSheet, pos, backSource, Color.Black * alpha, 0f, new Vector2(8f, 8f), scale, SpriteEffects.None, base_sort + 1E-05f);
                spriteBatch.Draw(Game1.objectSpriteSheet, pos + doneOffset, frontSource, Color.White * alpha, 0f, new Vector2(8f, 8f), scale, SpriteEffects.None, base_sort + 1E-05f);
                if (__instance.heldObject.Value is ColoredObject)
                {
                    spriteBatch.Draw(Game1.objectSpriteSheet, pos + doneOffset, frontSourceColor, (__instance.heldObject.Value as ColoredObject).color.Value * alpha, 0f, new Vector2(8f, 8f), scale, SpriteEffects.None, base_sort + 1.1E-05f);
                }
            }

        }
        [HarmonyPatch(typeof(Object), nameof(Object.performObjectDropInAction))]
        public class Object_performObjectDropInAction_Patch
        {
            public static void Prefix(Object __instance, bool probe, ref int __state)
            {
                if (!Config.ModEnabled || probe || !__instance.bigCraftable.Value)
                    return;
                __state = __instance.MinutesUntilReady;
            }
            public static void Postfix(Object __instance, bool probe, int __state, bool __result)
            {
                if (!Config.ModEnabled || probe || !__instance.bigCraftable.Value || __instance.MinutesUntilReady <= __state)
                    return;
                __instance.modData[modKey] = __instance.MinutesUntilReady + "";
            }
        }
    }
}