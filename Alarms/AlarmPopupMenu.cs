using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace Alarms
{
    internal class AlarmPopupMenu : IClickableMenu
    {
        private string notification;
        public AlarmPopupMenu(string notification)
        {
            this.notification = notification;
        }
        public override void draw(SpriteBatch b)
        {
            var size = Game1.dialogueFont.MeasureString(notification);
            var posx = Game1.viewport.Width / 2 - size.X / 2;
            var posy = Game1.viewport.Height / 2 - size.Y / 2;
            xPositionOnScreen = (int)posx - borderWidth;
            yPositionOnScreen = (int)posy - borderWidth;
            width = (int)size.X + borderWidth * 2 + 8;
            height = (int)size.Y + borderWidth * 2 + 48;
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true, null, false, true);
            b.DrawString(Game1.dialogueFont, notification, new Vector2(posx, posy + 32 + size.Y / 2), Color.Black);
            base.draw(b);
            drawMouse(b);
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            exitThisMenu(true);

        }
    }
}