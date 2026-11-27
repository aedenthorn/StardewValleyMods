using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FishingChestTweaks
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(BobberBar), new Type[] { typeof(string), typeof(float), typeof(bool), typeof(List<string>), typeof(string), typeof(bool), typeof(string), typeof(bool) } )]
        [HarmonyPatch(MethodType.Constructor)]
        public static class BobberBar_Patch
        {
            public static void Prefix(BobberBar __instance, ref bool treasure, ref bool goldenTreasure)
            {
                if (Config.Debug)
                {
                    treasure = true;
                }
                ChestPosition.Value = -1;
                ChestTargetPosition.Value = -1;
                CatchLevel.Value = -1;
                ChestAcceleration.Value = 0;
                FSAcceleration.Value = 0;
                ChestSpeed.Value = 0;
            }   
        }
        [HarmonyPatch(typeof(BobberBar), nameof(BobberBar.update))]
        public static class BobberBar_update_Patch
        {
            public static bool Prefix(BobberBar __instance)
            {
                if (!Config.ModEnabled || !Config.ChestWithoutFish || !__instance.treasureCaught || !__instance.fadeOut || Game1.player.CurrentTool is not FishingRod rod || __instance.distanceFromCatching > 0.9f)
                    return true;
                rod.treasureCaught = true;
                rod.whichFish = ItemRegistry.GetMetadata(__instance.whichFish);
                rod.openTreasureMenuEndFunction(0);
                return false;
            }
            public static void Postfix(BobberBar __instance)
            {
                if (!Config.ModEnabled || !__instance.treasure || __instance.treasureCaught || __instance.treasureAppearTimer > 0)
                    return;
                if (Config.NoReduction)
                {
                    __instance.treasureCatchLevel = Math.Max(__instance.treasureCatchLevel, CatchLevel.Value);
                    CatchLevel.Value = __instance.treasureCatchLevel;
                }
                    
                if ((!__instance.goldenTreasure || Config.GoldenChestMovement.ToLower() != "none") && (__instance.goldenTreasure || Config.ChestMovement.ToLower() != "none"))
                {
                    var moveType = __instance.goldenTreasure ? Config.GoldenChestMovement : Config.ChestMovement;

                    switch (moveType.ToLower())
                    {
                        case "follow":
                            __instance.treasurePosition = __instance.bobberPosition;
                            return;
                        case "random":
                            if (ChestPosition.Value < 0)
                                ChestPosition.Value = __instance.treasurePosition;
                            SetChestPosition(__instance, false);
                            __instance.treasurePosition = ChestPosition.Value;
                            return;
                        case "legend":
                            if (ChestTargetPosition.Value < 0)
                                ChestTargetPosition.Value = __instance.treasurePosition;
                            SetChestPosition(__instance, true);
                            __instance.treasurePosition = ChestPosition.Value;
                            return;
                        default:
                            return;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(ItemGrabMenu), new Type[] { typeof(IList<Item>), typeof(object) })]
        [HarmonyPatch(MethodType.Constructor)]
        public static class ItemGrabMenu_Patch
        {
            public static bool Prefix(ref IList<Item> inventory, object context)
            {
                if (!Config.ModEnabled || !Config.AutoCollect || context is not FishingRod)
                    return true;
                List<Item> items = new List<Item>();
                foreach (var item in inventory)
                {
                    var ostack = item.Stack;
                    if (Config.DropIfFull)
                    {
                        TryReturnObject(item, Game1.player);
                    }
                    else
                    {
                        if (!Game1.player.addItemToInventoryBool(item) || (item.Stack < ostack && item.Stack > 0))
                        {
                            items.Add(item);
                        }
                    }
                }
                if (items.Any())
                {
                    inventory = items;
                    return true;

                }
                Game1.activeClickableMenu = null;
                return false;
            }
        }
    }
}