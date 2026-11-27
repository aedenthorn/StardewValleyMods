using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData;
using StardewValley.Menus;
using StardewValley.Minigames;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace StardewGames
{
	public partial class ModEntry : Mod
    {
        public static int ticks;
        public static void DrawPrairieKing(SpriteBatch b, Rectangle area)
        {
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), area.X, area.Y, area.Width, area.Height, Color.White, 4f, false, -1f);
            var menuRect = new Rectangle(area.Location + new Point(8, 8), area.Size - new Point(16, 16));
            b.Draw(SHelper.ModContent.Load<Texture2D>("assets/prairieMenu.png"), menuRect, Color.White);

            var buttonWidth = menuRect.Width - 768;

            var newRect = new Rectangle(menuRect.Location + new Point(768, 0), new Point(buttonWidth, menuRect.Height / 2));
            var resumeRect = new Rectangle(menuRect.Location + new Point(768, menuRect.Height / 2), new Point(buttonWidth, menuRect.Height / 2));
            var newString = Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_NewGame");
            var resumeString = Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_Continue");
            var newSize = Game1.smallFont.MeasureString(newString);
            var resumeSize = Game1.smallFont.MeasureString(resumeString);
            SpriteText.drawStringHorizontallyCenteredAt(b, SHelper.Translation.Get("prairie-text"), menuRect.X + 768 / 2, menuRect.Y + 4, color: new Color(50,0,0));
            
            b.Draw(Game1.staminaRect, new Rectangle(menuRect.X + 768 + 4, menuRect.Y + menuRect.Height / 2, buttonWidth - 8, 2), Color.White);
            
            var mpos = Game1.getMousePosition();
            var newColor = Color.White;
            var resumeColor = Color.White;
            var speed = 20;
            if (newRect.Contains(mpos))
            {
                ticks++;
                ticks %= speed;
                if(ticks > speed / 2)
                {
                    newColor = Color.Gold;
                }
            }
            else if (resumeRect.Contains(mpos))
            {
                ticks++;
                ticks %= speed;
                if(ticks > speed / 2)
                {
                    resumeColor = Color.Gold;
                }
            }
            b.DrawString(Game1.smallFont, newString, new Vector2(menuRect.Right - buttonWidth / 2 - newSize.X / 2, menuRect.Y + menuRect.Height / 4 - newSize.Y / 2), newColor);


            b.DrawString(Game1.smallFont, resumeString, new Vector2(menuRect.Right - buttonWidth / 2 - resumeSize.X / 2, menuRect.Y + menuRect.Height * 3 / 4 - resumeSize.Y / 2), resumeColor);
        }

        public static void ClickPrairieKing(Rectangle area, int x, int y)
        {

            var menuRect = new Rectangle(new Point(8, 8), area.Size - new Point(16, 16));

            var buttonWidth = menuRect.Width - 768;

            var newRect = new Rectangle(menuRect.Location + new Point(768, 8), new Point(buttonWidth, menuRect.Height / 2));
            var resumeRect = new Rectangle(menuRect.Location + new Point(768, menuRect.Height / 2), new Point(buttonWidth, menuRect.Height / 2));
            if (newRect.Contains(x, y))
            {
                Game1.player.jotpkProgress.Value = null;
            }
            else if (!resumeRect.Contains(x, y))
            {
                return;
            }
            Game1.currentSong?.Stop(Microsoft.Xna.Framework.Audio.AudioStopOptions.Immediate);
            Game1.currentMinigame = new AbigailGame(null);
            currentMiniGame = CurrentMiniGame.PrairieKing;
        }
        public static void DrawJunimo(SpriteBatch b, Rectangle area)
        {

            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), area.X, area.Y, area.Width, area.Height, Color.White, 4f, false, -1f);
            var menuRect = new Rectangle(area.Location + new Point(8, 8), area.Size - new Point(16, 16));
            b.Draw(SHelper.ModContent.Load<Texture2D>("assets/kartMenu.png"), menuRect, Color.White);

            var buttonWidth = menuRect.Width - 768;

            var progressRect = new Rectangle(menuRect.Location + new Point(768, 0), new Point(buttonWidth, menuRect.Height / 2));
            var endlessRect = new Rectangle(menuRect.Location + new Point(768, menuRect.Height / 2), new Point(buttonWidth, menuRect.Height / 2));
            var progressString = Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_ProgressMode");
            var endlessString = Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_EndlessMode");
            var progressSize = Game1.smallFont.MeasureString(progressString);
            var endlessSize = Game1.smallFont.MeasureString(endlessString);

            //b.Draw(Game1.staminaRect, new Rectangle(menuRect.X + 768 + 4, menuRect.Y + menuRect.Height / 2, buttonWidth - 8, 2), Color.White);

            var mpos = Game1.getMousePosition();
            var newColor = Color.White;
            var resumeColor = Color.White;
            var speed = 20;
            if (progressRect.Contains(mpos))
            {
                ticks++;
                ticks %= speed;
                if (ticks > speed / 2)
                {
                    newColor = Color.Gold;
                }
            }
            else if (endlessRect.Contains(mpos))
            {
                ticks++;
                ticks %= speed;
                if (ticks > speed / 2)
                {
                    resumeColor = Color.Gold;
                }
            }
            b.DrawString(Game1.smallFont, progressString, new Vector2(menuRect.Right - buttonWidth / 2 - progressSize.X / 2, menuRect.Y + menuRect.Height / 4 - progressSize.Y / 2), newColor);


            b.DrawString(Game1.smallFont, endlessString, new Vector2(menuRect.Right - buttonWidth / 2 - endlessSize.X / 2, menuRect.Y + menuRect.Height * 3 / 4 - endlessSize.Y / 2), resumeColor);
        }

        public static void ClickJunimo(Rectangle area, int x, int y)
        {
            var menuRect = new Rectangle(new Point(8, 8), area.Size - new Point(16, 16));

            var buttonWidth = menuRect.Width - 768;

            var progressRect = new Rectangle(menuRect.Location + new Point(768, 8), new Point(buttonWidth, menuRect.Height / 2));
            var endlessRect = new Rectangle(menuRect.Location + new Point(768, menuRect.Height / 2), new Point(buttonWidth, menuRect.Height / 2));
            int mode = 2;
            if (progressRect.Contains(x, y))
            {
                mode = 3;
            }
            else if (!endlessRect.Contains(x, y))
            {
                return;
            }
            currentMiniGame = CurrentMiniGame.JunimoKart;
            Game1.currentSong?.Stop(Microsoft.Xna.Framework.Audio.AudioStopOptions.Immediate);
            Game1.currentMinigame = new MineCart(0, mode);
        }
        public static NPC RequireCharacter(string name, bool villager)
        {
            if (!Config.ModEnabled || currentMiniGame != CurrentMiniGame.JunimoKart)
                return Game1.RequireCharacter(name, villager);
            return new NPC()
            {
                Name = name
            };
        }
    }
}
