using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace SeedInfo
{
    public partial class ModEntry
    {
        public static Dictionary<int, SeedEntryInfo> shopDict = new();


        [HarmonyPatch(typeof(ShopMenu), nameof(ShopMenu.draw))]
        public class ShopMenu_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling ShopMenu.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i + 1].opcode == OpCodes.Ldfld && (FieldInfo)codes[i + 1].operand == AccessTools.Field(typeof(ShopMenu), "hoverText"))
                    {
                        SMonitor.Log("Adding draw method");
                        codes.Insert(i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DrawAllInfo))));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_1));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
            public static void Postfix(ShopMenu __instance)
            {
                if (!Config.ModEnabled)
                    return;
                shopDict.Clear();
                for (int i = 0; i < __instance.forSale.Count; i++)
                {
                    if (__instance.forSale[i] is not Object || (__instance.forSale[i] as Object).Category != Object.SeedsCategory)
                        continue;
                    shopDict[((Object)__instance.forSale[i]).ParentSheetIndex] = new SeedEntryInfo((Object)__instance.forSale[i]);
                }
            }
        }

        [HarmonyPatch(typeof(IClickableMenu), nameof(IClickableMenu.snapCursorToCurrentSnappedComponent))]
        public class IClickableMenu_snapCursorToCurrentSnappedComponent_Patch
        {
            public static bool Prefix(IClickableMenu __instance)
            {
                if (!Config.ModEnabled || __instance is not ShopMenu menu || __instance.currentlySnappedComponent is not ClickableComponent cc || !menu.forSaleButtons.Contains(cc))
                    return true;
                Game1.setMousePosition(cc.bounds.Right - cc.bounds.Width / 8, cc.bounds.Bottom - cc.bounds.Height / 4, true);
                return false;
            }
        }
    }
}