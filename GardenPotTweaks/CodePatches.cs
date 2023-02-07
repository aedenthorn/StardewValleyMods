using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using xTile.Dimensions;
using xTile.Tiles;
using xTile;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using System.Text;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;
using StardewValley.Tools;
using StardewValley.Network;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Netcode;
using Color = Microsoft.Xna.Framework.Color;

namespace GardenPotTweaks
{
    public partial class ModEntry
    {
        public static void PatchAll(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.findCloseFlower), new Type[] { typeof(GameLocation), typeof(Vector2), typeof(int), typeof(Func<Crop, bool>) }),
                postfix: new HarmonyMethod(typeof(Utility_findCloseFlower_Patch), nameof(Utility_findCloseFlower_Patch.Postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.ApplySprinkler)),
                postfix: new HarmonyMethod(typeof(Object_ApplySprinkler_Patch), nameof(Object_ApplySprinkler_Patch.Postfix))
            );

            //harmony.Patch(
            //    original: AccessTools.Method(typeof(Axe), nameof(Axe.DoFunction)),
            //    prefix: new HarmonyMethod(typeof(Axe_DoFunction_Patch), nameof(Axe_DoFunction_Patch.Prefix))
            //);

            harmony.Patch(
                original: AccessTools.Method(typeof(Pickaxe), nameof(Pickaxe.DoFunction)),
                prefix: new HarmonyMethod(typeof(Pickaxe_DoFunction_Patch), nameof(Pickaxe_DoFunction_Patch.Prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.maximumStackSize)),
                postfix: new HarmonyMethod(typeof(Object_maximumStackSize_Patch), nameof(Object_maximumStackSize_Patch.Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.drawInMenu), new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) }),
                postfix: new HarmonyMethod(typeof(Object_drawInMenu_Patch), nameof(Object_drawInMenu_Patch.Postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.drawWhenHeld)),
                postfix: new HarmonyMethod(typeof(Object_drawWhenHeld_Patch), nameof(Object_drawWhenHeld_Patch.Postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(IndoorPot), nameof(IndoorPot.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
                prefix: new HarmonyMethod(typeof(IndoorPot_draw_Patch), nameof(IndoorPot_draw_Patch.Prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.placementAction)),
                prefix: new HarmonyMethod(typeof(Object_placementAction_Patch), nameof(Object_placementAction_Patch.Prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Object), "loadDisplayName"),
                postfix: new HarmonyMethod(typeof(Object_loadDisplayName_Patch), nameof(Object_loadDisplayName_Patch.Postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(IndoorPot), nameof(IndoorPot.performObjectDropInAction)),
                prefix: new HarmonyMethod(typeof(IndoorPot_performObjectDropInAction_Patch), nameof(IndoorPot_performObjectDropInAction_Patch.Prefix))
            );
        }

        public class Utility_findCloseFlower_Patch
        {
            public static void Postfix(GameLocation location, Vector2 startTileLocation, int range, Func<Crop, bool> additional_check, ref Crop __result)
            {
                if (!Config.ModEnabled || !Config.EnableHoney)
                    return;
                Vector2 tilePos = __result is null ? Vector2.Zero : AccessTools.FieldRefAccess<Crop, Vector2>(__result, "tilePosition");
                float closestDistance = Vector2.Distance(startTileLocation, tilePos);
                foreach (var kvp in location.objects.Pairs)
                {
                    if (kvp.Value is not IndoorPot || (kvp.Value as IndoorPot).hoeDirt.Value?.crop is null || new Object((kvp.Value as IndoorPot).hoeDirt.Value.crop.indexOfHarvest.Value, 1, false, -1, 0).Category != -80 || (kvp.Value as IndoorPot).hoeDirt.Value.crop.currentPhase.Value < (kvp.Value as IndoorPot).hoeDirt.Value.crop.phaseDays.Count - 1 || (kvp.Value as IndoorPot).hoeDirt.Value.crop.dead.Value || (additional_check != null && !additional_check((kvp.Value as IndoorPot).hoeDirt.Value.crop)))
                        continue;

                    if (Config.FixFlowerFind)
                    {
                        var distance = Vector2.Distance(startTileLocation, kvp.Key);
                        if (distance <= range && distance < closestDistance)
                        {
                            closestDistance = distance;
                            __result = (kvp.Value as IndoorPot).hoeDirt.Value.crop;
                            AccessTools.FieldRefAccess<Crop, Vector2>(__result, "tilePosition") = kvp.Key;
                        }
                    }
                    else
                    {
                        if (__result is null)
                        {
                            if (range < 0 || Math.Abs(kvp.Key.X - startTileLocation.X) + Math.Abs(kvp.Key.Y - startTileLocation.Y) <= range)
                            {
                                tilePos = kvp.Key;
                                __result = (kvp.Value as IndoorPot).hoeDirt.Value.crop;
                                AccessTools.FieldRefAccess<Crop, Vector2>(__result, "tilePosition") = kvp.Key;
                            }
                        }
                        else if (Vector2.Distance(startTileLocation, kvp.Key) < Vector2.Distance(tilePos, startTileLocation))
                        {
                            __result = (kvp.Value as IndoorPot).hoeDirt.Value.crop;
                            tilePos = kvp.Key;
                            AccessTools.FieldRefAccess<Crop, Vector2>(__result, "tilePosition") = kvp.Key;
                        }
                    }
                }
            }
        }
        
        public class Object_ApplySprinkler_Patch
        {
            private static readonly WateringCan Can = new() { WaterLeft = 100, IsBottomless = true };

            public static void Postfix(GameLocation location, Vector2 tile)
            {
                if (!Config.ModEnabled || !Config.EnableSprinklering)
                    return;
                if (location.objects.TryGetValue(tile, out var obj) && obj is IndoorPot pot && pot.hoeDirt.Value.state.Value != 2)
                {
                    // water via fake watering can since sprinklers don't update garden pot visuals
                    pot.performToolAction(Can, location);
                }
            }
        }
        
        public class Axe_DoFunction_Patch
        {
            public static void Prefix(Axe __instance, GameLocation location, int x, int y, int power, Farmer who)
            {
                if (!Config.ModEnabled || !Config.EnableMoving)
                    return;
                int tileX = x / 64;
                int tileY = y / 64;
                Vector2 toolTilePosition = new Vector2((float)tileX, (float)tileY);
                if (location.Objects.TryGetValue(toolTilePosition, out var obj) && obj is IndoorPot && IsPotModified(obj as IndoorPot))
                {
                    location.debris.Add(new Debris(obj as IndoorPot, who.GetToolLocation(false), new Vector2((float)who.GetBoundingBox().Center.X, (float)who.GetBoundingBox().Center.Y)));
                    location.Objects[toolTilePosition].performRemoveAction(toolTilePosition, location);
                    location.Objects.Remove(toolTilePosition);
                }
            }
        }
        
        public class Pickaxe_DoFunction_Patch
        {
            public static void Prefix(Pickaxe __instance, GameLocation location, int x, int y, int power, Farmer who)
            {
                if (!Config.ModEnabled || !Config.EnableMoving)
                    return;
                int tileX = x / 64;
                int tileY = y / 64;
                Vector2 toolTilePosition = new Vector2((float)tileX, (float)tileY);
                if (location.Objects.TryGetValue(toolTilePosition, out var obj) && obj is IndoorPot && IsPotModified(obj as IndoorPot))
                {
                    var d = new Debris(obj as IndoorPot, who.GetToolLocation(false), new Vector2((float)who.GetBoundingBox().Center.X, (float)who.GetBoundingBox().Center.Y));
                    location.debris.Add(d);
                    location.Objects[toolTilePosition].performRemoveAction(toolTilePosition, location);
                    location.Objects.Remove(toolTilePosition);
                }
            }

        }

        public class Object_maximumStackSize_Patch
        {
            public static void Postfix(Object __instance, ref int __result)
            {
                if (!Config.ModEnabled || !Config.EnableMoving || __instance is not IndoorPot pot || !IsPotModified(pot))
                    return;
                __result = 1;
            }
        }
        
        public class Object_drawInMenu_Patch
        {
            public static void Postfix(Object __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
            {
                if (!Config.ModEnabled || __instance is not IndoorPot pot || !IsPotModified(pot))
                    return;

                var crop = pot.hoeDirt.Value.crop;
                var bush = pot.bush.Value;
                if(crop != null)
                {
                    var drawLocation = location + new Vector2(32f, 32f) + new Vector2(1, 2f);
                    var sourceRect = AccessTools.FieldRefAccess<Crop, Rectangle>(crop, "sourceRect");
                    if (sourceRect == Rectangle.Empty)
                    {
                       pot.hoeDirt.Value.crop.updateDrawMath(Vector2.One);
                        sourceRect = AccessTools.FieldRefAccess<Crop, Rectangle>(crop, "sourceRect");
                    }
                    var coloredSourceRect = AccessTools.FieldRefAccess<Crop, Rectangle>(crop, "coloredSourceRect");
                    var rotation = pot.hoeDirt.Value.getShakeRotation();
                    if (crop.forageCrop.Value)
                    {
                        spriteBatch.Draw(Game1.mouseCursors, drawLocation, new Rectangle?(sourceRect), Color.White, 0f, new Vector2(8f, 8f), 2f, SpriteEffects.None, layerDepth + 0.0001f);
                        return;
                    }
                    spriteBatch.Draw(Game1.cropSpriteSheet, drawLocation, new Rectangle?(sourceRect), (pot.hoeDirt.Value.state.Value == 1 && pot.hoeDirt.Value.crop.currentPhase.Value == 0 && !pot.hoeDirt.Value.crop.raisedSeeds.Value) ? (new Color(180, 100, 200) * 1f) : Color.White, rotation, new Vector2(8f, 24f), 2f, crop.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 0.0001f);
                    if (!crop.tintColor.Value.Equals(Color.White) && crop.currentPhase.Value == crop.phaseDays.Count - 1 && !crop.dead.Value)
                    {
                        spriteBatch.Draw(Game1.cropSpriteSheet, drawLocation, new Rectangle?(coloredSourceRect), crop.tintColor.Value, rotation, new Vector2(8f, 24f), 2f, crop.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 0.0002f);
                    }
                }
                if (pot.hoeDirt.Value.fertilizer.Value != 0)
                {
                    var drawLocation = location + new Vector2(16f, 32f) + new Vector2(1, 2f);

                    Rectangle fertilizer_rect = pot.hoeDirt.Value.GetFertilizerSourceRect(pot.hoeDirt.Value.fertilizer.Value);
                    fertilizer_rect.Width = 13;
                    fertilizer_rect.Height = 13;
                    spriteBatch.Draw(Game1.mouseCursors, drawLocation + new Vector2(4f, - 12f), new Rectangle?(fertilizer_rect), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, (pot.TileLocation.Y + 0.65f) * 64f / 10000f + (float)drawLocation.X / 64 * 1E-05f);
                }
                if (bush is not null)
                {
                    var drawLocation = location + new Vector2(0, 10f);

                    var alpha = AccessTools.FieldRefAccess<Bush, float>(bush, "alpha");
                    var sourceRect = AccessTools.FieldRefAccess<Bush, NetRectangle>(bush, "sourceRect").Value;
                    var size = GetBushEffectiveSize(bush);
                    var yDrawOffset = -24f;
                    if (bush.drawShadow.Value)
                    {
                        if (size > 0)
                        {
                            spriteBatch.Draw(Game1.mouseCursors, drawLocation + new Vector2(((size == 1) ? 0.5f : 1f) * 64f - 51f, -16f + yDrawOffset), Bush.shadowSourceRect, Color.White, 0f, Vector2.Zero, 2f, bush.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1E-06f);
                        }
                        else
                        {
                            spriteBatch.Draw(Game1.shadowTexture, new Vector2(drawLocation.X + 32f, drawLocation.Y + 64f - 4f + yDrawOffset), Game1.shadowTexture.Bounds, Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 2f, SpriteEffects.None, 1E-06f);
                        }
                    }
                    spriteBatch.Draw(Bush.texture.Value, drawLocation + new Vector2((float)((size + 1) * 64 / 2), 64f - (float)((size > 0 && (!bush.townBush.Value || size != 1) && (int)size != 4) ? 64 : 0) + yDrawOffset), sourceRect, Color.White * alpha, 0, new Vector2((size + 1) * 16 / 2, 32f), 2f, bush.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(bush.getBoundingBox(location / 64).Center.Y + 48) / 10000f - drawLocation.X / 64 / 1000000f);
                }

            }
        }
        
        public class Object_drawWhenHeld_Patch
        {
            public static void Postfix(Object __instance, SpriteBatch spriteBatch, Vector2 objectPosition)
            {
                if (!Config.ModEnabled || __instance is not IndoorPot pot || !IsPotModified(pot))
                    return;
                var tileLocation = new Vector2(objectPosition.X + Game1.viewport.X, objectPosition.Y + Game1.viewport.Y + 64) / 64;
                var crop = pot.hoeDirt.Value.crop;
                var bush = pot.bush.Value;
                if (crop != null)
                {
                    var offset = new Vector2(32f, 8f);
                    var sourceRect = AccessTools.FieldRefAccess<Crop, Rectangle>(crop, "sourceRect");
                    var coloredSourceRect = AccessTools.FieldRefAccess<Crop, Rectangle>(crop, "coloredSourceRect");
                    var rotation = pot.hoeDirt.Value.getShakeRotation();
                    if (crop.forageCrop.Value)
                    {
                        spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle?(sourceRect), Color.White, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (tileLocation.Y + 10.99f) * 64f / 10000f + tileLocation.X * 1E-05f);
                        return;
                    }
                    spriteBatch.Draw(Game1.cropSpriteSheet, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle?(sourceRect), (pot.hoeDirt.Value.state.Value == 1 && pot.hoeDirt.Value.crop.currentPhase.Value == 0 && !pot.hoeDirt.Value.crop.raisedSeeds.Value) ? (new Color(180, 100, 200) * 1f) : Color.White, rotation, new Vector2(8f, 24f), 4f, crop.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (tileLocation.Y + 10.99f) * 64f / 10000f + tileLocation.X * 1E-05f);
                    if (!crop.tintColor.Value.Equals(Color.White) && crop.currentPhase.Value == crop.phaseDays.Count - 1 && !crop.dead.Value)
                    {
                        spriteBatch.Draw(Game1.cropSpriteSheet, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle?(coloredSourceRect), crop.tintColor.Value, rotation, new Vector2(8f, 24f), 4f, crop.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (tileLocation.Y + 11f) * 64f / 10000f + tileLocation.X * 1E-05f);
                    }
                }
                if (pot.hoeDirt.Value.fertilizer.Value != 0)
                {
                    Rectangle fertilizer_rect = pot.hoeDirt.Value.GetFertilizerSourceRect(pot.hoeDirt.Value.fertilizer.Value);
                    fertilizer_rect.Width = 13;
                    fertilizer_rect.Height = 13;
                    spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 4f, tileLocation.Y * 64f - 12f)), new Rectangle?(fertilizer_rect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y + 0.65f) * 64f / 10000f + (float)tileLocation.X * 1E-02f);
                }
                if (bush is not null)
                {
                    var drawLocation = objectPosition + new Vector2(0, 64f);

                    var alpha = AccessTools.FieldRefAccess<Bush, float>(bush, "alpha");
                    var sourceRect = AccessTools.FieldRefAccess<Bush, NetRectangle>(bush, "sourceRect").Value;
                    var size = GetBushEffectiveSize(bush);
                    var yDrawOffset = -24f;
                    if (bush.drawShadow.Value)
                    {
                        if (size > 0)
                        {
                            spriteBatch.Draw(Game1.mouseCursors, drawLocation + new Vector2(((size == 1) ? 0.5f : 1f) * 64f - 51f, -16f + yDrawOffset), Bush.shadowSourceRect, Color.White, 0f, Vector2.Zero, 2f, bush.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1E-06f);
                        }
                        else
                        {
                            spriteBatch.Draw(Game1.shadowTexture, new Vector2(drawLocation.X + 32f, drawLocation.Y + 64f - 4f + yDrawOffset), Game1.shadowTexture.Bounds, Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 1E-06f);
                        }
                    }
                    spriteBatch.Draw(Bush.texture.Value, drawLocation + new Vector2((float)((size + 1) * 64 / 2), 64f - (float)((size > 0 && (!bush.townBush.Value || size != 1) && (int)size != 4) ? 64 : 0) + yDrawOffset), sourceRect, Color.White * alpha, 0, new Vector2((size + 1) * 16 / 2, 32f), 4f, bush.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(bush.getBoundingBox(objectPosition / 64).Center.Y + 48) / 10000f - drawLocation.X / 64 / 1000000f);
                }
            }
        }
        
        public class IndoorPot_draw_Patch
        {
            public static void Prefix(IndoorPot __instance, int x, int y)
            {
                if (!Config.ModEnabled)
                    return;
                __instance.TileLocation = new Vector2(x, y);
            }
        }
        
        public class Object_placementAction_Patch
        {
            public static bool Prefix(Object __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || !Config.EnableMoving || __instance is not IndoorPot pot || !IsPotModified(pot))
                    return true;
                location.objects.Add(new Vector2((float)(x / 64), (float)(y / 64)), pot);
                location.playSound("woodyStep", NetAudio.SoundContext.Default);
                __result = true;
                return false;
            }
        }

        public class Object_loadDisplayName_Patch
        {
            public static void Postfix(Object __instance, ref string __result)
            {
                if (!Config.ModEnabled || !Config.EnableMoving || __instance is not IndoorPot pot || !IsPotModified(pot))
                    return;
                var crop = pot.hoeDirt.Value.crop;
                var bush = pot.bush.Value;
                if (crop is not null)
                {
                    __result += $" ({new Object(pot.hoeDirt.Value.crop.indexOfHarvest.Value, 1).Name})";
                }
                else if (bush is not null)
                {
                    __result += $" ({pot.bush.Value.GetType().Name})";
                }
            }
        }
        
        public class IndoorPot_performObjectDropInAction_Patch
        {
            public static bool Prefix(IndoorPot __instance, Item dropInItem, bool probe, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled)
                    return true;
                if (Config.EnableAncientSeeds && dropInItem.ParentSheetIndex == 499)
                {
                    if (!probe)
                    {
                        if (!__instance.hoeDirt.Value.plant(dropInItem.ParentSheetIndex, (int)__instance.TileLocation.X, (int)__instance.TileLocation.Y, who, dropInItem.Category == -19, who.currentLocation))
                        {
                            __result = false;
                            return false;
                        }
                    }
                    else
                    {
                        __instance.heldObject.Value = new Object();
                    }
                    __result = true;
                    return false;
                }
                return true;
            }
        }
    }
}