using StardewValley;
using StardewValley.Menus;
using System;

namespace MailboxMenu
{
    internal class MyLetterViewerMenu : LetterViewerMenu
    {
        public MyLetterViewerMenu(string mail, string mailTitle, bool fromCollections) : base(mail, mailTitle, fromCollections)
        {
            exitFunction = delegate ()
            {
                Game1.activeClickableMenu = new MailMenu();
            };
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (scale < 1f)
            {
                return;
            }
            if (upperRightCloseButton != null && readyToClose() && upperRightCloseButton.containsPoint(x, y))
            {
                if (playSound)
                {
                    Game1.playSound("bigDeSelect");
                }
                exitThisMenu(ShouldPlayExitSound());
            }
            else
            {
                base.receiveLeftClick(x, y, playSound);
            }
        }
    }
}