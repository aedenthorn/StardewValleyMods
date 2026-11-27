using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace StardewGames
{
    public class GamesGameData
    {
        public Action<Rectangle, int, int> clickAction;
        public Action<SpriteBatch, Rectangle> drawAction;

        public GamesGameData(Action<Rectangle, int, int> clickAction, Action<SpriteBatch, Rectangle> drawAction)
        {
            this.clickAction = clickAction;
            this.drawAction = drawAction;
        }
    }
}