using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using static StardewValley.Minigames.TargetGame;

namespace GenieLamp
{
    internal class ObjectPickMenu : NamingMenu
    {
        string lastText = "";
        List<string> names = new();
        public ObjectPickMenu(doneNamingBehavior b, string title, string defaultName = "") : base(b, title, defaultName)
        {
            int adjust = 150;
            textBox.Width += adjust * 2;
            textBox.X -= adjust;
            doneNamingButton.bounds.Offset(new Vector2(adjust, 0));
            randomButton.bounds.Offset(new Vector2(adjust, 0));
            textBox.textLimit = 999;
        }
        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            if(names is null)
            {
                names = new List<string>();
            }
            if(!string.IsNullOrEmpty(textBox.Text) && textBox.Text != lastText)
            {
                lastText = textBox.Text;
                names.Clear();
                var dict = AccessTools.StaticFieldRefAccess<Dictionary<string, ItemMetadata>>(typeof(ItemRegistry), "CachedItems");
                var lower = lastText.ToLower();
                foreach (var kvp in dict)
                {
                    var data = kvp.Value.GetParsedData();
                    if (data.DisplayName.ToLower().StartsWith(lower) && !names.Contains(data.DisplayName))
                        names.Add(data.DisplayName);
                }
                names.Sort();
            }
            if (names.Any())
            {
                for (int i = 0; i < names.Count; i++)
                {
                    var lineY = textBox.Y + 64 + (i + 1) * Game1.smallFont.LineSpacing;
                    b.DrawString(Game1.smallFont, names[i], new Vector2(textBox.X, lineY), Color.LightGray);
                    if (ModEntry.SHelper.Input.IsDown(SButton.MouseLeft) && new Rectangle(textBox.X, lineY, (int)Game1.smallFont.MeasureString(names[i]).X, Game1.smallFont.LineSpacing).Contains(Game1.getMousePosition(true)))
                    {
                        textBox.Text = names[i];
                        break;
                    }
                }
                drawMouse(b);
            }
        }
        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);
            if(key == Keys.Escape)
            {
                exitThisMenu();
            }
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (randomButton.containsPoint(x, y))
            {
                var dict = AccessTools.StaticFieldRefAccess<Dictionary<string, ItemMetadata>>(typeof(ItemRegistry), "CachedItems");
                var keys = dict.Keys.ToArray();
                textBox.Text = dict[keys[Game1.random.Next(keys.Length)]].GetParsedData().DisplayName;
                randomButton.scale = this.randomButton.baseScale;
                Game1.playSound("drumkit6", null);
            }
            else
            {
                base.receiveLeftClick(x, y, playSound);
            }
        }
    }
}