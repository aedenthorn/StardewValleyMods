using FashionSense.Framework.UI;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;

namespace FashionSenseAppearanceMenu
{
    public partial class ModEntry
    {
        public static Point buttonOffset = new Point(12, 0);
        public static Point buttonSize = new Point(66, 60);
        public static bool CheckButton(ClickableComponent label, int x, int y)
        {
            return label.visible && new Rectangle(label.bounds.Location - buttonOffset, buttonSize).Contains(x, y);
        }
        public static void DrawButton(SpriteBatch b, ClickableComponent label)
        {
            b.Draw(Game1.staminaRect, new Rectangle(label.bounds.Location - buttonOffset, buttonSize), Color.White * 0.25f);
        }

        public static void OpenMenu(HandMirrorMenu mirrorMenu)
        {
            string filter = (string)AccessTools.Method(typeof(HandMirrorMenu), "GetNameOfEnabledFilter").Invoke(mirrorMenu, Array.Empty<Type>());
            Game1.activeClickableMenu = new FashionSenseAppearanceMenuMenu(mirrorMenu, Game1.player, filter);
            Game1.playSound("bigSelect");
        }

    }
}