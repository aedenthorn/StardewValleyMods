using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
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
                if (!Config.ModEnabled || !__instance.isVillager() || who.CurrentItem is null)
                    return true;
                IEnumerable<ClothingData> list = null;
                try
                {
                    list = clothingDict.Values.Where(k => k.giftName == who.CurrentItem.Name);
                }
                catch
                {
                    return true;
                }
                if (list is null)
                    return true;
                foreach(var data in list)
                {
                    if (ClothesFit(__instance, data, true) || ClothesFit(__instance, data, false))
                    {
                        if (__instance.modData.TryGetValue(giftKey, out string md))
                        {
                            var split = md.Split(',').ToList();
                            if (split.Contains(who.CurrentItem.Name))
                            {
                                SMonitor.Log($"{who.CurrentItem.Name} already exists in clothing dictionary for {__instance.Name}, removing");
                                split.Remove(who.CurrentItem.Name);
                                __instance.modData[giftKey] = string.Join(",", split);
                                SHelper.GameContent.InvalidateCache($"Characters\\{NPC.getTextureNameForCharacter(__instance.Name)}");
                                SHelper.GameContent.InvalidateCache($"Portraits\\{NPC.getTextureNameForCharacter(__instance.Name)}");
                                __result = true;
                                return false;
                            }
                        }
                        if (Config.ForceWearOnGift)
                            forceWear = data;
                        if (who.ActiveObject is null)
                        {
                            int index = giftIndexes[0];
                            switch (data.giftReaction)
                            {
                                case "like":
                                    index = giftIndexes[1];
                                    break;
                                case "dislike":
                                    index = giftIndexes[2];
                                    break;
                                case "hate":
                                    index = giftIndexes[3];
                                    break;
                                case "neutral":
                                    index = giftIndexes[4];
                                    break;
                            }

                            who.ActiveObject = new Object(index, 1) { Name = who.CurrentItem.Name };
                        }
                        __instance.tryToReceiveActiveObject(who);
                        __result = true;
                        return false;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(NPC), nameof(NPC.getGiftTasteForThisItem))]
        public class NPC_getGiftTasteForThisItem_Patch
        {
            public static bool Prefix(NPC __instance, Item item, ref int __result)
            {
                if (!Config.ModEnabled || !__instance.isVillager() || !giftIndexes.Contains(item.ParentSheetIndex))
                    return true;
                __result = Math.Abs(item.ParentSheetIndex) - 42424200;
                SMonitor.Log($"Returning gift taste of {__result} for {item.Name}");
                return false;
            }
        }
        [HarmonyPatch(typeof(NPC), nameof(NPC.dayUpdate))]
        public class NPC_dayUpdate_Patch
        {
            public static void Prefix(NPC __instance)
            {
                if (!Config.ModEnabled || !__instance.isVillager())
                    return;
                SHelper.GameContent.InvalidateCache($"Characters\\{NPC.getTextureNameForCharacter(__instance.Name)}");
                SHelper.GameContent.InvalidateCache($"Portraits\\{NPC.getTextureNameForCharacter(__instance.Name)}");
            }
        }
        [HarmonyPatch(typeof(NPC), new Type[] { typeof(AnimatedSprite), typeof(Vector2), typeof(string), typeof(int), typeof(string), typeof(bool), typeof(Dictionary<int, int[]>), typeof(Texture2D) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class NPC_Patch
        {
            public static void Postfix(NPC __instance)
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
                        SMonitor.Log($"{o.Name} already exists in clothing dictionary, removing");
                        var split = data.Split(',').ToList();
                        split.Remove(o.Name);
                        __instance.modData[giftKey] = string.Join(",", split);
                        forceWear = null;
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