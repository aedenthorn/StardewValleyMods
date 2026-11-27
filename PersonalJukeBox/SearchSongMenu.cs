using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Minigames;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PersonalJukeBox
{
    public class SearchSongMenu : IClickableMenu
    {
        public string text;
        public TextBox searchBox;
        public ClickableComponent cc;
        public List<string> options = new();
        public Dictionary<string, string> allTracksDict = new();
        public List<ClickableComponent> optionCCs = new();
        public SearchSongMenu(int x, int y, int width, int height) : base(x, y, width, height)
        {
            allTracksDict = new();
            foreach (var t in ModEntry.GetTracks(false))
            {
                allTracksDict[Utility.getSongTitleFromCueName(t)] = t;
            }
            options = new();
            searchBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
            {
                X = x,
                Y = y,
                Width = width,
            };
            searchBox.SelectMe();
            cc = new(new(x, y, width, searchBox.Height), "cc");
        }
        public override void draw(SpriteBatch b)
        {
            searchBox.Draw(b);
            var l = searchBox.X + 12;
            var w = searchBox.Width - 18;
            b.Draw(Game1.staminaRect, new Rectangle(l, searchBox.Y + searchBox.Height, w, searchBox.Height * options.Count), Color.Wheat);
            for (int i = 0; i < options.Count; i++)
            {
                var occ = optionCCs[i];
                var list = ModEntry.PlayerList;

                if (occ.bounds.Contains(Game1.getMousePosition(true)))
                {
                    b.Draw(Game1.staminaRect, occ.bounds, Color.Goldenrod);
                }
                if (list.Contains(allTracksDict[options[i]]))
                {
                    b.Draw(Game1.mouseCursors, new Rectangle(occ.bounds.Right - 28, occ.bounds.Top + 16, 16, 16), DropDown.starSource, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.976f);
                }
                b.DrawString(Game1.smallFont, options[i], new Vector2(searchBox.X + 16, occ.bounds.Top + 8), Game1.textColor);
            }
            drawMouse(b);
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if(cc.containsPoint(x, y))
            {
                searchBox.Update();
            }
            for(int i = 0; i < optionCCs.Count; i++)
            {
                if (optionCCs[i].containsPoint(x, y))
                {
                    if(ModEntry.Menu is not null)
                    {
                        ModEntry.Menu.ChangeMusic(allTracksDict[options[i]]);
                        ModEntry.Menu.searching = false;
                    }
                    exitThisMenu();
                    return;
                }
            }
        }
        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if(cc.containsPoint(x, y))
            {
                searchBox.Update();
            }
            for(int i = 0; i < optionCCs.Count; i++)
            {
                if (optionCCs[i].containsPoint(x, y))
                {
                    ModEntry.ToggleFavorite(allTracksDict[options[i]]);
                    if(ModEntry.Menu is not null)
                    {
                        ModEntry.Menu.UpdateSongFlags();
                    }
                    return;
                }
            }
        }
        public override void receiveKeyPress(Keys key)
        {
            if (key >= Keys.A && key <= Keys.Z)
            {

            }
            else if (key == Keys.Space)
            {

            }
            else if(ModEntry.Menu is not null && Game1.options.doesInputListContain(Game1.options.menuButton, key))
            {
                ModEntry.Menu.searching = false;
                exitThisMenu();
            }
            Game1.playSound("shiny4");
        }
        public override void update(GameTime time)
        {
            if(text != searchBox.Text)
            {
                options.Clear();
                text = searchBox.Text;
                if(text.Length < 2)
                {
                }
                else
                {
                    var lower = text.ToLower();
                    foreach (var kvp in allTracksDict)
                    {
                        if (kvp.Key.ToLower().Contains(lower) || kvp.Value.ToLower().Contains(lower))
                        {
                            options.Add(kvp.Key);
                        }
                    }
                    optionCCs = new();
                    var l = searchBox.X + 12;
                    var w = searchBox.Width - 18;
                    for (int i = 0; i < options.Count; i++)
                    {
                        var t = searchBox.Y + searchBox.Height * (i + 1);
                        optionCCs.Add(new(new(l, t, w, searchBox.Height), $"cc{i}"));
                    }
                }
            }
        }
    }
}