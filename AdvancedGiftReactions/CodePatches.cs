using HarmonyLib;
using StardewValley;
using System.Linq;
using Object = StardewValley.Object;

namespace AdvancedGiftReactions
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(NPC), nameof(NPC.getGiftTasteForThisItem))]
        public class NPC_getGiftTasteForThisItem_Patch
        {
            public static void Postfix(NPC __instance, Item item, ref int __result)
            {
                if (!Config.EnableMod)
                    return;
                int num;
                if (Config.IncreaseForBirthday && __instance.isBirthday())
                {
                    IncreaseGiftTaste(ref __result);
                }
                if (!__instance.modData.TryGetValue(giftsKey + item.ItemId, out var numString) || !int.TryParse(numString, out num))
                    num = 0;
                if (num == 0)
                {
                    if(Config.IncreaseForFirst)
                        IncreaseGiftTaste(ref __result);
                }
                else
                {
                    ModifyGiftTaste(ref __result, num);
                }
            }
        }

        [HarmonyPatch(typeof(Farmer), nameof(Farmer.hasGiftTasteBeenRevealed))]
        public class Farmer_hasGiftTasteBeenRevealed_Patch
        {
            public static bool Prefix(Farmer __instance, ref bool __result, string itemId)
            {
                if (!Config.EnableMod || !Config.RevealAllTastes)
                    return true;
                Item item = ItemRegistry.Create(itemId, 1);
                if(item is not Object)
                    return true;
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(NPC), nameof(NPC.receiveGift))]
        public class NPC_receiveGift_Patch
        {
            public static void Postfix(NPC __instance, Object o)
            {
                if (!Config.EnableMod)
                    return;
                if (!__instance.modData.TryGetValue(giftsKey + o.ItemId, out var numString) || !int.TryParse(numString, out var num))
                    num = 0;
                num++;
                __instance.modData[giftsKey + o.ItemId] = num + "";
            }
        }

        [HarmonyPatch(typeof(NPC), nameof(NPC.dayUpdate))]
        public class NPC_dayUpdate_Patch
        {
            public static void Postfix(NPC __instance, int dayOfMonth)
            {
                if (!Config.EnableMod)
                    return;
                switch (Config.ResetPeriod.ToLower())
                {
                    case "none":
                        return;
                    case "week":
                        if ((dayOfMonth - 1) % 7 != 0)
                            return;
                        break;
                    case "season":
                        if (dayOfMonth != 1)
                            return;
                        break;
                    case "year":
                        if (dayOfMonth != 1 || Game1.season != Season.Spring)
                            return;
                        break;
                    default:
                        return;
                }
                foreach(var key in __instance.modData.Keys.ToList().Where(k => k.StartsWith(giftsKey)))
                {
                    __instance.modData.Remove(key);
                }
            }
        }
    }
}