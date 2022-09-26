using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace NPCClothing
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(NPC), nameof(NPC.checkAction))]
        public class NPC_checkAction_Patch
        {
            public static bool Prefix(NPC __instance, ref bool __result, Farmer who, GameLocation l)
            {
                if (!Config.ModEnabled || !__instance.isVillager() || who.ActiveObject is not null || who.CurrentItem is null || !clothingDict.Where(k => k.Value.giftName == who.CurrentItem.Name).Any())
                    return true;

                who.ActiveObject = new Object(74, 1) { Name = who.CurrentItem.Name };
                __instance.tryToReceiveActiveObject(who);
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(NPC), nameof(NPC.reloadData))]
        public class NPC_reloadData_Patch
        {
            public static void Prefix(NPC __instance)
            {
                if (!Config.ModEnabled || !__instance.isVillager())
                    return;
                SHelper.GameContent.InvalidateCache($"Characters\\{NPC.getTextureNameForCharacter(__instance.Name)}");
                SHelper.GameContent.InvalidateCache($"Portraits\\{NPC.getTextureNameForCharacter(__instance.Name)}");
            }
        }
        [HarmonyPatch(typeof(NPC), nameof(NPC.receiveGift))]
        public class NPC_receiveGift_Patch
        {
            public static void Postfix(NPC __instance, Object o)
            {
                if (!Config.ModEnabled || !__instance.isVillager())
                    return;
                try
                {
                    var kvp = clothingDict.First(k => k.Value.giftName == o.Name);
                    SMonitor.Log($"Adding {o.Name} to clothing dictionary");
                    if(!__instance.modData.TryGetValue(giftKey, out string data))
                    {
                        __instance.modData[giftKey] = o.Name;
                    }
                    else if (!data.Split(',').Contains(o.Name))
                    {
                        __instance.modData[giftKey] = data + "," + o.Name;
                    }
                    else
                    {
                        SMonitor.Log($"{o.Name} already exists in clothing dictionary");
                        return;
                    }
                    SHelper.GameContent.InvalidateCache($"Characters\\{NPC.getTextureNameForCharacter(__instance.Name)}");
                    SHelper.GameContent.InvalidateCache($"Portraits\\{NPC.getTextureNameForCharacter(__instance.Name)}");
                    __instance.reloadSprite();
                }
                catch
                {
                    SMonitor.Log($"{o.Name} has no associated clothing");
                }
            }
        }
    }
}