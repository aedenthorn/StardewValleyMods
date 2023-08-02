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
using static StardewValley.Minigames.CraneGame;
using StardewValley.Locations;

namespace StatueShorts
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch),typeof(int),typeof(int),typeof(float) })]
        public class Object_draw_Patch_1
        {
            public static bool Prefix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
            {
                if (!Config.ModEnabled || !__instance.bigCraftable.Value || __instance.Name != "Solid Gold Lewis" || !__instance.modData.TryGetValue(modKey, out var which))
                    return true;
                Vector2 scaleFactor = __instance.getScale();
                scaleFactor *= 4f;
                Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64)));
                Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
                float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
                
                spriteBatch.Draw(SHelper.ModContent.Load<Texture2D>($"assets/{which}.png"), destination, null, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch),typeof(int),typeof(int),typeof(float),typeof(float) })]
        public class Object_draw_Patch_2
        {
            public static bool Prefix(Object __instance, SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha)
            {
                if (!Config.ModEnabled || __instance.isTemporarilyInvisible || !__instance.bigCraftable.Value || __instance.Name != "Solid Gold Lewis" || !__instance.modData.TryGetValue(modKey, out var which))
                    return true; 
                
                Vector2 scaleFactor = __instance.getScale();
                scaleFactor *= 4f;
                Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)xNonTile, (float)yNonTile));
                Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
                spriteBatch.Draw(SHelper.ModContent.Load<Texture2D>($"assets/{which}.png"), destination, null, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.DayUpdate))]
        public class Object_DayUpdate_Patch
        {
            public static bool Prefix(Object __instance, GameLocation location)
            {
                if (!Config.ModEnabled || __instance.isTemporarilyInvisible || !__instance.bigCraftable.Value || __instance.Name != "Solid Gold Lewis" || !__instance.modData.TryGetValue(modKey, out var which) || location is not Town)
                    return true;
                var obj = new Object(Vector2.Zero, 164, false);
                obj.modData[modKey] = which;
                if (Game1.random.NextDouble() < 0.9)
                {
                    if (Game1.getLocationFromName("ManorHouse").isTileLocationTotallyClearAndPlaceable(22, 6))
                    {
                        if (!Game1.player.hasOrWillReceiveMail("lewisStatue"))
                        {
                            Game1.mailbox.Add("lewisStatue");
                        }
                        Game1.getLocationFromName("ManorHouse").objects.Add(new Vector2(22f, 6f), obj);
                    }
                }
                else if (Game1.getLocationFromName("AnimalShop").isTileLocationTotallyClearAndPlaceable(11, 6))
                {
                    if (!Game1.player.hasOrWillReceiveMail("lewisStatue"))
                    {
                        Game1.mailbox.Add("lewisStatue");
                    }
                    Game1.getLocationFromName("AnimalShop").objects.Add(new Vector2(11f, 6f), obj);
                }
                __instance.rot();
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.checkForAction))]
        public class Object_checkForAction_Patch
        {
            public static bool Prefix(Object __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
            {
                if (!Config.ModEnabled || __instance.isTemporarilyInvisible || !__instance.bigCraftable.Value || __instance.Name != "Solid Gold Lewis")
                    return true;
                
                if(__instance.modData.TryGetValue(modKey, out var which))
                {
                    __result = true;
                    if (!justCheckingForActivity)
                    {
                        Game1.createObjectDebris(which == "trimmed" ? 71 : 789, (int)__instance.TileLocation.X, (int)__instance.TileLocation.Y, who.currentLocation);
                        __instance.modData.Remove(modKey);
                        Game1.playSound("dwop");
                    }
                    return false;
                }
                else if (who.ActiveObject is not null && who.ActiveObject.Name.Contains("Lucky Purple Shorts"))
                {
                    if (!justCheckingForActivity)
                    {
                        __instance.modData[modKey] = who.ActiveObject.Name.Equals("Lucky Purple Shorts") ? "shorts" : "trimmed";
                        who.reduceActiveItemByOne();
                        Game1.playSound("sandyStep");
                    }
                    __result = true;
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.performRemoveAction))]
        public class Object_performRemoveAction_Patch
        {
            public static void Prefix(Object __instance, Vector2 tileLocation, GameLocation environment)
            {
                if (!Config.ModEnabled ||!__instance.bigCraftable.Value || __instance.Name != "Solid Gold Lewis" || !__instance.modData.TryGetValue(modKey, out var which))
                    return;

                Game1.createObjectDebris(which == "trimmed" ? 71 : 789, (int)__instance.TileLocation.X, (int)__instance.TileLocation.Y, environment);

            }
        }
    }
}