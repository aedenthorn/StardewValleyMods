using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.Tools;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using xTile.Dimensions;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using System;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Object = StardewValley.Object;
using StardewValley.Characters;

namespace FloatingGardenPots
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled)
                    return true;
                if (who.ActiveObject?.bigCraftable.Value != true || who.ActiveObject.ParentSheetIndex != 62)
                    return true;
                Vector2 p = new Vector2(tileLocation.X, tileLocation.Y);
                if(!CrabPot.IsValidCrabPotLocationTile(__instance, tileLocation.X, tileLocation.Y)) 
                    return true;
                __result = true;
                var pot = new IndoorPot(p);
                pot.hoeDirt.Value.state.Value = 1;
                pot.showNextIndex.Value = true;
                pot.modData[modKey] = "true";
                __instance.objects.Add(p, pot);
                who.reduceActiveItemByOne();
                __instance.playSound("dropItemInWater");
                SMonitor.Log($"Placed garden pot at {p}");
                return false;
            }
        }
        [HarmonyPatch(typeof(IndoorPot), nameof(IndoorPot.DayUpdate))]
        public class IndoorPot_DayUpdate_Patch
        {
            public static void Postfix(IndoorPot __instance, GameLocation location)
            {
                if (!Config.ModEnabled || !location.isWaterTile((int)__instance.TileLocation.X, (int)__instance.TileLocation.Y))
                    return;
                __instance.hoeDirt.Value.state.Value = 1;
                __instance.showNextIndex.Value = true;
            }
        }
        [HarmonyPatch(typeof(IndoorPot), nameof(IndoorPot.draw))]
        public class IndoorPot_draw_Patch
        {
            public static bool Prefix(IndoorPot __instance, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
            {
                if (!Config.ModEnabled || !__instance.modData.ContainsKey(modKey))
                    return true;
                var yBob = (float)(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 500.0 + (double)(x * 64)) * 8.0 + 8.0);
                Vector2 offset = GetPotOffset(Game1.currentLocation, new Vector2(x, y));
                offset.Y += yBob;
                if (yBob <= 0.001f)
                {
                    Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 150f, 8, 0, offset + new Vector2((float)(x * 64 + 4), (float)(y * 64 + 32)), false, Game1.random.NextDouble() < 0.5, 0.001f, 0.01f, Color.White, 0.75f, 0.003f, 0f, 0f, false));
                }
                Vector2 scaleFactor = __instance.getScale();
                scaleFactor *= 4f;
                Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64))) + offset;
                Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
                spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destination, new Rectangle?(Object.getSourceRectForBigCraftable(__instance.showNextIndex.Value ? (__instance.ParentSheetIndex + 1) : __instance.ParentSheetIndex)), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + ((__instance.ParentSheetIndex == 105) ? 0.0035f : 0f) + (float)x * 1E-05f);
                if (__instance.hoeDirt.Value.fertilizer.Value != 0)
                {
                    Rectangle fertilizer_rect = __instance.hoeDirt.Value.GetFertilizerSourceRect(__instance.hoeDirt.Value.fertilizer.Value);
                    fertilizer_rect.Width = 13;
                    fertilizer_rect.Height = 13;
                    spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(__instance.TileLocation.X * 64f + 4f, __instance.TileLocation.Y * 64f - 12f)), new Rectangle?(fertilizer_rect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (__instance.TileLocation.Y + 0.65f) * 64f / 10000f + (float)x * 1E-05f);
                }
                if (__instance.hoeDirt.Value.crop != null)
                {
                    __instance.hoeDirt.Value.crop.drawWithOffset(spriteBatch, __instance.TileLocation + offset / 64f, (__instance.hoeDirt.Value.state.Value == 1 && __instance.hoeDirt.Value.crop.currentPhase.Value == 0 && !__instance.hoeDirt.Value.crop.raisedSeeds.Value) ? (new Color(180, 100, 200) * 1f) : Color.White, __instance.hoeDirt.Value.getShakeRotation(), new Vector2(32f, 8f));
                }
                if (__instance.heldObject.Value != null)
                {
                    __instance.heldObject.Value.draw(spriteBatch, x * 64 + (int)offset.X, y * 64 - 48 + (int)offset.Y, (__instance.TileLocation.Y + 0.66f) * 64f / 10000f + (float)x * 1E-05f, 1f);
                }
                if (__instance.bush.Value != null)
                {
                    __instance.bush.Value.draw(spriteBatch, new Vector2((float)x, (float)y) + offset / 64f, -24f);
                }
                return false;
            }

        }
    }
}