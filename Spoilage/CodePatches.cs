using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using System.Globalization;
using Object = StardewValley.Object;

namespace Spoilage
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Object), nameof(Object.drawInMenu))]
        public class Object_drawInMenu_Patch
        {
            public static void Prefix(Object __instance, ref Color color, ref int __state)
            {
                if (!Config.ModEnabled || !__instance.modData.ContainsKey(spoiledKey))
                    return;
                color = Config.CustomSpoiledColor;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.sellToStorePrice))]
        public class Object_sellToStorePrice_Patch
        {
            public static bool Prefix(Object __instance, ref int __result)
            {
                if (!Config.ModEnabled || !__instance.modData.ContainsKey(spoiledKey))
                    return true;
                __result = 0;
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.DisplayName))]
        [HarmonyPatch(MethodType.Getter)]
        public class Object_DisplayName_Patch
        {
            public static void Postfix(Object __instance, ref string __result)
            {
                if (!Config.ModEnabled || !__instance.modData.ContainsKey(spoiledKey))
                    return;
                __result = __instance.modData[spoiledKey] + SHelper.Translation.Get("spoiled");
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.getDescription))]
        public class Object_getDescription_Patch
        {
            public static void Postfix(Object __instance, ref string __result)
            {
                if (!Config.ModEnabled || !__instance.modData.TryGetValue(ageKey, out string ageString))
                    return;
                if (__instance.modData.ContainsKey(spoiledKey))
                    __result = SHelper.Translation.Get("spoiled-desc");
                else if (Config.DisplayDays)
                {
                    int days = (int)float.Parse(ageString, NumberStyles.Any, CultureInfo.InvariantCulture);
                    if (Config.DisplayDaysLeft)
                    {
                        int left = GetSpoilAge(__instance);
                        __result += string.Format(SHelper.Translation.Get("x-y-days-old"), days, left);
                    }
                    else if (days > 1)
                        __result += string.Format(SHelper.Translation.Get("x-days-old"), days);
                    else if(days == 1)
                        __result += string.Format(SHelper.Translation.Get("1-day-old"), days);
                }
            }
        }
    }
}