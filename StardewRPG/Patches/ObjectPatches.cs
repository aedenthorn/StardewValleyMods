using StardewValley;
using StardewValley.Tools;
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
            var sub = GetStatMod(GetStatValue(Game1.player, "wis", Config.BaseStatValue)) * Config.WisCraftTimeBonus;
            SMonitor.Log($"Modifying craft time {__instance.MinutesUntilReady } - {sub}");
            __instance.MinutesUntilReady = (int)Math.Round(__instance.MinutesUntilReady * (1 - sub));
        }
        private static void Object_salePrice_Postfix(Object __instance, ref int __result)
        {
            if (!Config.EnableMod)
                return;
            var mult = GetStatMod(GetStatValue(Game1.player, "cha", Config.BaseStatValue)) * Config.ChaPriceBonus;
            //SMonitor.Log($"Modifying buy price of {__instance.Name}: {__result} by {-mult}x");
            __result = (int)Math.Round(__result * Math.Max(0, 1 - mult));
        }
        private static void Object_sellToStorePrice_Postfix(Object __instance, ref int __result)
        {
            if (!Config.EnableMod)
                return;
            var mult = GetStatMod(GetStatValue(Game1.player, "cha", Config.BaseStatValue)) * Config.ChaPriceBonus;
            //SMonitor.Log($"Modifying sell price of of {__instance.Name}: {__result} by {-mult}x");
            __result = (int)Math.Min(__instance.salePrice(), Math.Round(__result * (1 + mult)));
        }
        private static void Object_draw_Prefix(Object __instance, float alpha, int x, int y)
        {
            if (!Config.EnableMod || __instance.ParentSheetIndex != 590)
                return;

            int wis = GetStatValue(Game1.player, "wis", Config.BaseStatValue);
            if (wis == 10)
                return;
            float val = (wis - 10) * Config.WisSpotVisibility;
            double tick = Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1200;
            if (val < 0)
                alpha *= 1 + val;
            else if (tick < 300)
                __instance.scale.Y = 4;
            else if (tick < 600)
                __instance.scale.Y = 4 + val;
            else if (tick < 900)
                __instance.scale.Y = 4 + val * 2;
            else
                __instance.scale.Y = 4 + val;
        }
    }
}