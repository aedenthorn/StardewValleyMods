using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using System;

namespace Terrarium
{
    internal class TerrariumSprite : TemporaryAnimatedSprite
    {
        public Vector2 tileLocation;

        public TerrariumSprite(Vector2 tileLocation)
        {
            this.tileLocation = tileLocation;
            texture = Game1.mouseCursors;
            sourceRect = new Microsoft.Xna.Framework.Rectangle(641, 1534, 48, 37);
            animationLength = 1;
            sourceRectStartingPos = new Vector2(641f, 1534f);
            interval = 5000f;
            totalNumberOfLoops = 9999;
            position = tileLocation * 64f + new Vector2(0f, -5f) * 4f;
            scale = 4f;
	        layerDepth = (tileLocation.Y + 2f + 0.1f) * 64f / 10000f;
		}

		public void doAction()
        {
            DelayedAction.playSoundAfterDelay("croak", Game1.random.Next(1000), null, -1);
        }
    }
}