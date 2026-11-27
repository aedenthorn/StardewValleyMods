using StardewValley;

namespace AFKPause
{
	public partial class ModEntry
	{
		public class Game1_UpdateGameClock_Patch
		{
			public static bool Prefix()
			{
				if (!Config.ModEnabled || !Game1.IsMasterGame || Game1.eventUp || Game1.isFestival() || elapsedTicks < Config.TicksTilAFK)
					return true;
				return false;
			}
		}
	}
}
