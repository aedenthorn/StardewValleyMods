using Microsoft.Xna.Framework;
using StardewValley.BellsAndWhistles;

namespace AnimatedParrotAndPerch
{
	internal class PerchParrot : EmilysParrot
	{
		public Vector2 tile;

		public PerchParrot(Vector2 location, Vector2 tile) : base(location)
		{
			this.tile = tile;
			layerDepth = 1;
		}
	}
}
