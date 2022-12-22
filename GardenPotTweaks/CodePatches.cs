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

namespace GardenPotTweaks
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Utility), nameof(Utility.findCloseFlower), new Type[] { typeof(GameLocation), typeof(Vector2), typeof(int), typeof(Func<Crop, bool>) })]
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
        [HarmonyPatch(typeof(Object), nameof(Object.ApplySprinkler))]
        public class Object_ApplySprinkler_Patch
        {
            public static void Postfix(GameLocation location, Vector2 tile)
            {
                if (!Config.ModEnabled || !Config.EnableSprinklering)
                    return;
                if (location.objects.TryGetValue(tile, out var obj) && obj is IndoorPot && (obj as IndoorPot).hoeDirt.Value.state.Value != 2)
                {
                    (location.objects[tile] as IndoorPot).hoeDirt.Value.state.Value = 1;
                }
            }
        }
        //[HarmonyPatch(typeof(Axe), nameof(Axe.DoFunction))]
        public class Axe_DoFunction_Patch
        {
            public static void Prefix(Axe __instance, GameLocation location, int x, int y, int power, Farmer who)
            {
                if (!Config.ModEnabled || !Config.EnableMoving)
                    return;
                int tileX = x / 64;
                int tileY = y / 64;
                Vector2 toolTilePosition = new Vector2((float)tileX, (float)tileY);
                if (location.Objects.TryGetValue(toolTilePosition, out var obj) && obj is IndoorPot && (obj as IndoorPot).hoeDirt.Value?.crop is not null)
                {
                    location.debris.Add(new Debris(obj as IndoorPot, who.GetToolLocation(false), new Vector2((float)who.GetBoundingBox().Center.X, (float)who.GetBoundingBox().Center.Y)));
                    location.Objects[toolTilePosition].performRemoveAction(toolTilePosition, location);
                    location.Objects.Remove(toolTilePosition);
                }
            }
        }
        [HarmonyPatch(typeof(Pickaxe), nameof(Pickaxe.DoFunction))]
        public class Pickaxe_DoFunction_Patch
        {
            public static void Prefix(Pickaxe __instance, GameLocation location, int x, int y, int power, Farmer who)
            {
                if (!Config.ModEnabled || !Config.EnableMoving)
                    return;
                int tileX = x / 64;
                int tileY = y / 64;
                Vector2 toolTilePosition = new Vector2((float)tileX, (float)tileY);
                if (location.Objects.TryGetValue(toolTilePosition, out var obj) && obj is IndoorPot && (obj as IndoorPot).hoeDirt.Value?.crop is not null)
                {
                    var d = new Debris(obj as IndoorPot, who.GetToolLocation(false), new Vector2((float)who.GetBoundingBox().Center.X, (float)who.GetBoundingBox().Center.Y));
                    location.debris.Add(d);
                    location.Objects[toolTilePosition].performRemoveAction(toolTilePosition, location);
                    location.Objects.Remove(toolTilePosition);
                }
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.maximumStackSize))]
        public class Object_maximumStackSize_Patch
        {
            public static void Postfix(Object __instance, ref int __result)
            {
                if (!Config.ModEnabled || !Config.EnableMoving || __instance is not IndoorPot || (__instance as IndoorPot).hoeDirt.Value?.crop is null)
                    return;
                __result = 1;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.drawInMenu))]
        public class Object_drawInMenu_Patch
        {
            public static void Postfix(Object __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
            {
                if (!Config.ModEnabled || __instance is not IndoorPot || (__instance as IndoorPot).hoeDirt.Value?.crop is null)
                    return;
                var drawLocation = location + new Vector2(32f, 32f) + new Vector2(1, 2f);
                var crop = (__instance as IndoorPot).hoeDirt.Value.crop;
                var sourceRect = AccessTools.FieldRefAccess<Crop, Rectangle>(crop, "sourceRect");
                if(sourceRect == Rectangle.Empty)
                {
                    (__instance as IndoorPot).hoeDirt.Value.crop.updateDrawMath(Vector2.One);
                    sourceRect = AccessTools.FieldRefAccess<Crop, Rectangle>(crop, "sourceRect");
                }
                var coloredSourceRect = AccessTools.FieldRefAccess<Crop, Rectangle>(crop, "coloredSourceRect");
                var rotation = (__instance as IndoorPot).hoeDirt.Value.getShakeRotation();
                if (crop.forageCrop.Value)
                {
                    spriteBatch.Draw(Game1.mouseCursors, drawLocation, new Rectangle?(sourceRect), Color.White, 0f, new Vector2(8f, 8f), 2f, SpriteEffects.None, layerDepth + 0.0001f);
                    return;
                }
                spriteBatch.Draw(Game1.cropSpriteSheet, drawLocation, new Rectangle?(sourceRect), ((__instance as IndoorPot).hoeDirt.Value.state.Value == 1 && (__instance as IndoorPot).hoeDirt.Value.crop.currentPhase.Value == 0 && !(__instance as IndoorPot).hoeDirt.Value.crop.raisedSeeds.Value) ? (new Color(180, 100, 200) * 1f) : Color.White, rotation, new Vector2(8f, 24f), 2f, crop.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 0.0001f);
                if (!crop.tintColor.Value.Equals(Color.White) && crop.currentPhase.Value == crop.phaseDays.Count - 1 && !crop.dead.Value)
                {
                    spriteBatch.Draw(Game1.cropSpriteSheet, drawLocation, new Rectangle?(coloredSourceRect), crop.tintColor.Value, rotation, new Vector2(8f, 24f), 2f, crop.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 0.0002f);
                }

            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.drawWhenHeld))]
        public class Object_drawWhenHeld_Patch
        {
            public static void Postfix(Object __instance, SpriteBatch spriteBatch, Vector2 objectPosition)
            {
                if (!Config.ModEnabled || __instance is not IndoorPot || (__instance as IndoorPot).hoeDirt.Value?.crop is null)
                    return;
                var tileLocation = new Vector2(objectPosition.X + Game1.viewport.X, objectPosition.Y + Game1.viewport.Y + 64) / 64;
                var crop = (__instance as IndoorPot).hoeDirt.Value.crop;
                var offset = new Vector2(32f, 8f);
                var sourceRect = AccessTools.FieldRefAccess<Crop, Rectangle>(crop, "sourceRect");
                var coloredSourceRect = AccessTools.FieldRefAccess<Crop, Rectangle>(crop, "coloredSourceRect");
                var rotation = (__instance as IndoorPot).hoeDirt.Value.getShakeRotation();
                if (crop.forageCrop.Value)
                {
                    spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle?(sourceRect), Color.White, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (tileLocation.Y + 10.99f) * 64f / 10000f + tileLocation.X * 1E-05f);
                    return;
                }
                spriteBatch.Draw(Game1.cropSpriteSheet, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle?(sourceRect), ((__instance as IndoorPot).hoeDirt.Value.state.Value == 1 && (__instance as IndoorPot).hoeDirt.Value.crop.currentPhase.Value == 0 && !(__instance as IndoorPot).hoeDirt.Value.crop.raisedSeeds.Value) ? (new Color(180, 100, 200) * 1f) : Color.White, rotation, new Vector2(8f, 24f), 4f, crop.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (tileLocation.Y + 10.99f) * 64f / 10000f + tileLocation.X * 1E-05f);
                if (!crop.tintColor.Value.Equals(Color.White) && crop.currentPhase.Value == crop.phaseDays.Count - 1 && !crop.dead.Value)
                {
                    spriteBatch.Draw(Game1.cropSpriteSheet, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle?(coloredSourceRect), crop.tintColor.Value, rotation, new Vector2(8f, 24f), 4f, crop.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (tileLocation.Y + 11f) * 64f / 10000f + tileLocation.X * 1E-05f);
                }

            }
        }
        [HarmonyPatch(typeof(IndoorPot), nameof(IndoorPot.draw))]
        public class IndoorPot_draw_Patch
        {
            public static void Prefix(IndoorPot __instance, int x, int y)
            {
                if (!Config.ModEnabled)
                    return;
                __instance.TileLocation = new Vector2(x, y);
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.placementAction))]
        public class Object_placementAction_Patch
        {
            public static bool Prefix(Object __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || !Config.EnableMoving || __instance is not IndoorPot || (__instance as IndoorPot).hoeDirt.Value?.crop is null)
                    return true;
                location.objects.Add(new Vector2((float)(x / 64), (float)(y / 64)), __instance);
                location.playSound("woodyStep", NetAudio.SoundContext.Default);
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), "loadDisplayName")]
        public class Object_loadDisplayName_Patch
        {
            public static void Postfix(Object __instance, ref string __result)
            {
                if (!Config.ModEnabled || !Config.EnableMoving || __instance is not IndoorPot || (__instance as IndoorPot).hoeDirt.Value?.crop is null)
                    return;
                __result += $" ({new Object((__instance as IndoorPot).hoeDirt.Value.crop.indexOfHarvest.Value, 1).Name})";
            }
        }
        [HarmonyPatch(typeof(IndoorPot), nameof(IndoorPot.performObjectDropInAction))]
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