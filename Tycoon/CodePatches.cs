using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System.Collections.Generic;

namespace Tycoon
{
	public partial class ModEntry
    {
        [HarmonyPatch(typeof(ShopMenu), "tryToPurchaseItem")]
        public class ShopMenu_tryToPurchaseItem_Patch
        {
            public static void Postfix(ISalable item, bool __result)
            {
                if (!Config.ModEnabled || !__result || !item.Name.StartsWith("aedenthorn.Tycoon_"))
                    return;

                foreach(var kvp in dataDict)
                {
                    if(item.Name == $"aedenthorn.Tycoon_{kvp.Key}")
                    {
                        if (ownedProperties is null)
                            ownedProperties = new();
                        ownedProperties[kvp.Key] = true;
                        SHelper.GameContent.InvalidateCache("Data/Shops");
                        SHelper.GameContent.InvalidateCache("Data/Minecarts");
                        return;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Furniture), "loadDescription")]
        public class Furniture_loadDescriptionWidth_Patch
        {
            public static void Postfix(Furniture __instance, ref string __result)
            {
                if (!Config.ModEnabled || !__instance.ItemId.StartsWith(SHelper.ModRegistry.ModID) || !dataDict.TryGetValue(__instance.ItemId.Substring(SHelper.ModRegistry.ModID.Length + 1), out var data))
                    return;
                __result = data.Description ?? string.Format(SHelper.Translation.Get("deed-description-x"), data.Name);
            }
        }
    }
}
