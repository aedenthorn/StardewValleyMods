using StardewModdingAPI;
using StardewValley;

namespace AllChestsMenu
{
	public partial class ModEntry
	{
		public static void OpenMenu()
		{
			if (!Config.ModEnabled || !Context.IsPlayerFree)
				return;

			Game1.activeClickableMenu = new AllChestsMenu();
			Game1.playSound("bigSelect");
		}
	}
}
