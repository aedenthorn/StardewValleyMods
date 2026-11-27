using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Locations;
using StardewValley.Monsters;
using Object = StardewValley.Object;

namespace BossCreatures
{
	public partial class ModEntry
	{
		public static void GenerateBossHealthBarTexture(int Health, int MaxHealth)
		{
			healthBarTexture = new Texture2D(Game1.graphics.GraphicsDevice, (int)Utility.ModifyCoordinateForUIScale(Game1.viewport.Width * 0.75f), 30);

			Color[] data = new Color[healthBarTexture.Width * healthBarTexture.Height];

			healthBarTexture.GetData(data);
			for (int i = 0; i < data.Length; i++)
			{
				if (i <= healthBarTexture.Width || i % healthBarTexture.Width == healthBarTexture.Width - 1)
				{
					data[i] = new Color(1f, 0.5f, 0.5f);
				}
				else if (data.Length - i < healthBarTexture.Width || i % healthBarTexture.Width == 0)
				{
					data[i] = new Color(0.5f, 0, 0);
				}
				else if (i % healthBarTexture.Width / (float)healthBarTexture.Width < (float)Health / MaxHealth)
				{
					data[i] = Color.Red;
				}
				else
				{
					data[i] = Color.Black;
				}
			}
			healthBarTexture.SetData(data);
		}

		private static void LoadBossLootList()
		{
			BossLootList = SHelper.Data.ReadJsonFile<LootList>("assets/boss_loot.json") ?? new LootList();
			if (BossLootList.loot.Count == 0)
			{
				SMonitor.Log("No boss loot!", LogLevel.Warn);
			}
		}

		public static string GetBossTexture(Type type)
		{
			string texturePath = type.Name switch
			{
				nameof(BugBoss) => "Characters\\Monsters\\Armored Bug",
				nameof(GhostBoss) => "Characters\\Monsters\\Ghost",
				nameof(SerpentBoss) => "Characters\\Monsters\\Serpent",
				nameof(SkeletonBoss) => "Characters\\Monsters\\Skeleton",
				nameof(SkullBoss) => "Characters\\Monsters\\Haunted Skull",
				nameof(SquidKidBoss) => "Characters\\Monsters\\Squid Kid",
				nameof(SlimeBoss) => "Characters\\Monsters\\Big Slime",
				_ => $"Characters\\Monsters\\{type.Name}"
			};

			if (Config.UseAlternateTextures)
			{
				try
				{
					Texture2D spriteTexture = SHelper.GameContent.Load<Texture2D>($"Characters/Monsters/{type.Name}");

					if (spriteTexture is not null)
					{
						texturePath = $"Characters\\Monsters\\{type.Name}";
					}
				}
				catch
				{
					SMonitor.Log($"texture not found: Characters\\Monsters\\{type.Name}", LogLevel.Debug);
				}
			}
			return texturePath;
		}

		private static bool IsBoss(NPC npc)
		{
			return npc is BugBoss or GhostBoss or SerpentBoss or SkeletonBoss or SkullBoss or SquidKidBoss or SlimeBoss;
		}

