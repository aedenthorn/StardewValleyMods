using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Projectiles;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace CoinCollector
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(MeleeWeapon), nameof(MeleeWeapon.doSwipe))]
        private static class MeleeWeapon_doSwipe_Patch
        {
            private static void Prefix(MeleeWeapon __instance)
            {
                if (!Config.ModEnabled || __instance.Name != Config.MetalDetectorID)
                    return;
                DoBlip();
            }
        }


        [HarmonyPatch(typeof(Projectile), nameof(Projectile.isColliding))]
        private static class Projectile_isColliding_Patch
        {
            public static bool Prefix(Projectile __instance, ref bool __result)
            {
                if (!Config.ModEnabled || !(__instance is IndicatorProjectile))
                    return true;
                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.makeHoeDirt))]
        private static class GameLocation_makeHoeDirt_Patch
        {
            public static void Prefix(GameLocation __instance, Vector2 tileLocation, bool ignoreChecks)
            {
                if (!Config.ModEnabled || !coinLocationDict.ContainsKey(__instance.Name) || !coinLocationDict[__instance.Name].ContainsKey(tileLocation) || (!ignoreChecks && (__instance.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Diggable", "Back") == null || __instance.isTileOccupied(tileLocation, "", false) || !__instance.isTilePassable(new Location((int)tileLocation.X, (int)tileLocation.Y), Game1.viewport))))
                    return;
                var data = coinDataDict[coinLocationDict[__instance.Name][tileLocation]];
                context.Monitor.Log($"Digging up {data.id}");
                Game1.createObjectDebris(data.parentSheetIndex, (int)tileLocation.X, (int)tileLocation.Y, -1, 0, 1f, null);
                coinLocationDict[__instance.Name].Remove(tileLocation);
            }
        }

        [HarmonyPatch(typeof(CraftingPage), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(List<Chest>) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class CraftingPage_Patch
        {
            public static void Prefix()
            {
                if (!Config.ModEnabled || Game1.player.craftingRecipes.ContainsKey(detectorName))
                    return;
                Game1.player.craftingRecipes.Add(detectorName, 0);
            }
        }
        [HarmonyPatch(typeof(CraftingPage), "layoutRecipes")]
        public class CraftingPage_layoutRecipes_Patch
        {
            public static void Postfix(CraftingPage __instance, bool ___cooking)
            {
                if (!Config.ModEnabled || ___cooking || __instance.pagesOfCraftingRecipes.Count == 0)
                    return;
                foreach (var key in __instance.pagesOfCraftingRecipes[__instance.pagesOfCraftingRecipes.Count - 1].Keys.ToList())
                {
                    if (__instance.pagesOfCraftingRecipes[__instance.pagesOfCraftingRecipes.Count - 1][key].name == detectorName)
                    {
                        var cc = key;
                        var recipe = __instance.pagesOfCraftingRecipes[__instance.pagesOfCraftingRecipes.Count - 1][key];
                        cc.texture = detectorTexture;
                        cc.sourceRect = new Rectangle(0, 0, 16, 16);
                        __instance.pagesOfCraftingRecipes[__instance.pagesOfCraftingRecipes.Count - 1].Remove(key);
                        __instance.pagesOfCraftingRecipes[__instance.pagesOfCraftingRecipes.Count - 1][cc] = recipe;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(CraftingRecipe), nameof(CraftingRecipe.createItem))]
        public class CraftingRecipe_createItem_Patch
        {
            public static void Postfix(CraftingRecipe __instance, ref Item __result)
            {
                if (!Config.ModEnabled || __instance.name != detectorName)
                    return;
                __result.modData[texturePath] = "true";
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.drawWhenHeld))]
        public class Object_drawWhenHeld_Patch
        {
            public static bool Prefix(Object __instance, SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
            {
                if (!Config.ModEnabled || !__instance.modData.ContainsKey(texturePath))
                    return true;
                spriteBatch.Draw(detectorTexture, objectPosition + new Vector2(0, 92), new Rectangle(32, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (f.getStandingY() + 3) / 10000f));
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.drawInMenu))]
        public class Object_drawInMenu_Patch
        {
            public static bool Prefix(Object __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
            {
                if (!Config.ModEnabled || detectorTexture is null || !__instance.modData.ContainsKey(texturePath))
                    return true;
                spriteBatch.Draw(detectorTexture, location + new Vector2(32f, 64f), new Rectangle(0, 0, 16, 16), color * transparency, 0f, new Vector2(8f, 16f), 8f * (((double)scaleSize < 0.2) ? scaleSize : (scaleSize / 2f)), SpriteEffects.None, layerDepth);
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public class Object_draw_Patch_1
        {
            public static bool Prefix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
            {
                if (!Config.ModEnabled || !__instance.modData.ContainsKey(texturePath))
                    return true;
                Vector2 scaleFactor = __instance.getScale();
                scaleFactor *= 16f;
                Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64)));
                Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
                float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
                spriteBatch.Draw(detectorTexture, destination, new Rectangle(0, 0, 16, 16), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float), typeof(float) })]
        public class Object_draw_Patch_2
        {
            public static bool Prefix(Object __instance, SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha)
            {
                if (!Config.ModEnabled || !__instance.modData.ContainsKey(texturePath))
                    return true;
                Vector2 scaleFactor = __instance.getScale();
                scaleFactor *= 4f;
                Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)xNonTile, (float)yNonTile));
                Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
                spriteBatch.Draw(detectorTexture, destination, new Rectangle(0, 0, 16, 16), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
                return false;
            }
        }
    }
}