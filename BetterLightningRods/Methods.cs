using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace BetterLightningRods
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        private static int GetLightningRod(List<Vector2> rods, int index)
        {
            int rod = Math.Min(index, rods.Count - 1);
            return rod;
        }
        private static double GetLightningChance(double chance)
        {
            return Config.EnableMod ? (double)(Config.LightningChance / 100f) : chance;
        }
        private static int GetRodsToCheck()
        {
            return Config.EnableMod ? Config.RodsToCheck : 2;
        }
        private static List<Vector2> ShuffleRodList(List<Vector2> rods)
        {
            //SMonitor.Log($"Shuffling {rods.Count} rods");
            ShuffleList(rods);
            if (Config.OnlyCheckEmpty)
            {
                for (int i = rods.Count - 1; i >= 0; i--)
                {
                    if (Game1.getFarm().objects[rods[i]].heldObject.Value != null)
                        rods.RemoveAt(i);
                }
            }
            return rods;
        }
        public static List<T> ShuffleList<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Game1.random.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
    }
}