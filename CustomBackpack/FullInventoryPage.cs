using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System.Reflection;

namespace CustomBackpack
{
    internal class FullInventoryPage : InventoryPage
    {

        FieldInfo trashRotField = AccessTools.Field(typeof(InventoryPage), "trashCanLidRotation");
        FieldInfo hoverTextField = AccessTools.Field(typeof(InventoryPage), "hoverText");
        FieldInfo hoverAmountField = AccessTools.Field(typeof(InventoryPage), "hoverAmount");
        FieldInfo hoverTitleField = AccessTools.Field(typeof(InventoryPage), "hoverTitle");
        FieldInfo hoveredItemField = AccessTools.Field(typeof(InventoryPage), "hoveredItem");

        public FullInventoryPage(InventoryMenu instance, int x, int y, int width, int height) : base(x, y, width, height)
        {

            inventory = new FullInventoryMenu(instance);
            equipmentIcons?.Clear();
            if(junimoNoteIcon is not null)
                junimoNoteIcon.bounds = new Rectangle();
            if (portrait is not null)
                portrait.bounds = new Rectangle();
            exitFunction = delegate ()
            {
                ModEntry.scrolled = ModEntry.oldScrolled;
                Game1.activeClickableMenu = ModEntry.lastMenu.Value;
            };
        }
        public override void draw(SpriteBatch b)
        {
            xPositionOnScreen = inventory.xPositionOnScreen;
            yPositionOnScreen = inventory.yPositionOnScreen - 36;
            width = inventory.width;
            height = inventory.height - 136;
            inventory.draw(b);

            float trashCanLidRotation = (float)trashRotField.GetValue(this);
            string hoverText = (string)hoverTextField.GetValue(this);
            int hoverAmount = (int)hoverAmountField.GetValue(this);
            string hoverTitle = (string)hoverTitleField.GetValue(this);
            Item hoveredItem = (Item)hoveredItemField.GetValue(this);
            int num = width / 3;
            if (organizeButton != null)
            {
                organizeButton.bounds.X = xPositionOnScreen + width + 64;
                organizeButton.draw(b);
            }
            trashCan.bounds.X = xPositionOnScreen + width + 64;
            trashCan.bounds.Y = organizeButton.bounds.Y + 256;
            trashCan.draw(b);
            b.Draw(Game1.mouseCursors, new Vector2(trashCan.bounds.X + 60, trashCan.bounds.Y + 40), new Rectangle?(new Rectangle(564 + Game1.player.trashCanLevel * 18, 129, 18, 10)), Color.White, trashCanLidRotation, new Vector2(16f, 10f), 4f, SpriteEffects.None, 0.86f);
            if (checkHeldItem(null))
            {
                Game1.player.CursorSlotItem.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 16, Game1.getOldMouseY() + 16), 1f);
            }
            if (hoverText != null && !hoverText.Equals(""))
            {
                if (hoverAmount > 0)
                {
                    drawToolTip(b, hoverText, hoverTitle, null, true, -1, 0, -1, -1, null, hoverAmount);
                }
                else
                {
                    drawToolTip(b, hoverText, hoverTitle, hoveredItem, checkHeldItem(null), -1, 0, -1, -1, null, -1);
                }
            }
            drawMouse(b);
        }
    }
}