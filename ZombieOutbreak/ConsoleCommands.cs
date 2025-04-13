using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;

namespace ZombieOutbreak
{
	public partial class ModEntry
	{
		internal void RegisterConsoleCommands()
		{
			SHelper.ConsoleCommands.Add("zo_infect", "Infect an NPC or a player with zombie virus.\n\nUsage: zo_infect [npcName | playerName | playerId = \"default\"]\nThe \"default\" keyword matches the current player.\n- npcName: The internal (English) name of the npc.\n- playerName: The name of the player.\n- playerId: The unique multiplayer ID of the player.", ZO_Infect);
		}

		private void ZO_Infect(string command, string[] args)
		{
			if (!IsModEnabled() || !IsSaveLoaded())
				return;

			if (args.Any())
			{
				if (Game1.characterData is not null && Game1.characterData.ContainsKey(args[0]))
				{
					TryAddZombieNPC(args[0]);
					return;
				}

				IEnumerable<Farmer> farmers = Game1.getAllFarmers();

				if (farmers is not null)
				{
					bool isInt = long.TryParse(args[0], out long value);

					foreach (Farmer farmer in farmers)
					{
						if (farmer.Name == args[0] || (isInt && farmer.UniqueMultiplayerID == value))
						{
							TryAddZombieFarmer(farmer.Name, farmer.UniqueMultiplayerID);
							return;
						}
					}
				}
				SMonitor.Log($"No npc or player found with name or id '{args[0]}'.", LogLevel.Error);
			}
			else
			{
				TryAddZombieFarmer(Game1.player.Name, Game1.player.UniqueMultiplayerID);
			}
		}

		private static bool IsModEnabled()
		{
			if (!Config.ModEnabled)
			{
				SMonitor.Log("You must enable the mod to run this command.", LogLevel.Error);
				return false;
			}
			return true;
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

		private static void TryAddZombieNPC(string name)
		{
			if (curedNPCs.Contains(name))
			{
				SMonitor.Log($"Npc '{name}' has already been cured today and is therefore immune for the rest of the day.", LogLevel.Info);
			}
			else if (zombieNPCTextures.ContainsKey(name))
			{
				SMonitor.Log($"Npc '{name}' is already infected.", LogLevel.Info);
			}
			else
			{
				AddZombieNPC(name);
				SMonitor.Log($"Npc '{name}' infected.", LogLevel.Info);
			}
		}

		private static void TryAddZombieFarmer(string name, long id)
		{
			if (curedFarmers.Contains(id))
			{
				SMonitor.Log($"Player '{name}' (id: {id}) has already been cured today and is therefore immune for the rest of the day.", LogLevel.Info);
			}
			else if (zombieFarmerTextures.ContainsKey(id))
			{
				SMonitor.Log($"Player '{name}' (id: {id}) is already infected.", LogLevel.Info);
			}
			else
			{
				AddZombieFarmer(id);
				SMonitor.Log($"Player '{name}' (id: {id}) infected.", LogLevel.Info);
			}
		}
	}
}
