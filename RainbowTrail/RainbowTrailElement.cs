using Microsoft.Xna.Framework;

namespace RainbowTrail
{
	public class RainbowTrailElement
	{
		public Vector2 Position;
		public int Direction;
		public int Duration;

		public RainbowTrailElement(Vector2 position, int direction)
		{
			Position = position;
			Direction = direction;
			Duration = ModEntry.Config.MaxDuration;
		}
	}
}
