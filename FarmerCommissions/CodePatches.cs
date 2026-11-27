using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;
using System;
using StardewValley.Quests;
using Object = StardewValley.Object;
using Microsoft.Xna.Framework;
using StardewValley.Monsters;
using Microsoft.Xna.Framework.Graphics;

namespace FarmerCommissions
{
    public partial class ModEntry
    {

        private static ClickableComponent submitButton;

        [HarmonyPatch(typeof(Billboard), new Type[] { typeof(bool) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class Billboard_Patch
        {

            public static void Postfix(Billboard __instance)
            {
                if (!Config.ModEnabled || __instance.GetType().Name != "OrdersBillboard")
                    return;
                submitButton = new ClickableComponent(new Rectangle(__instance.xPositionOnScreen + __instance.width / 2 - 128, __instance.yPositionOnScreen + __instance.height - 128, (int)Game1.dialogueFont.MeasureString(SHelper.Translation.Get("submit-commission")).X + 24, (int)Game1.dialogueFont.MeasureString(SHelper.Translation.Get("submit-commission")).Y + 24), "");
            }
        }
        [HarmonyPatch(typeof(IClickableMenu), nameof(IClickableMenu.draw), new Type[] { typeof(SpriteBatch) })]
        public class IClickableMenu_draw_Patch
        {
            public static void Postfix(IClickableMenu __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || __instance.GetType().Name != "OrdersBillboard")
                    return;
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), submitButton.bounds.X, submitButton.bounds.Y, submitButton.bounds.Width, submitButton.bounds.Height, (submitButton.scale > 1f) ? Color.LightPink : Color.White, 4f * submitButton.scale, true, -1f);
                Utility.drawTextWithShadow(b, SHelper.Translation.Get("submit-commission"), Game1.dialogueFont, new Vector2((float)(submitButton.bounds.X + 12), (float)(submitButton.bounds.Y + (LocalizedContentManager.CurrentLanguageLatin ? 16 : 12))), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
            }
        }
        [HarmonyPatch(typeof(IClickableMenu), nameof(IClickableMenu.receiveLeftClick))]
        public class Billboard_receiveLeftClick_Patch
        {
            public static bool Prefix(IClickableMenu __instance, int x, int y)
            {
                if (!Config.ModEnabled || __instance.GetType().Name != "OrdersBillboard" || submitButton is null || !submitButton.containsPoint(x, y))
                    return true;
                Game1.playSound("bigSelect");
                Game1.activeClickableMenu = new SubmitCommissionMenu();
                return false;
            }
        }

    }
}