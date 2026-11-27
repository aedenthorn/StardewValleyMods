using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace PetBed
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Pet), nameof(Pet.warpToFarmHouse))]
        public static class Pet_warpToFarmHouse_Patch
        {
            public static bool Prefix(Pet __instance, Farmer who)
            {
                SMonitor.Log("Warping pet to farmhouse");
                return !Config.ModEnabled || Game1.random.NextDouble() > Config.BedChance / 100f || !WarpPetToBed(__instance, Utility.getHomeOfFarmer(who), false);
            }
        }
        
        [HarmonyPatch(typeof(Pet), nameof(Pet.CurrentBehavior))]
        [HarmonyPatch(MethodType.Setter)]
        public static class Pet_CurrentBehavior_Patch
        {
            public static void Prefix(Pet __instance, string value)
            {
                if(!Config.ModEnabled || !Context.IsWorldReady || __instance.CurrentBehavior == value || __instance.CurrentBehavior != "Sleep" || !__instance.modData.TryGetValue(sleepingKey, out var tile))
                    return;

                SMonitor.Log("Waking up");
                foreach (var f in __instance.currentLocation.furniture)
                {
                    if (f.TileLocation.ToString() == tile)
                    {
                        __instance.isSleepingOnFarmerBed.Value = false;
                        if (SHelper.GameContent.Load<Dictionary<string, PetBedData>>(dictPath).TryGetValue(f.ItemId, out var data))
                        {
                            __instance.Position = new Vector2((f.TileLocation.X + data.WakeX) * Game1.tileSize, (f.TileLocation.Y + data.WakeY) * Game1.tileSize);
                        }
                        return;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Pet), nameof(Pet.setAtFarmPosition))]
        public static class Pet_setAtFarmPosition_Patch
        {
            public static bool Prefix(Pet __instance)
            {
                SMonitor.Log("Setting pet to farm position");
                return !Config.ModEnabled || Game1.random.NextDouble() > Config.BedChance / 100f || !Game1.IsMasterGame || Game1.isRaining || !WarpPetToBed(__instance, Game1.getFarm(), true);
            }
        }
        [HarmonyPatch(typeof(Pet), nameof(Pet.dayUpdate))]
        public static class Pet_dayUpdate_Patch
        {
            public static void Prefix(Pet __instance, ref bool __state)
            {
                if (!Config.ModEnabled)
                    return;
                if(__instance.currentLocation is Farm && !Game1.isRaining && Game1.random.NextDouble() < Config.BedChance / 100f && Game1.IsMasterGame)
                {
                    __state = true;
                }
                if(__instance.currentLocation is FarmHouse && Game1.isRaining && Game1.random.NextDouble() < Config.BedChance / 100f && Game1.IsMasterGame)
                {
                    __state = true;
                }
            }
            public static void Postfix(Pet __instance, bool __state)
            {
                if (__state)
                {
                    SMonitor.Log("Setting pet to bed position");
                    WarpPetToBed(__instance, __instance.currentLocation, true);
                }
            }
        }
        [HarmonyPatch(typeof(Furniture), nameof(Furniture.HasSittingFarmers))]
        public static class Furniture_HasSittingFarmers_Patch
        {
            public static bool Prefix(Furniture __instance, ref bool __result)
            {
                if (Config.ModEnabled && __instance is not BedFurniture && Furniture.isDrawingLocationFurniture)
                {
                    foreach(var c in Game1.currentLocation.characters)
                    {
                        if(c is Pet && __instance.boundingBox.Value.Intersects(c.GetBoundingBox()))
                        {
                            (c as Pet).isSleepingOnFarmerBed.Value = false;
                            __result = true;
                            return false;
                        }
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Microsoft.Xna.Framework.Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
        public static class GameLocation_isCollidingPosition_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling GameLocation.isCollidingPosition");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(Furniture), nameof(Furniture.IntersectsForCollision)))
                    {
                        SMonitor.Log("adding check for pet on furniture");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.CheckForPetOnBed))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_S, 6));
                        codes.Insert(i + 1, codes[i - 3].Clone());
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(Pet), nameof(Pet.draw), new Type[] { typeof(SpriteBatch) })]
        public static class Pet_draw_Patch
        {
            public static void Prefix(Pet __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || !__instance.modData.TryGetValue(sleepingKey, out var tile))
                    return;
                if(__instance.CurrentBehavior != "Sleep")
                {
                    __instance.modData.Remove(sleepingKey);
                    return;
                }
                foreach(var f in __instance.currentLocation.furniture)
                {
                    if (f.TileLocation.ToString() == tile)
                    {
                        __instance.isSleepingOnFarmerBed.Value = true;
                        if (SHelper.GameContent.Load<Dictionary<string, PetBedData>>(dictPath).TryGetValue(f.ItemId, out var data) && data.FrontTexture != null)
                        {
                            var texture = SHelper.GameContent.Load<Texture2D>(data.FrontTexture);
                            Vector2 actualDrawPosition = Game1.GlobalToLocal(Game1.viewport, AccessTools.FieldRefAccess<Furniture, NetVector2>(f, "drawPosition").Value + ((f.shakeTimer > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero));
                            SpriteEffects spriteEffects = (f.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None);

                            b.Draw(texture, actualDrawPosition, null, Color.White, 0f, Vector2.Zero, 4f, spriteEffects, (__instance.StandingPixel.Y + 113) / 10000f);
                        }
                        return;
                    }
                }
                __instance.modData.Remove(sleepingKey);
            }
        }
        private static bool CheckForPetOnBed(bool result, Furniture f, Character character)
        {

            if (!Config.ModEnabled || character is not Pet || !f.boundingBox.Value.Intersects(character.GetBoundingBox()))
            {
                return result;
            }
            return false;
        }
    }
}