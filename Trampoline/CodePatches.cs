using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using Object = StardewValley.Object;
using StardewValley.Objects;
using Netcode;
using System.Collections.Generic;

namespace Trampoline
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Furniture), nameof(Furniture.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public class Furniture_draw_Patch
        {
            public static bool Prefix(Furniture __instance, SpriteBatch spriteBatch, int x, int y, float alpha, NetVector2 ___drawPosition)
            {
                if (!Config.EnableMod || !__instance.modData.ContainsKey(trampolineKey))
                    return true;
                Rectangle drawn_source_rect = new Rectangle(0, 0, 32, 48);
                bool touching = IsOnTrampoline() && __instance == Game1.player.sittingFurniture && Game1.player.yOffset < 12;
                if (touching)
                    drawn_source_rect.X += 32;
                Rectangle cutoffRect1 = new Rectangle(32, 0, 32, 30);
                Rectangle cutoffRect2 = new Rectangle(32, 30, 32, 18);
                if (Furniture.isDrawingLocationFurniture)
                {
                    if (touching)
                    {
                        spriteBatch.Draw(trampolineTexture, Game1.GlobalToLocal(Game1.viewport, ___drawPosition.Value + ((__instance.shakeTimer > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero)), new Rectangle?(cutoffRect1), Color.White * alpha, 0f, Vector2.Zero, 4f, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 2E-09f + __instance.TileLocation.Y / 100000f);
                        spriteBatch.Draw(trampolineTexture, Game1.GlobalToLocal(Game1.viewport, ___drawPosition.Value + ((__instance.shakeTimer > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero)) + new Vector2(0, 120), new Rectangle?(cutoffRect2), Color.White * alpha, 0f, Vector2.Zero, 4f, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((float)(__instance.boundingBox.Value.Bottom - 48) / 10000f));
                    }
                    else
                    {
                        spriteBatch.Draw(trampolineTexture, Game1.GlobalToLocal(Game1.viewport, ___drawPosition.Value + ((__instance.shakeTimer > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero)), new Rectangle?(drawn_source_rect), Color.White * alpha, 0f, Vector2.Zero, 4f, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 2E-09f + __instance.TileLocation.Y / 100000f);
                    }
                }
                else
                {
                    spriteBatch.Draw(trampolineTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)), (float)(y * 64 - (__instance.sourceRect.Height * 4 - __instance.boundingBox.Height) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)))), new Rectangle?(drawn_source_rect), Color.White * alpha, 0f, Vector2.Zero, 4f, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 2E-09f + __instance.TileLocation.Y / 100000f);
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(Furniture), nameof(Furniture.GetSeatPositions))]
        public class Furniture_GetSeatPositions_Patch
        {
            public static bool Prefix(Furniture __instance, ref List<Vector2> __result)
            {
                if (!Config.EnableMod || !__instance.modData.ContainsKey(trampolineKey))
                    return true;
                __result = new List<Vector2>()
                {
                    __instance.TileLocation + new Vector2(0.5f, 0.5f)
                };
                return false;
            }
        }
        [HarmonyPatch(typeof(Furniture), nameof(Furniture.GetSeatCapacity))]
        public class Furniture_GetSeatCapacity_Patch
        {
            public static bool Prefix(Furniture __instance, ref int __result)
            {
                if (!Config.EnableMod || !__instance.modData.ContainsKey(trampolineKey))
                    return true;
                __result = 1;
                return false;
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.draw), new Type[] { typeof(SpriteBatch) })]
        public class Farmer_draw_Patch
        {
            public static void Prefix(Farmer __instance, ref bool __state)
            {
                if (!Config.EnableMod || !__instance.modData.ContainsKey(trampolineKey) || !IsOnTrampoline(__instance))
                    return;

            }
            public static void Postfix(Farmer __instance, bool __state)
            {
                if (!Config.EnableMod || !__instance.modData.ContainsKey(trampolineKey) || !__state)
                    return;
                
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.ShowSitting))]
        public class Farmer_ShowSitting_Patch
        {
            public static bool Prefix(Farmer __instance)
            {
                return !Config.EnableMod || !IsOnTrampoline(__instance);
            }
        }
    }
}