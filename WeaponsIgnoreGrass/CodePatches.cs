using StardewValley;
using StardewValley.Tools;

namespace WeaponsIgnoreGrass
{
	public partial class ModEntry
	{
		public class Grass_performToolAction_Patch
		{
			public static bool Prefix(Tool t, int explosion)
			{
				if (!Config.ModEnabled || explosion > 0)
					return true;

				if (t is MeleeWeapon)
				{
					if ((Config.WeaponsIgnoreGrass && !t.isScythe()) || (Config.ScythesIgnoreGrass && t.isScythe()))
					{
						return false;
					}
				}
				return true;
			}
		}
	}
}
