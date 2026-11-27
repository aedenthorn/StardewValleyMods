using System;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;

namespace BossCreatures
{
	public class ToughFly : Fly
	{
		public ToughFly() : base()
		{
			hard = true;
		}

		public ToughFly(Vector2 position, float difficulty) : base(position, true)
		{
			Health = (int)Math.Round(Health * 1.5 * difficulty);
		}

		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			base.MovePosition(time, viewport, currentLocation);
		}
	}
}
