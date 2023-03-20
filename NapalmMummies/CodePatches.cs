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
                if (!Config.ModEnabled || __instance.reviveTimer.Value != 10000 || (who.leftRing.Value.indexInTileSheet.Value != 811 && who.rightRing.Value.indexInTileSheet.Value != 811))
                    return;
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