using StardewValley;
using System;
using Object = StardewValley.Object;

namespace StardewRPG
{
    public partial class ModEntry
    {
        private static void Object_performObjectDropInAction_Prefix(Object __instance, ref int __state)
        {
            if (!Config.EnableMod || !__instance.bigCraftable.Value)
                return;
            __state = __instance.MinutesUntilReady;
        }
        private static void Object_performObjectDropInAction_Postfix(Object __instance, int __state)
        {
            if (!Config.EnableMod || !__instance.bigCraftable.Value || __state >= __instance.MinutesUntilReady)
                return;
            __instance.MinutesUntilReady = (int)Math.Round(__instance.MinutesUntilReady * (1 - GetStatMod(GetStatValue(Game1.player, "wis", Config.BaseStatValue)) * Config.WisCraftTimeBonus));
        }
    }
}