		public static bool WithinAnyPlayerThreshold(NPC npc, int threshold)
		{
			if (npc.currentLocation is not null && npc.currentLocation.farmers.Any())
			{
				foreach (Farmer farmer in npc.currentLocation.farmers)
				{
					if (Math.Abs(npc.Tile.X - farmer.Tile.X) <= threshold && Math.Abs(npc.Tile.Y - farmer.Tile.Y) <= threshold)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool IsLessThanHalfHealth(Monster monster)
		{
			return monster.Health < monster.MaxHealth / 2;
		}

		public static bool IsLessThanQuarterHealth(Monster monster)
		{
			return monster.Health < monster.MaxHealth / 4;
		}

		private static void MakeVillagersPanic(GameLocation location)
		{
			if (location is not MineShaft)
			{
				foreach (NPC character in location.characters)
				{
					if (character.IsVillager && !character.isCharging)
					{
						character.speed = 4;
						character.isCharging = true;
						character.blockedInterval = 0;
					}
				}
			}
		}

		private static void CalmDownVillagers(GameLocation location)
		{
			if (location is not MineShaft)
			{
				foreach (NPC character in location.characters)
				{
					if (character.IsVillager && character.isCharging)
					{
						character.speed = 2;
						character.isCharging = false;
						character.blockedInterval = 0;
					}
				}
			}
		}

		internal static void RevertMusic(GameLocation location)
		{
			Game1.changeMusicTrack("none", true, MusicContext.Default);
			location.checkForMusic(new GameTime());
		}

		public static Monster GetBoss(GameLocation location)
		{
			foreach (NPC npc in location.characters)
			{
				if (IsBoss(npc))
				{
					return npc as Monster;
				}
			}
			return null;
		}

		public static bool HasBoss(GameLocation location)
		{
			return GetBoss(location) is not null;
		}

		private static void TryAddBoss(GameLocation location)
		{
			Monster boss = GetBoss(location);

			if (boss is not null && boss.Health > 0)
			{
				SetupBossBattle(location, false);
			}
			else
			{
				if (!BossSpawnedLocations.Contains(location.Name))
				{
					BossSpawnedLocations.Add(location.Name);
					if (location is MineShaft mineShaft && mineShaft.mustKillAllMonstersToAdvance() && Game1.random.Next(0, 100) < Config.PercentChanceOfBossInMonsterArea)
					{
						SpawnRandomBoss(location);
					}
					else if (location is Farm && Game1.random.Next(0, 100) < Config.PercentChanceOfBossInFarm)
					{
						SpawnRandomBoss(location);
					}
					else if (location is Town && Game1.random.Next(0, 100) < Config.PercentChanceOfBossInTown)
					{
						SpawnRandomBoss(location);
					}
					else if (location is Forest && Game1.random.Next(0, 100) < Config.PercentChanceOfBossInForest)
					{
						SpawnRandomBoss(location);
					}
					else if (location is Mountain && Game1.random.Next(0, 100) < Config.PercentChanceOfBossInMountain)
					{
						SpawnRandomBoss(location);
					}
					else if (location is Desert && Game1.random.Next(0, 100) < Config.PercentChanceOfBossInDesert)
					{
						SpawnRandomBoss(location);
					}
					else if (location.Name == "CrimsonBadlands" && Game1.random.Next(0, 100) < Config.PercentChanceOfBossInCrimsonBadlands)
					{
						SpawnRandomBoss(location);
					}
				}
			}
		}

		private static void SpawnRandomBoss(GameLocation location)
		{
			Vector2 spawnPosition = GetSpawnLocation(location);

			if (spawnPosition != Vector2.Zero)
			{
				float difficulty = Config.BaseUndergroundDifficulty;
				int random = Game1.random.Next(0, (int)Math.Round(Config.WeightSkullBossChance * 100 + Config.WeightSerpentBossChance * 100 + Config.WeightBugBossChance * 100 + Config.WeightGhostBossChance * 100 + Config.WeightSkeletonBossChance * 100 + Config.WeightSquidBossChance * 100 + Config.WeightSlimeBossChance * 100));

				if (location is MineShaft)
				{
					difficulty *= (location as MineShaft).mineLevel / 100f;
					SMonitor.Log("boss difficulty: " + difficulty, LogLevel.Debug);
				}
				else
				{
					difficulty = Game1.random.Next((int)Math.Round(Config.MinOverlandDifficulty * 1000), (int)Math.Round(Config.MaxOverlandDifficulty * 1000)+1) / 1000f;
					SMonitor.Log("boss difficulty: " + difficulty, LogLevel.Debug);
				}
				if (random < Config.WeightSkullBossChance * 100)
				{
					SkullBoss skullBoss = new(spawnPosition, difficulty)
					{
						currentLocation = location,
					};

					location.characters.Add(skullBoss);
				}
				else if (random < Config.WeightSkullBossChance * 100 + Config.WeightSerpentBossChance * 100)
				{
					SerpentBoss serpentBoss = new(spawnPosition, difficulty)
					{
						currentLocation = location,
					};

					location.characters.Add(serpentBoss);
				}
				else if (random < Config.WeightSkullBossChance * 100 + Config.WeightSerpentBossChance * 100 + Config.WeightBugBossChance * 100)
				{
					BugBoss bugBoss = new(spawnPosition, difficulty)
					{
						currentLocation = location,
					};

					location.characters.Add(bugBoss);
				}
				else if (random < Config.WeightSkullBossChance * 100 + Config.WeightSerpentBossChance * 100 + Config.WeightBugBossChance * 100 + Config.WeightGhostBossChance * 100)
				{
					GhostBoss ghostBoss = new(spawnPosition, difficulty)
					{
						currentLocation = location,
					};

					location.characters.Add(ghostBoss);
				}
				else if (random < Config.WeightSkullBossChance * 100 + Config.WeightSerpentBossChance * 100 + Config.WeightBugBossChance * 100 + Config.WeightGhostBossChance * 100 + Config.WeightSkeletonBossChance * 100)
				{
					SkeletonBoss skeletonBoss = new(spawnPosition, difficulty)
					{
						currentLocation = location,
					};

					location.characters.Add(skeletonBoss);
				}
				else if (random < Config.WeightSkullBossChance * 100 + Config.WeightSerpentBossChance * 100 + Config.WeightBugBossChance * 100 + Config.WeightGhostBossChance * 100 + Config.WeightSkeletonBossChance * 100 + Config.WeightSquidBossChance * 100)
				{
					SquidKidBoss squidKidBoss = new(spawnPosition, difficulty)
					{
						currentLocation = location,
					};

					location.characters.Add(squidKidBoss);
				}
				else
				{
					SlimeBoss slimeBoss = new(spawnPosition, difficulty)
					{
						currentLocation = location,
					};

					location.characters.Add(slimeBoss);
				}
				SetupBossBattle(location);
			}
			else
			{
				SMonitor.Log("no spawn location for boss!", LogLevel.Debug);
			}

		}

		private static void SetupBossBattle(GameLocation location, bool showWarningMessage = true)
		{
			SetBattleWeather(location);
			Game1.changeMusicTrack(Config.BattleMusic, false, MusicContext.Default);
			SHelper.Events.Display.RenderedHud += OnRenderedHud;
			if (showWarningMessage)
			{
				Game1.showGlobalMessage(SHelper.Translation.Get("boss-warning"));
			}
		}

		private static Vector2 GetSpawnLocation(GameLocation location)
		{
			List<Vector2> tiles = new();

			if (location is MineShaft)
			{
				for (int x = 0; x < location.map.Layers[0].LayerWidth; x++)
				{
					for (int y = 0; y < location.map.Layers[0].LayerHeight; y++)
					{
						Vector2 tileLocation = new(x, y);

						if (location.isTileOnMap(tileLocation) && (location as MineShaft).isTileClearForMineObjects(tileLocation))
						{
							tiles.Add(tileLocation);
						}
					}
				}
			}
			else
			{
				for (int x = (int)Math.Round(location.map.Layers[0].LayerWidth *0.1f); x < (int)Math.Round(location.map.Layers[0].LayerWidth * 0.9f); x++)
				{
					for (int y = (int)Math.Round(location.map.Layers[0].LayerHeight * 0.1f); y < (int)Math.Round(location.map.Layers[0].LayerHeight * 0.9f); y++)
					{
						Vector2 tileLocation = new(x, y);

						if (location.isTileOnMap(tileLocation) && location.CanSpawnCharacterHere(tileLocation) && !location.isWaterTile(x, y))
						{
							tiles.Add(tileLocation);
						}
					}
				}
			}
			if (tiles.Count == 0)
			{
				return Vector2.Zero;
			}
			else
			{
				List<Vector2> perfectTiles = new();

				foreach (Vector2 tile in tiles)
				{
					if (tiles.Contains(new Vector2(tile.X - 1, tile.Y - 1))
						&& tiles.Contains(new Vector2(tile.X, tile.Y - 1))
						&& tiles.Contains(new Vector2(tile.X + 1, tile.Y - 1))
						&& tiles.Contains(new Vector2(tile.X - 1, tile.Y))
						&& tiles.Contains(new Vector2(tile.X + 1, tile.Y))
						&& tiles.Contains(new Vector2(tile.X + 1, tile.Y + 1))
						&& tiles.Contains(new Vector2(tile.X, tile.Y + 1))
						&& tiles.Contains(new Vector2(tile.X + 1, tile.Y + 1)))
					{
						perfectTiles.Add(tile);
					}
				}
				if (perfectTiles.Count == 0)
				{
					return tiles[Game1.random.Next(0, tiles.Count)] * 64f;
				}
				else
				{
					List<Vector2> ultraPerfectTiles = new();

					foreach (Vector2 tile in perfectTiles)
					{
						if (perfectTiles.Contains(new Vector2(tile.X - 1, tile.Y - 1))
							&& perfectTiles.Contains(new Vector2(tile.X, tile.Y - 1))
							&& perfectTiles.Contains(new Vector2(tile.X + 1, tile.Y - 1))
							&& perfectTiles.Contains(new Vector2(tile.X - 1, tile.Y))
							&& perfectTiles.Contains(new Vector2(tile.X + 1, tile.Y))
							&& perfectTiles.Contains(new Vector2(tile.X + 1, tile.Y + 1))
							&& perfectTiles.Contains(new Vector2(tile.X, tile.Y + 1))
							&& perfectTiles.Contains(new Vector2(tile.X + 1, tile.Y + 1)))
						{
							ultraPerfectTiles.Add(tile);
						}
					}
					if (ultraPerfectTiles.Count == 0)
					{
						return perfectTiles[Game1.random.Next(0, perfectTiles.Count)] * 64f;
					}
					else
					{
						return ultraPerfectTiles[Game1.random.Next(0, ultraPerfectTiles.Count)] * 64f;
					}
				}
			}
		}

		private static void SetDefaultWeather(GameLocation location)
		{
			if (!Config.ModEnabled || !Config.BattleWeather)
				return;

			if (Game1.getOnlineFarmers().All(farmer => farmer.currentLocation == location || !HasBoss(farmer.currentLocation)))
			{
				if (string.IsNullOrEmpty(defaultWeather) || string.IsNullOrEmpty(islandWeather))
				{
					defaultWeather = Game1.netWorldState.Value.GetWeatherForLocation("Default").Weather;
					islandWeather = Game1.netWorldState.Value.GetWeatherForLocation("Island").Weather;
				}
				if (!location.NameOrUniqueName.StartsWith("Island"))
				{
					Game1.isRaining = defaultWeather.Equals("Rain") || defaultWeather.Equals("Storm");
					Game1.isGreenRain = defaultWeather.Equals("GreenRain");
					Game1.isSnowing = defaultWeather.Equals("Snow");
					Game1.isLightning = defaultWeather.Equals("Storm");
				}
				else
				{
					Game1.isRaining = islandWeather.Equals("Rain") || islandWeather.Equals("Storm");
					Game1.isLightning = islandWeather.Equals("Storm");
				}
				location.GetWeather().isRaining.Value = Game1.isRaining;
				location.GetWeather().isGreenRain.Value = Game1.isGreenRain;
				location.GetWeather().isSnowing.Value = Game1.isSnowing;
				location.GetWeather().isLightning.Value = Game1.isLightning;
				Game1.updateWeatherIcon();
			}
		}

		private static void SetBattleWeather(GameLocation location)
		{
			if (!Config.ModEnabled || !Config.BattleWeather || !location.IsOutdoors)
				return;

			Game1.isRaining = !Game1.isGreenRain;
			Game1.isSnowing = false;
			Game1.isLightning = true;
			location.GetWeather().isRaining.Value = Game1.isRaining;
			location.GetWeather().isSnowing.Value = Game1.isSnowing;
			location.GetWeather().isLightning.Value = Game1.isLightning;
			Game1.updateWeatherIcon();
		}

		public static void HandleBossDefeat(GameLocation currentLocation, Monster monster, float difficulty)
		{
			Rectangle bossBoundingBox = monster.GetBoundingBox();

			SpawnBossLoot(currentLocation, bossBoundingBox.Center.X, bossBoundingBox.Center.Y, difficulty);
			CalmDownVillagers(currentLocation);
			Game1.playSound(Config.VictorySound);
			RevertMusic(currentLocation);
			DelayedAction.screenFlashAfterDelay(1f, 0);
			SetDefaultWeather(currentLocation);
			SHelper.Events.Display.RenderedHud -= OnRenderedHud;
		}

		private static void SpawnBossLoot(GameLocation location, float x, float y, float difficulty)
		{
			foreach (string loot in BossLootList.loot)
			{
				string[] loota = loot.Split('/');

				if (!int.TryParse(loota[0], out int objectId) || (objectId >= 0 && !Game1.objectData.TryGetValue(loota[0], out _)))
				{
					SMonitor.Log($"loot object {loota[0]} is invalid", LogLevel.Error);
					continue;
				}
				if (!double.TryParse(loota[1], out double chance))
				{
					SMonitor.Log($"loot chance {loota[1]} is invalid", LogLevel.Error);
					continue;
				}
				while (chance > 1 || (chance > 0 && Game1.random.NextDouble() < chance))
				{
					if (objectId < 0)
					{
						Game1.createDebris(Math.Abs(objectId), (int)x, (int)y, (int)Math.Round(Game1.random.Next(10, 40) * difficulty), location);
					}
					else
					{
						Game1.createItemDebris(new Object(loota[0], 1), new Vector2(x, y), Game1.random.Next(4), location);
					}
					chance -= 1;
				}
			}
		}

		public static Vector2 VectorFromDegrees(float degrees)
		{
			float radians = MathHelper.ToRadians(degrees);

			return new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
		}
	}
}
