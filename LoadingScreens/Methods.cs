using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace LoadingScreens
{
    public partial class ModEntry
    {
        private void DrawLoadingScreen(SpriteBatch spriteBatch) // 2/x  2/3
        {
            if(currentLoadingScreen is null)
            {
                GetNewLoadingScreen();
            }
            if (currentLoadingScreen is null)
                return;
            currentLoadingScreen.text = "Pelican Town is located within the Ferngill Republic, at war with the Gotoro Empire across the Gem Sea.";
            Rectangle screen = new Rectangle(0,0, Game1.viewport.Width, Game1.viewport.Height);
            if(currentLoadingScreen.texture.Width * Game1.viewport.Height != currentLoadingScreen.texture.Height * Game1.viewport.Width)
            {
                var diff = currentLoadingScreen.texture.Width * Game1.viewport.Height - currentLoadingScreen.texture.Height * Game1.viewport.Width;
                if(diff > 0)
                {
                    var width = currentLoadingScreen.texture.Width / currentLoadingScreen.texture.Height * Game1.viewport.Height;
                    screen = new Rectangle(Game1.viewport.Width / 2 - width / 2, 0, width, Game1.viewport.Height);
                }
                else
                {
                    var height = currentLoadingScreen.texture.Height / currentLoadingScreen.texture.Width * Game1.viewport.Width;
                    screen = new Rectangle(0, Game1.viewport.Height / 2 - height / 2, Game1.viewport.Width, height);
                }
            }
            spriteBatch.Draw(currentLoadingScreen.texture, screen, Color.White * Game1.fadeToBlackAlpha);
            SpriteText.drawStringWithScrollCenteredAt(spriteBatch, currentLoadingScreen.text, Game1.viewport.Width / 2, Game1.viewport.Height - 200, Game1.viewport.Width - 100);
        }

        private void GetNewLoadingScreen()
        {
            if (!screenDict.Any())
                return;
            currentLoadingScreen = screenDict[Game1.random.Next(screenDict.Count)].Value;
        }
    }
}