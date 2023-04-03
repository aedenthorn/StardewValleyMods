using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using StardewValley.Monsters;
using xTile.Dimensions;
using StardewValley.Objects;

namespace NapalmMummies
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Mummy), nameof(Mummy.takeDamage))]
        public class Mummy_takeDamage_Patch
        {
            public static void Postfix(Mummy __instance, Farmer who)
            {
                if (!Config.ModEnabled || __instance.reviveTimer.Value != 10000)
                    return;
                List<Ring> rings = new List<Ring>();
                if(who.leftRing.Value is CombinedRing)
                {
                    rings.AddRange((who.leftRing.Value as CombinedRing).combinedRings);
                }
                else
                {
                    rings.Add(who.leftRing.Value);
                }
                if(who.rightRing.Value is CombinedRing)
                {
                    rings.AddRange((who.rightRing.Value as CombinedRing).combinedRings);
                }
                else
                {
                    rings.Add(who.rightRing.Value);
                }
                if(rings.Exists(r => r is not null && r.indexInTileSheet.Value == 811))
                    __instance.currentLocation.explode(__instance.getTileLocation(), 2, who, false, -1);
            }
        }
        [HarmonyPatch(typeof(Ring), nameof(Ring.onMonsterSlay))]
        public class Ring_onMonsterSlay_Patch
        {
            public static bool Prefix(Ring __instance, Monster m)
            {
                if (!Config.ModEnabled || m is not Mummy || __instance.indexInTileSheet.Value != 811)
                    return true;
                return false;
            }
        }
    }
}