using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Survivors
{
    public interface IStardewGamesAPI
    {
        public void AddGame(string name, Action<Rectangle, int, int> clickAction, Action<SpriteBatch, Rectangle> drawAction);
        public void ReturnToMenu();
    }
}