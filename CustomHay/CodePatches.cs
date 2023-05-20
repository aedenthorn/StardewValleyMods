using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace CustomHay
{
    public partial class ModEntry
    {

        //[HarmonyPatch(typeof(Farm), nameof(Farm.tryToAddHay))]
        public class Farm_tryToAddHay_Patch
        {
            public static void Prefix(Farm __instance, int num)
            {
                if (Config.ModEnabled)
                {
                    SMonitor.Log($"silos: {Utility.numSilos()}");
                    SMonitor.Log($"pieces of hay: {__instance.piecesOfHay.Value}");
                    SMonitor.Log($"pieces to add: {Math.Min(Utility.numSilos() * 240 - __instance.piecesOfHay.Value, num)}");
                }
            }
            public static void Postfix(Farm __instance, int num, int __result)
            {
                if (Config.ModEnabled)
                {
                    SMonitor.Log($"trying to add {num} hay: {__result}");
                    foreach (Building b in (Game1.getLocationFromName("Farm") as Farm).buildings)
                    {
                        if (b.buildingType.Equals("Silo"))
                        {
                            SMonitor.Log($"Found a silo: {b.daysOfConstructionLeft.Value}");
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Grass), nameof(Grass.performToolAction))]
        public class Grass_performToolAction_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Grass.performToolAction");
                var codes = new List<CodeInstruction>(instructions);
                var found1 = false;
                var found2 = false;
                for (int i = 0; i < codes.Count; i++)
                {
                    if (!found1 && i < codes.Count - 2 && codes[i].opcode == OpCodes.Ldc_R8 && codes[i + 2].opcode == OpCodes.Ldc_R8 && (double)codes[i].operand == 0.5 && (double)codes[i + 2].operand == 0.75)
                    {
                        SMonitor.Log("modifying hay chances");
                        codes[i].opcode = OpCodes.Call;
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetOrdinaryHayChance));
                        codes[i + 2].opcode = OpCodes.Call;
                        codes[i + 2].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetGoldHayChance));
                        found1 = true;
                    }
                    if (found1 && !found2 && codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(Random), nameof(Random.NextDouble)))
                    {
                        SMonitor.Log("replacing random value");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetRandomValue))));
                        found2 = true;
                    }
                    if (found1 && found2)
                        break;
                }

                return codes.AsEnumerable();
            }
            public static void PrefixX(Tool t, Vector2 tileLocation)
            {
                if(Config.ModEnabled && t is MeleeWeapon && (t as MeleeWeapon).isScythe())
                {
                    Random random = Game1.IsMultiplayer ? Game1.recentMultiplayerRandom : new Random((int)(Game1.uniqueIDForThisGame + tileLocation.X * 1000f + tileLocation.Y * 11f));
                    SMonitor.Log($"performing scythe action on grass {tileLocation}, seed {(int)(Game1.uniqueIDForThisGame + tileLocation.X * 1000f + tileLocation.Y * 11f)}, chance {random.NextDouble()}, mp: {Game1.IsMultiplayer}, id {Game1.uniqueIDForThisGame}");
                }

            }
        }

        private static double GetOrdinaryHayChance()
        {
            var r = Config.ModEnabled ? (double)Config.OrdinaryHayChance : 0.5;
            //SMonitor.Log($"chance: {r}");
            return r;
        }
        private static double GetGoldHayChance()
        {
            var r = Config.ModEnabled ? (double)Config.GoldHayChance : 0.75;
            //SMonitor.Log($"chance: {r}");
            return r;
        }
        private static double GetRandomValue(double value)
        {
            var r = Config.ModEnabled ? Game1.random.NextDouble() : value;
            //SMonitor.Log($"rolled: {r}");
            return r;
        }
    }
}