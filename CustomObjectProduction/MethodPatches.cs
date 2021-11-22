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
        private static void Game1__newDayAfterFade_Prefix()
        {
            objectProductionDataDict = SHelper.Content.Load<Dictionary<string, ProductData>>(dictPath, ContentSource.GameContent) ?? new Dictionary<string, ProductData>();
            SMonitor.Log($"Loaded {objectProductionDataDict.Count} products for today");
        }
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
                if (objectProductionDataDict.ContainsKey(__instance.objects[vect].ParentSheetIndex + ""))
                {
                    SMonitor.Log("1");
                    product = objectProductionDataDict[__instance.objects[vect].ParentSheetIndex + ""];
                }
                else if (objectProductionDataDict.ContainsKey(__instance.objects[vect].Name))
                {
                    SMonitor.Log("2");
                    product = objectProductionDataDict[__instance.objects[vect].Name];
                }
                else
                {
                    SMonitor.Log("3");
                    return true;
                }

                __result = true;

                SMonitor.Log($"Trying to take product {product.id} from {__instance.objects[vect].Name} ");

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
            if (objectProductionDataDict.ContainsKey(__instance.ParentSheetIndex + ""))
                product = objectProductionDataDict[__instance.ParentSheetIndex + ""];
            else if (objectProductionDataDict.ContainsKey(__instance.Name))
                product = objectProductionDataDict[__instance.Name];
            else 
                return;

            if (int.TryParse(product.id, out int productID))
            {

                if(productID == -1)
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
            return;
        }
    }
}