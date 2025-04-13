using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace CraftableTerrarium
{
	public class TerrariumFrogs : SebsFrogs
	{
		public Vector2 tile;

		public TerrariumFrogs(Vector2 tile) : base()
		{
			this.tile = tile;
		}

		public static void DoAction()
		{
			if (!string.IsNullOrEmpty(ModEntry.Config.Sound))
			{
				DelayedAction.playSoundAfterDelay(ModEntry.Config.Sound, Game1.random.Next(1000, 3000));
			}
		}
	}
}
