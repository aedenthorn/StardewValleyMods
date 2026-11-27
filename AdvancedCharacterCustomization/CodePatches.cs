using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;

namespace AdvancedCharacterCustomization
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(CharacterCustomization), nameof(CharacterCustomization.receiveLeftClick))]
        public class CharacterCustomization_receiveLeftClick_Patch
        {

            public static bool Prefix(CharacterCustomization __instance, int x, int y, bool playSound, ClickableComponent ___skinLabel, ClickableComponent ___hairLabel, ClickableComponent ___accLabel, ClickableComponent ___shirtLabel, ClickableComponent ___pantsStyleLabel, Farmer ____displayFarmer, bool ____isDyeMenu)
            {
                if (!Config.ModEnabled || ____isDyeMenu || __instance.showingCoopHelp)
                    return true;
                StyleType which;
                if (CheckButton(___skinLabel, x, y))
                {
                    which = StyleType.Skin;
                }
                else if (CheckButton(___hairLabel, x, y))
                {
                    which  = StyleType.Hair;
                }
                else if (CheckButton(___shirtLabel, x, y))
                {
                    which  = StyleType.Shirt;
                }
                else if (CheckButton(___pantsStyleLabel, x, y))
                {
                    which  = StyleType.Pants;
                }
                else if (CheckButton(___accLabel, x, y))
                {
                    which  = StyleType.Acc;
                }
                else
                {
                    return true;
                }
                if (Game1.activeClickableMenu == __instance)
                {
                    Game1.activeClickableMenu = new AdvancedCharacterCustomizationMenu(__instance, ____displayFarmer, which);
                }
                else if (TitleMenu.subMenu == __instance)
                {
                    TitleMenu.subMenu = new AdvancedCharacterCustomizationMenu(__instance, ____displayFarmer, which);
                }
                else 
                {
                    return true;
                } 
                if (playSound)
                {
                    Game1.playSound("bigSelect");
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(CharacterCustomization), nameof(CharacterCustomization.draw))]
        public class CharacterCustomization_draw_Patch
        {

            public static void Postfix(CharacterCustomization __instance, SpriteBatch b, ClickableComponent ___skinLabel, ClickableComponent ___hairLabel, ClickableComponent ___accLabel, ClickableComponent ___shirtLabel, ClickableComponent ___pantsStyleLabel, Farmer ____displayFarmer, bool ____isDyeMenu)
            {
                if (!Config.ModEnabled || ____isDyeMenu || __instance.showingCoopHelp)
                    return;
                var x = Game1.getMouseX(true);
                var y = Game1.getMouseY(true);
                if (CheckButton(___skinLabel, x, y))
                {
                    DrawButton(b, ___skinLabel);
                }
                else if (CheckButton(___hairLabel, x, y))
                {
                    DrawButton(b, ___hairLabel);
                }
                else if (CheckButton(___shirtLabel, x, y))
                {
                    DrawButton(b, ___shirtLabel);
                }
                else if (CheckButton(___pantsStyleLabel, x, y))
                {
                    DrawButton(b, ___pantsStyleLabel);
                }
                else if (CheckButton(___accLabel, x, y))
                {
                    DrawButton(b, ___accLabel);
                }
            }
        }

        [HarmonyPatch(typeof(TitleMenu), nameof(TitleMenu.receiveKeyPress))]
        public class TitleMenu_receiveKeyPress_Patch
        {

            public static bool Prefix(Keys key)
            {
                if (!Config.ModEnabled || TitleMenu.subMenu is not AdvancedCharacterCustomizationMenu acc)
                    return true;
                acc.receiveKeyPress(key);
                return false;
            }
        }

        [HarmonyPatch(typeof(TitleMenu), nameof(TitleMenu.backButtonPressed))]
        public class TitleMenu_backButtonPressed_Patch
        {

            public static bool Prefix()
            {
                if (!Config.ModEnabled || TitleMenu.subMenu is not AdvancedCharacterCustomizationMenu acc)
                    return true;
                Game1.playSound("bigDeSelect");
                TitleMenu.subMenu = acc.menu;
                return false;
            }
        }
    }
}