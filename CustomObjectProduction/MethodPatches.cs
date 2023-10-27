using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using xTile.Dimensions;

namespace CustomObjectProduction
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        private static bool GameLocation_checkAction_Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
        {
            if (!Config.EnableMod)
                return true;

            Vector2 vect = new Vector2(tileLocation.X, tileLocation.Y);
            if (__instance.objects.ContainsKey(vect))
            {
                if (__instance.objects[vect].heldObject.Value == null || !__instance.objects[vect].readyForHarvest.Value)
                    return true;

                SMonitor.Log($"checking action on {__instance.objects[vect].Name} {__instance.objects[vect].heldObject.Value},{__instance.objects[vect].readyForHarvest.Value},{__instance.objects[vect].ParentSheetIndex},{__instance.objects[vect].Name}");

                ProductData product = null;

                var objectProductionDataDict = SHelper.GameContent.Load<Dictionary<string, ProductData>>(dictPath) ?? new Dictionary<string, ProductData>();
                if (objectProductionDataDict.ContainsKey(__instance.objects[vect].ParentSheetIndex + ""))
                {
                    product = objectProductionDataDict[__instance.objects[vect].ParentSheetIndex + ""];
                }
                else if (objectProductionDataDict.ContainsKey(__instance.objects[vect].Name))
                {
                    product = objectProductionDataDict[__instance.objects[vect].Name];
                }
                else
                {
                    return true;
                }

                __result = true;



                SMonitor.Log($"Trying to take product from {__instance.objects[vect].Name} ");

                Object objectThatWasHeld = __instance.objects[vect].heldObject.Value;
                __instance.objects[vect].heldObject.Value = null;
                if (!who.addItemToInventoryBool(objectThatWasHeld, false))
                {
                    __instance.objects[vect].heldObject.Value = objectThatWasHeld;
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
                    return false;
                }
                Game1.playSound("coin");
                __instance.objects[vect].readyForHarvest.Value = false;
                __instance.objects[vect].showNextIndex.Value = false;
                return false;
            }
            return true;
        }

        private static void Object_DayUpdate_Postfix(Object __instance)
        {
            if (!Config.EnableMod)
                return;

            ProductData product = null;
            var objectProductionDataDict = SHelper.GameContent.Load<Dictionary<string, ProductData>>(dictPath) ?? new Dictionary<string, ProductData>();
            if (objectProductionDataDict.ContainsKey(__instance.ParentSheetIndex + ""))
                product = objectProductionDataDict[__instance.ParentSheetIndex + ""];
            else if (objectProductionDataDict.ContainsKey(__instance.Name))
                product = objectProductionDataDict[__instance.Name];
            else 
                return;

            if(product.infoList.Count > 0)
            {
                float totalWeight = 0;
                foreach (var r in product.infoList)
                {
                    totalWeight += r.weight;
                }
                int currentWeight = 0;
                double chance = Game1.random.NextDouble();
                foreach (var r in product.infoList)
                {
                    currentWeight += r.weight;
                    if (chance < currentWeight / totalWeight)
                    {
                        int amount = Game1.random.Next(r.min, r.max + 1);
                        int quality = Game1.random.Next(r.minQuality, r.maxQuality+ 1);
                        Object obj = GetObjectFromID(r.id, amount, quality);
                        if(obj == null)
                        {
                            __instance.MinutesUntilReady = 0;
                            __instance.readyForHarvest.Value = false;
                            __instance.heldObject.Value = null;
                            return;
                        }
                        __instance.MinutesUntilReady = 0;
                        __instance.readyForHarvest.Value = true;
                        __instance.heldObject.Value = obj;
                        return;
                    }
                }
            }
            else if(int.TryParse(product.id, out int productID))
            {
                if (productID == -1)
                {
                    __instance.MinutesUntilReady = 0;
                    __instance.readyForHarvest.Value = false;
                    __instance.heldObject.Value = null;
                    return;
                }

                __instance.MinutesUntilReady = 0;
                __instance.readyForHarvest.Value = true;
                __instance.heldObject.Value = new Object(productID, product.amount, false, -1, product.quality);
            }
        }
    }
}