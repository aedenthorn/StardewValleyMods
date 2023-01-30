using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using xTile.Dimensions;
using Color = Microsoft.Xna.Framework.Color;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ImmersiveSprinklers
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Object), nameof(Object.placementAction))]
        public class Object_placementAction_Patch
        {
            public static bool Prefix(Object __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
            {
                if (!Config.EnableMod || !__instance.IsSprinkler())
                    return true;
                Vector2 placementTile = new Vector2((float)(x / 64), (float)(y / 64));
                if (!location.terrainFeatures.TryGetValue(placementTile, out var tf) || tf is not HoeDirt)
                    return true;
                int which = GetMouseCorner();
                ReturnSprinkler(who, location, tf, placementTile, which);
                tf.modData[sprinklerKey + which] = GetSprinklerString(__instance);
                location.playSound("woodyStep", NetAudio.SoundContext.Default);
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.DrawOptimized))]
        public class HoeDirt_DrawOptimized_Patch
        {
            public static void Postfix(HoeDirt __instance, SpriteBatch dirt_batch, Vector2 tileLocation)
            {
                if (!Config.EnableMod)
                    return;
                for (int i = 0; i < 4; i++)
                {
                    if(__instance.modData.TryGetValue(sprinklerKey + i, out var sprinklerString))
                    {
                        if(!sprinklerDict.TryGetValue(sprinklerString, out var obj))
                        {
                            obj = GetSprinkler(sprinklerString);
                        }
                        if(obj is not null)
                        {
                            var globalPosition = tileLocation * 64 + new Vector2(32 - 8 * Config.Scale + Config.DrawOffsetX, 32 - 8 * Config.Scale + Config.DrawOffsetY) + GetSprinklerCorner(i) * 32;
                            var position = Game1.GlobalToLocal(globalPosition);
                            dirt_batch.Draw(Game1.objectSpriteSheet, position, GameLocation.getSourceRectForObject(obj.ParentSheetIndex), Color.White * Config.Alpha, 0, Vector2.Zero, Config.Scale, obj.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (globalPosition.Y + 16 + Config.DrawOffsetZ) / 10000f);
                        }
                    }
                }
            }

        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isTileOccupiedForPlacement))]
        public class GameLocation_isTileOccupiedForPlacement_Patch
        {
            public static void Postfix(GameLocation __instance, Vector2 tileLocation, Object toPlace, ref bool __result)
            {
                if (!Config.EnableMod || !__result || toPlace is null || !toPlace.IsSprinkler())
                    return;
                if (__instance.terrainFeatures.ContainsKey(tileLocation) && __instance.terrainFeatures[tileLocation] is HoeDirt && ((HoeDirt)__instance.terrainFeatures[tileLocation]).crop is not null)
                {
                    __result = false;
                }
            }

        }
        [HarmonyPatch(typeof(GameLocation), "initNetFields")]
        public class GameLocation_initNetFields_Patch
        {
            public static void Postfix(GameLocation __instance)
            {
                if (!Config.EnableMod)
                    return;
                __instance.terrainFeatures.OnValueRemoved += delegate (Vector2 tileLocation, TerrainFeature tf)
                {
                    if (tf is not HoeDirt)
                        return;
                    for (int i = 0; i < 4; i++)
                    {
                        if (tf.modData.TryGetValue(sprinklerKey + i, out var sprinklerString))
                        {
                            var obj = GetSprinkler(sprinklerString);
                            if (obj is not null)
                            {
                                __instance.debris.Add(new Debris(obj, tileLocation * 64));
                            }
                        }
                    }
                };
            }
        }
        [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.dayUpdate))]
        public class HoeDirt_dayUpdate_Patch
        {
            public static void Postfix(HoeDirt __instance, GameLocation environment, Vector2 tileLocation)
            {
                if (!Config.EnableMod || (environment.IsOutdoors && Game1.IsRainingHere(environment)))
                    return;
                for (int i = 0; i < 4; i++)
                {
                    if (__instance.modData.TryGetValue(sprinklerKey + i, out var sprinklerString))
                    {
                        var obj = GetSprinkler(sprinklerString);
                        if (obj is not null)
                        {
                            var which = i;
                            environment.postFarmEventOvernightActions.Add(delegate
                            {
                                ActivateSprinkler(environment, tileLocation, obj, which, true);
                            });
                        }
                    }
                }
            }

        }
    }
}