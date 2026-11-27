using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
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
                if (!Config.ModEnabled || (Config.RequireKeyPress && !Config.PressKeys.IsDown()) || !__instance.bigCraftable.Value || __instance.readyForHarvest.Value || __instance.heldObject.Value is not Object ho || __instance.MinutesUntilReady <= 0)
                    return;
                Rectangle source;
                Rectangle coloredSource;
                ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(ho.QualifiedItemId);
                ColoredObject co = ho as ColoredObject;
                if (co != null)
                {
                    source = itemData.GetSourceRect(0, new int?(co.ParentSheetIndex));
                    if (co.ColorSameIndexAsParentSheetIndex)
                    {
                        coloredSource = source;
                    }
                    else
                    {
                        coloredSource = itemData.GetSourceRect(1, new int?(co.ParentSheetIndex));
                    }
                }
                else
                {
                    source = itemData.GetSourceRect();
                    coloredSource = source;
                }

                float done = GetDoneFraction(__instance);
                float base_sort = (float)((y + 1) * 64) / 10000f + __instance.TileLocation.X / 50000f;
                float scale = 3f * Config.SizePercent / 100f;
                alpha = Config.OpacityPercent / 100f;
                var backSource = source;
                var frontSource = source;
                var frontSourceColored = coloredSource;
                var offset = (int)Math.Ceiling(16 * (1 - done));
                backSource.Height = offset;
                frontSource.Height = (int)Math.Floor(16 * done);
                frontSourceColored.Height = (int)Math.Floor(16 * done);
                frontSource.Offset(0, 16 - frontSource.Height);
                frontSourceColored.Offset(0, 16 - frontSourceColored.Height);
                
                var doneOffset = new Vector2(0, (16 - frontSource.Height) * scale);
                Vector2 pos = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32), (float)(y * 64 + 32)));
                spriteBatch.Draw(Game1.objectSpriteSheet, pos, backSource, Color.Black * alpha, 0f, new Vector2(8f, 8f), scale, SpriteEffects.None, base_sort + 1E-05f);

                Texture2D texture = itemData.GetTexture();

                if (co != null)
                {
                    if (!co.ColorSameIndexAsParentSheetIndex)
                    {
                        spriteBatch.Draw(Game1.objectSpriteSheet, pos + doneOffset, frontSource, Color.White * alpha, 0f, new Vector2(8f, 8f), scale, SpriteEffects.None, base_sort + 1.1E-05f);
                        spriteBatch.Draw(Game1.objectSpriteSheet, pos + doneOffset, frontSourceColored, co.color.Value * alpha, 0f, new Vector2(8f, 8f), scale, SpriteEffects.None, base_sort + 1.2E-05f);
                    }
                }
                else
                {
                    spriteBatch.Draw(Game1.objectSpriteSheet, pos + doneOffset, frontSource, Color.White * alpha, 0f, new Vector2(8f, 8f), scale, SpriteEffects.None, base_sort + 1E-05f);
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