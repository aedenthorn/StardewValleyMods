using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Events;
using StardewValley.Extensions;

namespace MeteorDefence
{
	public partial class ModEntry
	{
		internal const int soundDelay = 1500;
		internal static bool playedDefenceSound;
		internal static bool playedDestructionOrImpactSound;
		internal static int numberOfStruck;
		internal static int numberOfStruckSound;
		internal static int numberOfMeteorites;
		internal static int meteoritesDestructionLimit;
		internal static int numberOfMeteroritesToDestroyAtOnce;
		internal static List<Vector2> strikeLocations;

		class SoundInTheNightEvent_setUp_Patch
		{
			public static bool Prefix(NetInt ___behavior, ref string ___soundName, ref string ___message, ref Vector2 ___targetLocation, ref bool __result)
			{
				if (!Config.ModEnabled || ___behavior.Value != SoundInTheNightEvent.meteorite)
					return true;

				Random random = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed);
				Farm farm = Game1.getFarm();

				strikeLocations = new List<Vector2>();
				playedDefenceSound = false;
				playedDestructionOrImpactSound = false;
				numberOfStruck = 0;
				numberOfStruckSound = 0;
				numberOfMeteroritesToDestroyAtOnce = 0;
				numberOfMeteorites = random.Next(Config.MinimumMeteorites, Config.MaximumMeteorites + 1);
				meteoritesDestructionLimit = Config.MeteoritesDestroyedPerObject >= 0 ? Game1.getFarm().Objects.Values.ToList().FindAll(o => o.ItemId.Equals($"{SModManifest.UniqueID}_SpaceLaser")).Count * Config.MeteoritesDestroyedPerObject : int.MaxValue;

				___soundName = $"{SModManifest.UniqueID}_flyingMeteorite";
				___message = Game1.content.LoadString("Strings\\Events:SoundInTheNight_Meteorite");
				__result = true;
				for (int i = 0; i < numberOfMeteorites; i++)
				{
					Vector2 targetLocation = new(random.Next(0, farm.map.RequireLayer("Back").LayerWidth), random.Next(0, farm.map.RequireLayer("Back").LayerHeight));
					bool isValidtargetLocation = true;

					if (!Config.StrikeAnywhere)
					{
						for (int j = (int)targetLocation.X; j <= targetLocation.X + 1f; j++)
						{
							for (int k = (int)targetLocation.Y; k <= targetLocation.Y + 1f; k++)
							{
								Vector2 tile = new(j, k);

								if (!farm.isTileOpenBesidesTerrainFeatures(tile) || !farm.isTileOpenBesidesTerrainFeatures(new Vector2(tile.X + 1f, tile.Y)) || !farm.isTileOpenBesidesTerrainFeatures(new Vector2(tile.X + 1f, tile.Y - 1f)) || !farm.isTileOpenBesidesTerrainFeatures(new Vector2(tile.X, tile.Y - 1f)) || farm.isWaterTile((int)tile.X, (int)tile.Y) || farm.isWaterTile((int)tile.X + 1, (int)tile.Y) || strikeLocations.Contains(tile) || strikeLocations.Contains(new Vector2(tile.X -1f, tile.Y - 1f)) || strikeLocations.Contains(new Vector2(tile.X + 1f, tile.Y - 1f)) || strikeLocations.Contains(new Vector2(tile.X - 1f, tile.Y + 1f)) || strikeLocations.Contains(new Vector2(tile.X + 1f, tile.Y + 1f)) || strikeLocations.Contains(new Vector2(tile.X, tile.Y + -1f)) || strikeLocations.Contains(new Vector2(tile.X, tile.Y + 1f)) || strikeLocations.Contains(new Vector2(tile.X - 1f, tile.Y)) || strikeLocations.Contains(new Vector2(tile.X + 1f, tile.Y)))
								{
									isValidtargetLocation = false;
								}
							}
						}
					}
					if (isValidtargetLocation)
					{
						if (strikeLocations.Count == 0)
						{
							___targetLocation = targetLocation;
						}
						strikeLocations.Add(targetLocation);
						__result = false;
					}
				}

				string action = !__result ? "Starting" : "Cancelling";

				SMonitor.Log($"{action} meteorite strike with {strikeLocations.Count}/{numberOfMeteorites} meteorites, defence systems can destroy up to {meteoritesDestructionLimit} meteorites");
				return false;
			}
		}

