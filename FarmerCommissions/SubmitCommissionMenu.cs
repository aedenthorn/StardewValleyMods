using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Quests;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FarmerCommissions
{
    public class SubmitCommissionMenu : IClickableMenu
    {
        public TextBox nameBox;
        public TextBox amountBox;
        public TextBox daysBox;
        public string nameString;
        public string amountString;
        public string daysString;
        public ClickableComponent nameBoxCC;
        public ClickableComponent amountBoxCC;
        public ClickableComponent daysBoxCC;
        public ClickableTextureComponent okButton;

        public SubmitCommissionMenu() : base(Game1.uiViewport.Width / 2 - (Game1.uiViewport.Width / 6 + borderWidth), Game1.uiViewport.Height / 2 - (Game1.uiViewport.Height / 6 + borderWidth), Game1.uiViewport.Width / 3 + borderWidth * 2, Game1.uiViewport.Height / 3 + borderWidth * 2, false)
        {
            nameString = "";
            amountString = "1";
            daysString = "1";
            nameBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
            {
                X = xPositionOnScreen + borderWidth,
                Width = width - borderWidth / 2,
                Y = yPositionOnScreen + borderWidth
            };
            nameBoxCC = new ClickableComponent(new Rectangle(nameBox.X, nameBox.Y, nameBox.Width, nameBox.Height), "")
            {
                myID = 1,
                downNeighborID = 2,
            };
            amountBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
            {
                X = nameBox.X,
                Width = nameBox.Width,
                Y = nameBox.Y + nameBox.Height,
            };
            amountBoxCC = new ClickableComponent(new Rectangle(amountBox.X, amountBox.Y, amountBox.Width, amountBox.Height), "")
            {
                myID = 2,
                downNeighborID = 3,
                upNeighborID = 1
            };
            daysBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
            {
                X = amountBox.X,
                Width = amountBox.Width,
                Y = amountBox.Y + amountBox.Height,
            };
            daysBoxCC = new ClickableComponent(new Rectangle(daysBox.X, daysBox.Y, daysBox.Width, daysBox.Height), "")
            {
                myID = 3,
                upNeighborID = 2,
                downNeighborID = 4
            };
            okButton = new ClickableTextureComponent(new Rectangle(daysBox.X, daysBox.Y + daysBox.Height, 48, 48), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 0.75f, false)
            {
                myID = 4,
                upNeighborID = 3
            };
        }
        public override void draw(SpriteBatch b)
        {
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true, null, false, true);

            nameBox.Draw(b);
            amountBox.Draw(b);
            daysBox.Draw(b);
            okButton.draw(b);
            base.draw(b); 
            if (!Game1.options.snappyMenus)
            {
                Game1.mouseCursorTransparency = 1f;
                drawMouse(b, false, -1);
            }
        }
        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);
            if (amountBox.Selected)
            {
                if(Regex.IsMatch(amountBox.Text, @"^[0-9]*$", RegexOptions.Compiled))
                {
                    amountString = amountBox.Text;
                }
                else
                {
                    amountBox.Text = amountString;
                }
            }
            else if (daysBox.Selected)
            {
                if(Regex.IsMatch(daysBox.Text, @"^[0-9]*$", RegexOptions.Compiled))
                {
                    daysString = daysBox.Text;
                }
                else
                {
                    daysBox.Text = daysString;
                }
            }
            else if (nameBox.Selected)
            {
                if(Regex.IsMatch(nameBox.Text, @"^[A-Za-z]*$", RegexOptions.Compiled))
                {
                    nameString = nameBox.Text;
                }
                else
                {
                    nameBox.Text = nameString;
                }
            }
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            nameBox.Selected = false;
            amountBox.Selected = false;
            daysBox.Selected = false;
            nameBox.Update();
            amountBox.Update();
            daysBox.Update();
            if(okButton.containsPoint(x, y))
            {
                Game1.playSound("bigDeSelect");
                Object item = GetObject(nameString, int.Parse(amountString));
                if(item is null)
                {
                    return;
                }

                ModEntry.helpWantedAPI.AddQuestToday(new QuestData()
                {
                    quest = MakeQuest(Game1.player, item, int.Parse(daysString))
                });
            }
        }

        public static Object GetObject(string nameString, int stack)
        {
            try
            {
                int index = Game1.objectInformation.First(p => p.Value.StartsWith(nameString + "/")).Key;
                return new Object(index, stack);
            }
            catch
            {
                return null;
            }
        }

        public static Quest MakeQuest(Farmer farmer, Item item, int days)
        {
            var quest = new ItemDeliveryQuest();
            quest.target.Value = farmer.Name;
            quest.questTitle = ModEntry.SHelper.Translation.Get("quest-title");
            quest.item.Value = item.ParentSheetIndex;
            quest.daysLeft.Value = days;
            quest.questDescription = string.Format(ModEntry.SHelper.Translation.Get("quest-description"), item.Stack, item.DisplayName, farmer.Name);
            return quest;
        }
    }
}