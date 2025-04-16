using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;

namespace PersonalTravelingCart
{
	public partial class ModEntry
	{
		internal void RegisterConsoleCommands()
		{
			SHelper.ConsoleCommands.Add("ptc_warp_horses_to_stables", "Warp horses to their stables.", PTC_warp_horses_to_stables);
			SHelper.ConsoleCommands.Add("ptc_hitch_carts_to_horses", "Hitch carts to their horses.", PTC_hitch_carts_to_horses);
		}

		private void PTC_warp_horses_to_stables(string command, string[] args)
		{
			if (!Context.IsWorldReady)
			{
				SMonitor.Log("You must load a save to use this command.", LogLevel.Error);
				return;
			}
			if (!Context.IsMainPlayer)
			{
				SMonitor.Log("You must be the main player to use this command.", LogLevel.Error);
				return;
			}
			Utility.ForEachBuilding(building =>
			{
				if (building is Stable stable)
				{
					Horse horse = Utility.findHorse(stable.HorseId);
					Point defaultHorseTile = stable.GetDefaultHorseTile();

					if (horse is not null)
					{
						Game1.warpCharacter(horse, stable.parentLocationName.Value, defaultHorseTile);
					}
				}
				return true;
			});
			SMonitor.Log("Horses have been warped to their stables.", LogLevel.Info);
		}

		private void PTC_hitch_carts_to_horses(string command, string[] args)
		{
			if (!Context.IsWorldReady)
			{
				SMonitor.Log("You must load a save to use this command.", LogLevel.Error);
				return;
			}
			if (!Context.IsMainPlayer)
			{
				SMonitor.Log("You must be the main player to use this command.", LogLevel.Error);
				return;
			}

			Farm farm = Game1.getFarm();

			Utility.ForEachCharacter(character =>
			{
				if (character is Horse horse)
				{
					horse.modData.Remove(parkedKey);
				}
				return true;
			});
			foreach (string key in farm.modData.Keys)
			{
				if (key.StartsWith($"{parkedListKey}/"))
				{
					farm.modData.Remove(key);
				}
			}
			SMonitor.Log("Carts have been hitched to their horses.", LogLevel.Info);
		}
	}
}
