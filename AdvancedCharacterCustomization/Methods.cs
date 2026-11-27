using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;

namespace AdvancedCharacterCustomization
{
    public partial class ModEntry
    {
        public static Point buttonOffset = new Point(12, 0);
        public static Point buttonSize = new Point(66, 60);
        public static bool CheckButton(ClickableComponent label, int x, int y)
        {
            return label?.visible == true && new Rectangle(label.bounds.Location - buttonOffset, buttonSize).Contains(x, y);
        }
        public static void DrawButton(SpriteBatch b, ClickableComponent label)
        {
            b.Draw(Game1.staminaRect, new Rectangle(label.bounds.Location - buttonOffset, buttonSize), Color.White * 0.25f);
        }

    }
}