using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace BulkAnimalPurchase
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(PurchaseAnimalsMenu), new Type[] { typeof(List<Object>) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class PurchaseAnimalsMenu_Patch
        {
            public static void Prefix()
            {
                if (!Config.EnableMod)
                    return;
                animalsToBuy = 1;

                Point start = new Point(Game1.uiViewport.Width / 2 + PurchaseAnimalsMenu.menuWidth / 2 - IClickableMenu.borderWidth * 2, (Game1.uiViewport.Height - PurchaseAnimalsMenu.menuHeight - IClickableMenu.borderWidth * 2) / 4) + new Point(-100, PurchaseAnimalsMenu.menuHeight + 120);

                minusButton = new ClickableTextureComponent("BAPMod_minus", new Rectangle(start, new Point(64, 64)), null, "", Game1.mouseCursors, OptionsPlusMinus.minusButtonSource, 4f, false)
                {
                    myID = 200,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                };
                plusButton = new ClickableTextureComponent("BAPMod_plus", new Rectangle(start + new Point(100, 0), new Point(64, 64)), null, "", Game1.mouseCursors, OptionsPlusMinus.plusButtonSource, 4f, false)
                {
                    myID = 201,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                };

            }
        }
        private static bool skip = false;
        [HarmonyPatch(typeof(Game1), nameof(Game1.drawDialogueBox), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool),typeof(bool), typeof(string), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(int) })]
        public class Game1_drawDialogueBox_Patch
        {
            public static void Prefix()
            {
                if (!Config.EnableMod || Game1.activeClickableMenu is not PurchaseAnimalsMenu || Game1.IsFading() || skip || AccessTools.FieldRefAccess<PurchaseAnimalsMenu, bool>(Game1.activeClickableMenu as PurchaseAnimalsMenu, "onFarm") || AccessTools.FieldRefAccess<PurchaseAnimalsMenu, bool>(Game1.activeClickableMenu as PurchaseAnimalsMenu, "namingAnimal"))
                    return;
                var menu = Game1.activeClickableMenu;
                var b = Game1.spriteBatch;
                skip = true;
                Game1.drawDialogueBox(menu.xPositionOnScreen, menu.yPositionOnScreen + menu.height - 100, menu.width, 200, false, true, null, false, true, -1, -1, -1);
                skip = false;
                Utility.drawTextWithShadow(b, SHelper.Translation.Get("amount"), Game1.dialogueFont, new Vector2(menu.xPositionOnScreen + 40, menu.yPositionOnScreen + menu.height + 10), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
                minusButton.draw(b);
                Utility.drawTextWithShadow(b, animalsToBuy + "", Game1.dialogueFont, new Vector2(menu.xPositionOnScreen + menu.width - 116 - Game1.dialogueFont.MeasureString(animalsToBuy + "").X / 2, menu.yPositionOnScreen + menu.height + 10), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
                plusButton.draw(b);
            }
        }
        [HarmonyPatch(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.draw), new Type[] { typeof(SpriteBatch) })]
        public class PurchaseAnimalsMenu_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling PurchaseAnimalsMenu.draw");
                var codes = new List<CodeInstruction>(instructions);
                bool found = false;
                for (int i = 0; i < codes.Count; i++)
                {
                    if (found && codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(LocalizedContentManager), nameof(LocalizedContentManager.LoadString), new Type[] { typeof(string),typeof(object),typeof(object) }))
                    {
                        SMonitor.Log("Adding to string result");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, typeof(ModEntry).GetMethod(nameof(ModEntry.AddToString))));
                        break;
                    }
                    else if (!found && codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11355")
                    {
                        SMonitor.Log("found string 11355");
                        found = true;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.performHoverAction))]
        public class PurchaseAnimalsMenu_performHoverAction_Patch
        {
            public static bool Prefix(PurchaseAnimalsMenu __instance, int x, int y, bool ___freeze, bool ___onFarm, bool ___namingAnimal)
            {
                if (!Config.EnableMod || Game1.IsFading() || ___freeze || ___onFarm || ___namingAnimal)
                    return true;
                if (minusButton != null && minusButton.containsPoint(x, y) && animalsToBuy > 1)
                {
                    __instance.hovered = minusButton;
                }
                if (plusButton != null && plusButton.containsPoint(x, y))
                {
                    __instance.hovered = plusButton;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.setUpForReturnAfterPurchasingAnimal))]
        public class PurchaseAnimalsMenu_setUpForReturnAfterPurchasingAnimal_Patch
        {
            public static bool Prefix(PurchaseAnimalsMenu __instance, ref FarmAnimal ___animalBeingPurchased, int ___priceOfAnimal)
            {
                if (!Config.EnableMod || animalsToBuy <= 1)
                    return true;
                animalsToBuy--;
                Game1.addHUDMessage(new HUDMessage(___animalBeingPurchased.isMale() ? Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11311", ___animalBeingPurchased.displayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11314", ___animalBeingPurchased.displayName), 1));

                string type = ___animalBeingPurchased.type.Value;
                if (!SHelper.ModRegistry.IsLoaded("aedenthorn.LivestockChoices")) 
                {
                    if (type.EndsWith(" Chicken") && !type.Equals("Void Chicken") && !type.Equals("Golden Chicken"))
                    {
                        type = "Chicken";
                    }
                    else if (type.EndsWith(" Cow"))
                    {
                        type = "Cow";
                    }
                }
                ___animalBeingPurchased = new FarmAnimal(type, new Multiplayer().getNewID(), Game1.player.UniqueMultiplayerID);
                SMonitor.Log($"next animal type: {___animalBeingPurchased.type}; price {___priceOfAnimal}, funds left {Game1.player.Money}");
                return false;
            }
        }
        [HarmonyPatch(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.receiveLeftClick))]
        public class PurchaseAnimalsMenu_receiveLeftClick_Patch
        {
            public static bool Prefix(PurchaseAnimalsMenu __instance, int x, int y, bool ___freeze, bool ___onFarm, bool ___namingAnimal, int ___priceOfAnimal, ref int __state)
            {
                __state = ___priceOfAnimal;
                if (!Config.EnableMod || Game1.IsFading() || ___freeze || ___onFarm || ___namingAnimal)
                    return true;
                if (minusButton != null && minusButton.containsPoint(x, y) && animalsToBuy > 1)
                {
                    Game1.playSound("smallSelect");
                    animalsToBuy--;
                    return false;
                }
                if (plusButton != null && plusButton.containsPoint(x, y))
                {
                    Game1.playSound("smallSelect");
                    animalsToBuy++;
                    return false;
                }
                return true;
            }
            public static void Postfix(int __state, ref int ___priceOfAnimal) 
            {
                if (!Config.EnableMod || __state == ___priceOfAnimal)
                    return;
                ___priceOfAnimal /= animalsToBuy;
                SMonitor.Log($"Price of animal: {___priceOfAnimal}x{animalsToBuy}");
            }

        }
        [HarmonyPatch(typeof(Object), nameof(Object.salePrice))]
        public class Item_salePrice_Patch
        {
            public static void Postfix(ref int __result)
            {
                if (!Config.EnableMod || Game1.activeClickableMenu is not PurchaseAnimalsMenu)
                    return;
                __result *= animalsToBuy;
            }
        }
        [HarmonyPatch(typeof(SpriteText), nameof(SpriteText.drawStringWithScrollBackground))]
        public class SpriteText_drawStringWithScrollBackground_Patch
        {
            public static void Prefix(ref string s, ref string placeHolderWidthText)
            {
                if (!Config.EnableMod || Game1.activeClickableMenu is not PurchaseAnimalsMenu || animalsToBuy <= 1 || (placeHolderWidthText != "Golden Chicken" && placeHolderWidthText != "Truffle Pig"))
                    return;
                s += " x" + animalsToBuy;
                placeHolderWidthText += " x" + animalsToBuy;
            }
        }
    }
}