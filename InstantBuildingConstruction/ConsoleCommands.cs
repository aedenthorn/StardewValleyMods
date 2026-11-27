using StardewModdingAPI;
using System;

namespace InstantBuildingConstructionAndUpgrade
{
	public partial class ModEntry
	{
        public void RegisterConsoleCommands()
		{
			SHelper.ConsoleCommands.Add("ibcu_reload", "Reapply modifications to building data and farmhouse renovation data.", IBCU_reload);
		}

		public void IBCU_reload(string command, string[] args)
		{
			InvalidateCaches();
		}

    }
}
