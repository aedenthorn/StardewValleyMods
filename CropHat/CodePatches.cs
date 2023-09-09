using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace CropHat
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(FarmerRenderer), nameof(FarmerRenderer.drawHairAndAccesories))]
        public class FarmerRenderer_drawHairAndAccesories_Patch
        {
            public static void Prefix(FarmerRenderer __instance, Farmer who, ref Hat __state)
            {
                if (!Config.EnableMod || who.hat.Value is null || !who.hat.Value.modData.ContainsKey(seedKey))
                    return;
                __state = who.hat.Value;
                who.hat.Value = null;
            }
            public static void Postfix(FarmerRenderer __instance, Vector2 ___positionOffset, Hat __state, SpriteBatch b, int facingDirection, Farmer who, Vector2 position, Vector2 origin, float scale, int currentFrame, float rotation, Color overrideColor, float layerDepth)
            {
                if (!Config.EnableMod || __state is null)
                    return;
                who.hat.Value = __state;
                //string phase = who.hat.Value.modData[phaseKey];
                //string days = who.hat.Value.modData[daysKey];
                bool flip = who.FarmerSprite.CurrentAnimationFrame.flip;
				float layer_offset = 3.9E-05f;
                var sourceRect = new Rectangle(Convert.ToInt32(who.hat.Value.modData[xKey]), Convert.ToInt32(who.hat.Value.modData[yKey]), 16, 32);

                b.Draw(Game1.cropSpriteSheet, position + origin + ___positionOffset + new Vector2((float)(-8 + (flip ? -1 : 1) * FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4), (float)(-16 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (who.hat.Value.ignoreHairstyleOffset.Value ? 0 : FarmerRenderer.hairstyleHatOffset[who.hair.Value % 16]) + 4 + __instance.heightOffset.Value)) + new Vector2(8, -80), sourceRect, Color.White, rotation, origin, 4f * scale, who.FacingDirection < 2 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + layer_offset);
            }
        }
        [HarmonyPatch(typeof(Hat), nameof(Hat.draw))]
        public class Hat_draw_Patch
        {
            public static bool Prefix(Hat __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, int direction)
            {
                if (!Config.EnableMod || !(__instance.modData.TryGetValue(seedKey, out string seedIndex)))
                    return true;
                spriteBatch.Draw(Game1.cropSpriteSheet, location + new Vector2(10f, 10f), new Rectangle?(new Rectangle(Convert.ToInt32(__instance.modData[xKey]), Convert.ToInt32(__instance.modData[yKey]), 16, 32)), Color.White * transparency, 0f, new Vector2(3f, 3f), 3f * scaleSize, SpriteEffects.None, layerDepth);
                return false;
            }
        }
        [HarmonyPatch(typeof(Hat), nameof(Hat.drawInMenu))]
        public class Hat_drawInMenu_Patch
        {
            public static bool Prefix(Hat __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
            {
                if (!Config.EnableMod || !(__instance.modData.TryGetValue(seedKey, out string seedIndex)))
                    return true;
                scaleSize *= 0.75f;
                spriteBatch.Draw(Game1.cropSpriteSheet, location + new Vector2(38f, 0), new Rectangle?(new Rectangle(Convert.ToInt32(__instance.modData[xKey]), Convert.ToInt32(__instance.modData[yKey]), 16, 32)), color * transparency, 0f, new Vector2(10f, 10f), 4f * scaleSize, SpriteEffects.None, layerDepth);
                return false;
            }
        }
        [HarmonyPatch(typeof(Hat), "loadDisplayFields")]
        public class Hat_loadDisplayFields_Patch
        {
            public static bool Prefix(Hat __instance, ref bool __result)
            {
                if (!Config.EnableMod || !__instance.modData.TryGetValue(seedKey, out string seedIndex))
                    return true;
                Dictionary<int, string> cropData = Game1.content.Load<Dictionary<int, string>>("Data\\Crops");
                if (!cropData.TryGetValue(Convert.ToInt32(seedIndex), out string cropString))
                    return true;
                string[] split = cropString.Split('/', StringSplitOptions.None);
                var harvestIndex = Convert.ToInt32(split[3]);
                if (!Game1.objectInformation.TryGetValue(harvestIndex, out string objectInformation))
                    return true;
                split = objectInformation.Split('/', StringSplitOptions.None);
                __instance.displayName = string.Format(SHelper.Translation.Get("x-hat"), split[4]);
                __instance.description = split[5];
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
            {
                if (!Config.EnableMod)
                    return true;
                if (Config.AllowOthersToPick)
                {
                    foreach (Farmer farmer in Game1.getAllFarmers())
                    {
                        var loc = farmer.Position + new Vector2(32, -88);
                        if (who.currentLocation == farmer.currentLocation && farmer.hat.Value is not null && Vector2.Distance(Game1.GlobalToLocal(loc), Game1.getMousePosition().ToVector2()) < 32 && farmer.hat.Value.modData.TryGetValue(phaseKey, out string phaseString))
                        {
                            if (ReadyToHarvest(farmer.hat.Value) && Utility.withinRadiusOfPlayer((int)farmer.Position.X, (int)farmer.Position.Y, 1, Game1.player))
                            {
                                HarvestHatCrop(farmer);
                                __result = true;
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    var loc = Game1.player.Position + new Vector2(32, -88);
                    if (Vector2.Distance(Game1.GlobalToLocal(loc), Game1.getMousePosition().ToVector2()) < 32 && Game1.player.hat.Value is not null && Game1.player.hat.Value.modData.TryGetValue(phaseKey, out string phaseString))
                    {
                        if (ReadyToHarvest(Game1.player.hat.Value))
                        {
                            HarvestHatCrop(Game1.player);
                            __result = true;
                            return false;
                        }
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(InventoryPage), nameof(InventoryPage.receiveLeftClick))]
        public class InventoryPage_receiveLeftClick_Patch
        {
            public static bool Prefix(InventoryPage __instance, int x, int y)
            {
                if (!Config.EnableMod || (Game1.player.CursorSlotItem is null) || Game1.player.hat.Value is not null)
                    return true;
                Dictionary<int, string> cropData = Game1.content.Load<Dictionary<int, string>>("Data\\Crops");
                if (!cropData.TryGetValue(Game1.player.CursorSlotItem.ParentSheetIndex, out string cropString))
                    return true;


                foreach (ClickableComponent c in __instance.equipmentIcons)
                {
                    if (c.name == "Hat" && c.containsPoint(x, y))
                    {
                        SMonitor.Log($"Trying to wear {Game1.player.CursorSlotItem.Name}");
                        string[] split = cropString.Split('/');
                        int row = Convert.ToInt32(split[2]);
                        Hat hat = new Hat(0);
                        hat.modData[seedKey] = "" + Game1.player.CursorSlotItem.ParentSheetIndex;
                        hat.modData[daysKey] = "0";
                        hat.modData[phaseKey] = "0";
                        hat.modData[phasesKey] = (split[0].Split(' ').Length + 1)+"";
                        hat.modData[rowKey] = row+"";
                        hat.modData[grownKey] = "false";
                        hat.modData[xKey] = GetSourceX(row, 0, 0, false, false) + "";
                        hat.modData[yKey] = GetSourceY(row) + "";

                        var harvestIndex = Convert.ToInt32(split[3]);
                        if (Game1.objectInformation.TryGetValue(harvestIndex, out string objectInformation))
                        {
                            var split2 = objectInformation.Split('/', StringSplitOptions.None);
                            hat.displayName = string.Format(SHelper.Translation.Get("x-hat"), split2[4]);
                            hat.description = split2[5];
                        }

                        Game1.player.CursorSlotItem.Stack--;
                        if (Game1.player.CursorSlotItem.Stack <= 0)
                            Game1.player.CursorSlotItem = null;
                        Game1.player.hat.Value = hat;
                        Game1.playSound("grassyStep");
                        return false;
                    }
                }
                return true;
            }
        }
    }
}