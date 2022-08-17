using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace MusicalPaths
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.UpdateWhenCurrentLocation))]
        public class GameLocation_UpdateWhenCurrentLocation_Patch
        {

            public static void Postfix(GameLocation __instance)
            {
                if (!Config.ModEnabled || Game1.dialogueUp || Game1.soundBank is null)
                    return;

                checking = false;

                foreach (Farmer farmer in __instance.farmers)
                {
                    Vector2 playerPos = farmer.getTileLocation();
                    if (__instance.terrainFeatures.TryGetValue(playerPos, out TerrainFeature f) && f is Flooring && f.modData.TryGetValue(typeKey, out string soundType))
                    {
                        int.TryParse(f.modData[lastTimeKey], out int lastTime);
                        if (soundType.Equals("Flute Block") && (Game1.currentGameTime.TotalGameTime.TotalMilliseconds - lastTime >= 1000 || lastTime > Game1.currentGameTime.TotalGameTime.TotalMilliseconds))
                        { 
                            var sound = Game1.soundBank.GetCue("flute");
                            sound.SetVariable("Pitch", int.Parse(f.modData[whichKey]));
                            sound.Play();
                            f.modData[lastTimeKey] = "" + (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
                        }
                        else if (soundType.Equals("Drum Block") && (Game1.currentGameTime.TotalGameTime.TotalMilliseconds - lastTime >= 1000 || lastTime > Game1.currentGameTime.TotalGameTime.TotalMilliseconds))
                        {
                            var sound = Game1.soundBank.GetCue("drumkit" + int.Parse(f.modData[whichKey]));
                            sound.Play();
                            f.modData[lastTimeKey] = "" + (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static void Postfix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || who.ActiveObject is not null || __result || checking)
                    return;
                checking = true;
                var x = Environment.StackTrace;
                if (__instance.terrainFeatures.TryGetValue(new Vector2(tileLocation.X, tileLocation.Y), out TerrainFeature f) && f is Flooring && f.modData.TryGetValue(typeKey, out string soundType))
                {
                    if (soundType.Equals("Flute Block"))
                    {
                        var which = (int.Parse(f.modData[whichKey]) + 100) % 2400;
                        f.modData[whichKey] = "" + which;
                        var sound = Game1.soundBank.GetCue("flute");
                        sound.SetVariable("Pitch", which);
                        sound.Play();
                    }
                    else if (soundType.Equals("Drum Block") && Game1.currentGameTime.TotalGameTime.TotalMilliseconds - int.Parse(f.modData[lastTimeKey]) >= 1000)
                    {
                        var which = (int.Parse(f.modData[whichKey]) + 1) % 7;
                        f.modData[whichKey] = "" + which;
                        var sound = Game1.soundBank.GetCue("drumkit" + which);
                        sound.Play();
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.placementAction))]
        public class Object_placementAction_Patch
        {
            public static bool Prefix(Object __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || !SHelper.Input.IsDown(Config.ModKey))
                    return true;
                Vector2 placementTile = new Vector2((float)(x / 64), (float)(y / 64));
                if (location.terrainFeatures.TryGetValue(placementTile, out TerrainFeature f) && f is Flooring)
                {
                    string soundName;
                    if (__instance.Name.Equals("Flute Block"))
                    {
                        soundName = "flute";
                    }
                    else if (__instance.Name.Equals("Drum Block"))
                    {
                        soundName = "drumkit0";
                    }
                    else return true;
                    f.modData[typeKey] = __instance.Name;
                    f.modData[whichKey] = "0";
                    f.modData[lastTimeKey] = "" + (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
                    Game1.soundBank.GetCue(soundName).Play();
                    if (Config.ConsumeBlock)
                        Game1.player.reduceActiveItemByOne();
                    __result = true;
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Flooring), nameof(Flooring.draw))]
        public class Flooring_draw_Patch
        {
            public static void Postfix(Flooring __instance, SpriteBatch spriteBatch, Vector2 tileLocation)
            {
                if (!Config.ModEnabled || !Config.ShowBlockOutLine || !__instance.modData.TryGetValue(typeKey, out string blockType))
                    return;
                spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, tileLocation * 64f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, blockType == "Flute Block" ? 464 : 463, 16, 16), Color.White * Config.BlockOutLineOpacity, 0, Vector2.Zero, 4f, SpriteEffects.None, 1E-09f + 0.000001f);
            }
        }
    }
}