		class SoundInTheNightEvent_makeChangesToLocation_Patch
		{
			public static bool Prefix(NetInt ___behavior, ref Vector2 ___targetLocation)
			{
				if (!Config.ModEnabled || ___behavior.Value != SoundInTheNightEvent.meteorite || meteoritesDestructionLimit <= numberOfStruck)
				{
					SMonitor.Log($"dropping meteor {numberOfStruck} at {___targetLocation}");
					return true;
				}
				SMonitor.Log($"dropping debris for numberOfStruck meteor {numberOfStruck} at {___targetLocation}");
				Game1.createMultipleObjectDebris("386", (int)___targetLocation.X, (int)___targetLocation.Y, 6, Game1.MasterPlayer.UniqueMultiplayerID, Game1.getFarm());
				Game1.createMultipleObjectDebris("390", (int)___targetLocation.X, (int)___targetLocation.Y, 6, Game1.MasterPlayer.UniqueMultiplayerID, Game1.getFarm());
				Game1.createMultipleObjectDebris("535", (int)___targetLocation.X, (int)___targetLocation.Y, 2, Game1.MasterPlayer.UniqueMultiplayerID, Game1.getFarm());
				return false;
			}

			public static void Postfix(SoundInTheNightEvent __instance, NetInt ___behavior, ref Vector2 ___targetLocation)
			{
				if (!Config.ModEnabled || ___behavior.Value != SoundInTheNightEvent.meteorite || numberOfStruck >= strikeLocations.Count - 1)
					return;

				___targetLocation = strikeLocations[++numberOfStruck];
				__instance.makeChangesToLocation();
			}
		}

		class SoundInTheNightEvent_tickUpdate_Patch
		{
			public static void Prefix(NetInt ___behavior, ref float ___timer, ref bool ___playedSound, GameTime time)
			{
				if (!Config.ModEnabled || ___behavior.Value != SoundInTheNightEvent.meteorite)
					return;

				int elapsedMilliseconds = time.ElapsedGameTime.Milliseconds;

				if (numberOfMeteroritesToDestroyAtOnce <= 0)
				{
					int remain = strikeLocations.Count - numberOfStruckSound;
					int min = (int)Math.Ceiling((double)strikeLocations.Count / 4);
					int max = (int)Math.Ceiling((double)strikeLocations.Count / 2);

					numberOfMeteroritesToDestroyAtOnce = Math.Min(Math.Min(new Random().Next(min, max), remain), 5);
				}
				if (___timer + elapsedMilliseconds > 3500 && !playedDefenceSound && numberOfStruckSound < meteoritesDestructionLimit)
				{
					SMonitor.Log("Playing defence sound");
					for (int i = 0; i + numberOfStruckSound < meteoritesDestructionLimit && i < numberOfMeteroritesToDestroyAtOnce; i++)
					{
						DelayedAction.playSoundAfterDelay(Config.DefenceSound, i * soundDelay / numberOfMeteroritesToDestroyAtOnce);
					}
					playedDefenceSound = true;
				}
				if (___timer + elapsedMilliseconds > 5300 && !playedDestructionOrImpactSound)
				{
					int i;

					SMonitor.Log("Playing destruction sound");
					for (i = 0; i + numberOfStruckSound < meteoritesDestructionLimit && i < numberOfMeteroritesToDestroyAtOnce; i++)
					{
						DelayedAction.playSoundAfterDelay(Config.DestructionSound, i * soundDelay / numberOfMeteroritesToDestroyAtOnce);
					}
					SMonitor.Log("Playing impact sound");
					while (i < numberOfMeteroritesToDestroyAtOnce)
					{
						DelayedAction.playSoundAfterDelay(Config.ImpactSound, i * soundDelay / numberOfMeteroritesToDestroyAtOnce);
						i++;
					}
					playedDestructionOrImpactSound = true;
				}
				if (___timer + elapsedMilliseconds > 5300 && numberOfStruckSound + numberOfMeteroritesToDestroyAtOnce < strikeLocations.Count)
				{
					playedDefenceSound = false;
					playedDestructionOrImpactSound = false;
					___playedSound = false;
					___timer = 1000;
					numberOfStruckSound += numberOfMeteroritesToDestroyAtOnce;
					numberOfMeteroritesToDestroyAtOnce = 0;
				}
			}
		}
	}
}
