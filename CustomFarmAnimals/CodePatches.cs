using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace CustomFarmAnimals
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(PurchaseAnimalsMenu), new Type[] { typeof(List<Object>) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class PurchaseAnimalsMenu_Patch
        {
            public static void Postfix(PurchaseAnimalsMenu __instance, List<Object> stock)
            {
                if (!Config.EnableMod)
                    return;
                var dict = Game1.content.Load<Dictionary<string, string>>("Data\\FarmAnimals");
                List<Object> newStock = new List<Object>();
                foreach(string key in dict.Keys)
                {
                    if(!stock.Exists(a => GetGenericName(a.Name) == key) && dataDict.ContainsKey(key))
                    {
                        string[] split = dict[key].Split('/');

                        newStock.Add(new Object(100, 1, false, Convert.ToInt32(split[24]), 0)
                        {
                            Name = key,
                            Type = GetTypeConditionalOnBuilding(split[15]),
                            displayName = split[25]
                        });
                    }
                }
                for (int j = 0; j < newStock.Count; j++)
                {
                    var data = dataDict[newStock[j].Name];
                    int i = stock.Count + j;
                    __instance.animalsToPurchase.Add(new ClickableTextureComponent(newStock[j].salePrice().ToString() ?? "", new Rectangle(__instance.xPositionOnScreen + IClickableMenu.borderWidth + i % 3 * 64 * 2, __instance.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth / 2 + i / 3 * 85, 128, 64), null, newStock[j].Name, iconDict[data.iconPath], new Rectangle(0, 0, 32, 16), 4f )
                    {
                        item = newStock[j],
                        myID = i,
                        rightNeighborID = ((i % 3 == 2) ? -1 : (i + 1)),
                        leftNeighborID = ((i % 3 == 0) ? -1 : (i - 1)),
                        downNeighborID = i + 3,
                        upNeighborID = i - 3
                    });
                }

            }

        }
        [HarmonyPatch(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.getAnimalTitle))]
        public class PurchaseAnimalsMenu_getAnimalTitle_Patch
        {
            public static void Postfix(string name, ref string __result)
            {
                if (!Config.EnableMod || __result.Length > 0)
                    return;
                string outName = GetName(name);
                if (outName != null)
                    __result = outName;
            }
        }

        [HarmonyPatch(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.getAnimalDescription))]
        public class PurchaseAnimalsMenu_getAnimalDescription_Patch
        {
            public static bool Prefix(string name, ref string __result)
            {
                if (!Config.EnableMod)
                    return true;
                if (name.EndsWith(" Chicken"))
                    __result = Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11334") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11335");
                else if (name.EndsWith(" Cow"))
                    __result = Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11343") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
                else
                    return true;
                return false;
            }
        }
        [HarmonyPatch(typeof(FarmAnimal), new Type[] { typeof(string), typeof(long), typeof(long) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class FarmAnimal_Patch
        {
            public static void Prefix(FarmAnimal __instance, string type, ref string __state)
            {
                if (!Config.EnableMod)
                    return;
                __state = type;
            }
            public static void Postfix(FarmAnimal __instance, string type, string __state)
            {
                if (!Config.EnableMod || __state == type || (!type.EndsWith("Chicken") && !type.EndsWith("Cow")))
                    return;
                __instance.type.Value = __state;
                __instance.reloadData();
            }
        }

    }
}