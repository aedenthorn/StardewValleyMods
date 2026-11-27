using StardewModdingAPI;

namespace OverworldChests
{
	public partial class ModEntry
	{
		internal void RegisterConsoleCommands()
		{
			SHelper.ConsoleCommands.Add("oc_remove", "Remove overworld chests.", OC_remove);
			SHelper.ConsoleCommands.Add("oc_spawn", "Spawn overworld chests.", OC_spawn);
			SHelper.ConsoleCommands.Add("oc_respawn", "Respawn overworld chests.", OC_respawn);
		}

		private void OC_remove(string command, string[] args)
		{
			if (!IsSaveLoaded())
				return;

			RemoveChests(true);
			SMonitor.Log("Overworld chests removed.", LogLevel.Info);
		}

		private void OC_spawn(string command, string[] args)
		{
			if (!IsSaveLoaded())
				return;

			SpawnChests(true);
			SMonitor.Log("Overworld chests spawned.", LogLevel.Info);
		}

		private void OC_respawn(string command, string[] args)
		{
			if (!IsSaveLoaded())
				return;

			RespawnChests(true);
			SMonitor.Log("Overworld chests respawned.", LogLevel.Info);
		}

		private static bool IsSaveLoaded()
		{
			if (!Context.IsWorldReady)
			{
				SMonitor.Log("You must load a save to run this command.", LogLevel.Error);
				return false;
			}
			return true;
		}
	}
}
