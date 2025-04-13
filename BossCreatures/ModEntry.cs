using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using Object = StardewValley.Object;

namespace BossCreatures
{
	public class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

		private static int toggleSprite = 0;
		private static readonly List<string> CheckedBosses = new();
		private static readonly string defaultMusic = "none";
		private static Texture2D healthBarTexture;
		private static readonly Dictionary<Type,string> BossTypes = new() {
			{ typeof(BugBoss), "Armored Bug"},
			{ typeof(GhostBoss), "Ghost"},
			{ typeof(SerpentBoss), "Serpent"},
			{ typeof(SkeletonBoss), "Skeleton"},
			{ typeof(SkullBoss), "Haunted Skull"},
			{ typeof(SquidKidBoss), "Squid Kid"},
			{ typeof(SlimeBoss), "Big Slime"},
		};
		private static LootList BossLootList;
		private static int lastBossHealth;
		private static string defaultWeather;
		private static string islandWeather;

		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.Player.Warped += Warped;
			helper.Events.GameLoop.GameLaunched += OnGameLaunched;
			helper.Events.GameLoop.DayStarted += OnDayStarted;
			helper.Events.GameLoop.DayEnding += OnDayEnding;
			helper.Events.GameLoop.UpdateTicked += UpdateTicked;
			helper.Events.Display.WindowResized += WindowResized;

			BossLootList = Helper.Data.ReadJsonFile<LootList>("assets/boss_loot.json") ?? new LootList();
			if (BossLootList.loot.Count == 0)
			{
				SMonitor.Log("No boss loot!", LogLevel.Warn);
			}
		}

		private void OnDayEnding(object sender, DayEndingEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			defaultWeather = null;
			islandWeather = null;
			foreach (GameLocation location in Game1.locations)
			{
				for (int i = 0; i < location.characters.Count; i++)
				{
					if (BossTypes.ContainsKey(location.characters[i].GetType()) || location.characters[i] is ToughFly || location.characters[i] is ToughGhost)
					{
						location.characters.RemoveAt(i);
					}

				}
			}
			foreach (GameLocation location in Game1._locationLookup.Values)
			{
				for (int i = 0; i < location.characters.Count; i++)
				{
					if (BossTypes.ContainsKey(location.characters[i].GetType()) || location.characters[i] is ToughFly || location.characters[i] is ToughGhost)
					{
						location.characters.RemoveAt(i);
					}
				}
			}
		}

		private void OnDayStarted(object sender, DayStartedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			CheckedBosses.Clear();
		}

		private void Warped(object sender, WarpedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			SMonitor.Log("Entered location: " + e.NewLocation.Name);
			foreach (GameLocation location in Game1.locations)
			{
				for (int i = 0; i < location.characters.Count; i++)
				{
					if (location.characters[i] is ToughFly || location.characters[i] is ToughGhost)
					{
						location.characters.RemoveAt(i);
					}

				}
			}
			if (BossHere(e.OldLocation) != null && BossHere(e.NewLocation) == null)
			{
				SetDefaultWeather(e.NewLocation);
				RevertMusic(e.NewLocation);
			}
			if (!Game1.eventUp)
			{
				TryAddBoss(e.NewLocation);
			}
			Game1.updateWeatherIcon();
		}

		public static string GetBossTexture(Type type)
		{
			string texturePath = $"Characters\\Monsters\\{BossTypes[type]}";

			if (Config.UseAlternateTextures)
			{
				try
				{
					Texture2D spriteTexture = SHelper.GameContent.Load<Texture2D>($"Characters/Monsters/{type.Name}");

					if (spriteTexture != null)
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

		public static void BossDeath(GameLocation currentLocation, Monster monster, float difficulty)
		{
			SHelper.Events.Display.RenderedHud -= OnRenderedHud;

			Rectangle monsterBox = monster.GetBoundingBox();

			SpawnBossLoot(currentLocation, monsterBox.Center.X, monsterBox.Center.Y, difficulty);
			if (currentLocation is not MineShaft)
			{
				foreach (NPC character in currentLocation.characters)
				{
					if (character.IsVillager && character.isCharging)
					{
						character.speed = 2;
						character.isCharging = false;
						character.blockedInterval = 0;
					}
				}
			}
			Game1.playSound(Config.VictorySound);
			RevertMusic(currentLocation);
			DelayedAction.screenFlashAfterDelay(1f, 0);
			SetDefaultWeather(currentLocation);
		}

		public static void OnRenderedHud(object sender, RenderedHudEventArgs e)
		{
			Monster boss = BossHere(Game1.player.currentLocation);

			if (boss == null)
			{
				SHelper.Events.Display.RenderedHud -= OnRenderedHud;
				return;
			}
			if (boss.Health != lastBossHealth)
			{
				lastBossHealth = boss.Health;
				MakeBossHealthBar(boss.Health, boss.MaxHealth);
			}
			e.SpriteBatch.Draw(healthBarTexture, new Vector2((int)Math.Round(Game1.viewport.Width * 0.13f), 100), Color.White);

			Vector2 bossPos = boss.Position;

			if (!Utility.isOnScreen(bossPos, 0))
			{
				int x = (int)Math.Max(10, Math.Min(Game1.viewport.X + Game1.viewport.Width - 90, bossPos.X) - Game1.viewport.X);
				int y = (int)Math.Max(10, Math.Min(Game1.viewport.Y + Game1.viewport.Height - 90, bossPos.Y) - Game1.viewport.Y);

				if (toggleSprite < 20)
				{
					Texture2D texture = SHelper.GameContent.Load<Texture2D>("Characters/Monsters/Haunted Skull");
					ClickableTextureComponent bossIcon = new(new Rectangle(x, y, 80, 80), texture, new Rectangle(toggleSprite > 10 ? 16 : 0, 32, 16, 16), 5f, false);
					bossIcon.draw(Game1.spriteBatch);
				}
				toggleSprite++;
				toggleSprite %= 30;
			}
		}

		public static void MakeBossHealthBar(int Health, int MaxHealth)
		{
			healthBarTexture = new Texture2D(Game1.graphics.GraphicsDevice, (int)Math.Round(Game1.viewport.Width * 0.75f), 30);

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
				else if (i % healthBarTexture.Width / (float)healthBarTexture.Width < (float)Health / (float)MaxHealth)
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

		internal static void RevertMusic(GameLocation location)
		{
			Game1.changeMusicTrack(defaultMusic, true, MusicContext.Default);
			location.checkForMusic(new GameTime());
		}

		public static Monster BossHere(GameLocation location)
		{
			using List<NPC>.Enumerator enumerator = location.characters.GetEnumerator();

			while (enumerator.MoveNext())
			{
				NPC j = enumerator.Current;

				if (BossTypes.ContainsKey(j.GetType()))
				{
					return (Monster)j;
				}
			}
			return null;
		}

		private static void TryAddBoss(GameLocation location)
		{
			Monster boss = BossHere(location);

			if (boss != null && boss.Health > 0)
			{
				SetBattleWeather(location);
				Game1.changeMusicTrack(Config.BattleMusic, false, MusicContext.Default);
				SHelper.Events.Display.RenderedHud += OnRenderedHud;
				return;
			}
			if (CheckedBosses.Contains(location.Name))
			{
				return;
			}
			CheckedBosses.Add(location.Name);
			if ((location is MineShaft) && (location as MineShaft).mustKillAllMonstersToAdvance() && Game1.random.Next(0, 100) < Config.PercentChanceOfBossInMonsterArea)
			{
				SpawnRandomBoss(location);
			}
			else if ((location is Farm) && Game1.random.Next(0, 100) < Config.PercentChanceOfBossInFarm)
			{
				SpawnRandomBoss(location);
			}
			else if ((location is Town) && Game1.random.Next(0, 100) < Config.PercentChanceOfBossInTown)
			{
				SpawnRandomBoss(location);
			}
			else if ((location is Forest) && Game1.random.Next(0, 100) < Config.PercentChanceOfBossInForest)
			{
				SpawnRandomBoss(location);
			}
			else if ((location is Mountain) && Game1.random.Next(0, 100) < Config.PercentChanceOfBossInMountain)
			{
				SpawnRandomBoss(location);
			}
			else if ((location is Desert) && Game1.random.Next(0, 100) < Config.PercentChanceOfBossInDesert)
			{
				SpawnRandomBoss(location);
			}
			else if ((location.Name == "CrimsonBadlands") && Game1.random.Next(0, 100) < Config.PercentChanceOfBossInCrimsonBadlands)
			{
				SpawnRandomBoss(location);
			}
		}

		private static void SpawnRandomBoss(GameLocation location)
		{
			Vector2 spawnPos = GetSpawnLocation(location);

			if (spawnPos == Vector2.Zero)
			{
				SMonitor.Log("no spawn location for boss!", LogLevel.Debug);
				return;
			}

			float difficulty = Config.BaseUndergroundDifficulty;

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

			int random = Game1.random.Next(0, (int)Math.Round(Config.WeightSkullBossChance * 100 + Config.WeightSerpentBossChance * 100 + Config.WeightBugBossChance * 100 + Config.WeightGhostBossChance * 100 + Config.WeightSkeletonBossChance * 100 + Config.WeightSquidBossChance * 100 + Config.WeightSlimeBossChance * 100));

			if (random < Config.WeightSkullBossChance * 100)
			{
				SkullBoss k = new(spawnPos, difficulty)
				{
					currentLocation = location,
				};

				location.characters.Add(k);
			}
			else if (random < Config.WeightSkullBossChance * 100 + Config.WeightSerpentBossChance * 100)
			{
				SerpentBoss s = new(spawnPos, difficulty)
				{
					currentLocation = location,
				};

				location.characters.Add(s);
			}
			else if (random < Config.WeightSkullBossChance * 100 + Config.WeightSerpentBossChance * 100 + Config.WeightBugBossChance * 100)
			{
				BugBoss b = new(spawnPos, difficulty)
				{
					currentLocation = location,
				};

				location.characters.Add(b);
			}
			else if (random < Config.WeightSkullBossChance * 100 + Config.WeightSerpentBossChance * 100 + Config.WeightBugBossChance * 100 + Config.WeightGhostBossChance * 100)
			{
				GhostBoss g = new(spawnPos, difficulty)
				{
					currentLocation = location,
				};

				location.characters.Add(g);
			}
			else if (random < Config.WeightSkullBossChance * 100 + Config.WeightSerpentBossChance * 100 + Config.WeightBugBossChance * 100 + Config.WeightGhostBossChance * 100 + Config.WeightSkeletonBossChance * 100)
			{
				SkeletonBoss sk = new(spawnPos, difficulty)
				{
					currentLocation = location,
				};

				location.characters.Add(sk);
			}
			else if (random < Config.WeightSkullBossChance * 100 + Config.WeightSerpentBossChance * 100 + Config.WeightBugBossChance * 100 + Config.WeightGhostBossChance * 100 + Config.WeightSkeletonBossChance * 100 + Config.WeightSquidBossChance * 100)
			{
				SquidKidBoss sq = new(spawnPos, difficulty)
				{
					currentLocation = location,
				};

				location.characters.Add(sq);
			}
			else
			{
				SlimeBoss sl = new(spawnPos, difficulty)
				{
					currentLocation = location,
				};

				location.characters.Add(sl);
			}
			SetBattleWeather(location);
			Game1.showGlobalMessage(SHelper.Translation.Get("boss-warning"));
			Game1.changeMusicTrack(Config.BattleMusic, false, MusicContext.Default);
			SHelper.Events.Display.RenderedHud += OnRenderedHud;
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

		public static void SpawnBossLoot(GameLocation location, float x, float y, float difficulty)
		{
			foreach (string loot in BossLootList.loot)
			{
				string[] loota = loot.Split('/');

				if (!int.TryParse(loota[0], out int objectId) || (objectId >= 0 && !Game1.objectData.TryGetValue(loota[0], out _)))
				{
					SMonitor.Log($"loot object {loota[0]} is invalid", LogLevel.Error);
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

		public static Vector2 RotateVector2d(Vector2 inV, float degrees)
		{
			float rads = (float)Math.PI / 180 * degrees;
			Vector2 result = new()
			{
				X = (float)(inV.X * Math.Cos(rads) - inV.Y * Math.Sin(rads)),
				Y = (float)(inV.X * Math.Sin(rads) + inV.Y * Math.Cos(rads))
			};

			return result;
		}

		public static Vector2 RotateVector(Vector2 v, float degrees)
		{
			double radians = Math.PI / 180 * degrees;
			double sin = Math.Sin(radians);
			double cos = Math.Cos(radians);
			float tx = v.X;
			float ty = v.Y;

			return new Vector2((float)cos * tx - (float)sin * ty, (float)sin * tx + (float)cos * ty);
		}

		public static Vector2 VectorFromDegree(int degrees)
		{
			double radians = Math.PI / 180 * degrees;

			return new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
		}

		public static bool IsLessThanHalfHealth(Monster m)
		{
			return m.Health < m.MaxHealth / 2;
		}

		private void UpdateTicked(object sender, UpdateTickedEventArgs e)
		{
			if (!Config.ModEnabled || !Context.IsWorldReady || Game1.player.currentLocation is MineShaft)
				return;

			Monster boss = BossHere(Game1.player.currentLocation);

			if (boss != null)
			{
				foreach (NPC character in Game1.player.currentLocation.characters)
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

		private void WindowResized(object sender, WindowResizedEventArgs e)
		{
			if (!Config.ModEnabled || !Context.IsWorldReady)
				return;

			Monster boss = BossHere(Game1.player.currentLocation);

			if (boss != null)
			{
				MakeBossHealthBar(boss.Health, boss.MaxHealth);
				return;
			}
		}

		private static void SetDefaultWeather(GameLocation location)
		{
			if (!Config.ModEnabled || !Config.BattleWeather)
				return;

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

		private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
		{
			// get Generic Mod Config Menu's API (if it's installed)
			var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			if (configMenu is null)
				return;

			// register mod
			configMenu.Register(
				mod: ModManifest,
				reset: () => Config = new ModConfig(),
				save: () => Helper.WriteConfig(Config)
			);

			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
				getValue: () => Config.ModEnabled,
				setValue: value => {
					if (Context.IsWorldReady && value == false)
					{
						if (BossHere(Game1.player.currentLocation) != null)
						{
							SetDefaultWeather(Game1.player.currentLocation);
							RevertMusic(Game1.player.currentLocation);
						}
						OnDayEnding(null, null);
						OnDayStarted(null, null);
					}
					Config.ModEnabled = value;
				}
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.BattleWeather.Name"),
				getValue: () => Config.BattleWeather,
				setValue: value => {
					if (Context.IsWorldReady && BossHere(Game1.player.currentLocation) != null)
					{
						if (value == false)
						{
							SetDefaultWeather(Game1.player.currentLocation);
							Config.BattleWeather = value;
							return;
						}
						else
						{
							Config.BattleWeather = value;
							SetBattleWeather(Game1.player.currentLocation);
							return;
						}
					}
					Config.BattleWeather = value;
				}
			);
			configMenu.AddPageLink(
				mod: ModManifest,
				pageId: "Spawning",
				text: () => SHelper.Translation.Get("GMCM.Spawning.Name")
			);
			configMenu.AddPageLink(
				mod: ModManifest,
				pageId: "Difficulty",
				text: () => SHelper.Translation.Get("GMCM.Difficulty.Name")
			);
			configMenu.AddPageLink(
				mod: ModManifest,
				pageId: "Sprites",
				text: () => SHelper.Translation.Get("GMCM.Sprites.Name")
			);
			configMenu.AddPageLink(
				mod: ModManifest,
				pageId: "Audio",
				text: () => SHelper.Translation.Get("GMCM.Audio.Name")
			);
			configMenu.AddPage(
				mod: ModManifest,
				pageId: "Spawning",
				pageTitle: () => SHelper.Translation.Get("GMCM.Spawning.Name")
			);
			configMenu.AddSectionTitle(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.BossSpawnPercentChance.Name")
			);
			configMenu.AddParagraph(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.BossSpawnPercentChance.Desc")
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.MonsterArea.Name"),
				getValue: () => Config.PercentChanceOfBossInMonsterArea,
				setValue: value => Config.PercentChanceOfBossInMonsterArea = value,
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Farm.Name"),
				getValue: () => Config.PercentChanceOfBossInFarm,
				setValue: value => Config.PercentChanceOfBossInFarm = value,
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Town.Name"),
				getValue: () => Config.PercentChanceOfBossInTown,
				setValue: value => Config.PercentChanceOfBossInTown = value,
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Forest.Name"),
				getValue: () => Config.PercentChanceOfBossInForest,
				setValue: value => Config.PercentChanceOfBossInForest = value,
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Mountain.Name"),
				getValue: () => Config.PercentChanceOfBossInMountain,
				setValue: value => Config.PercentChanceOfBossInMountain = value,
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Desert.Name"),
				getValue: () => Config.PercentChanceOfBossInDesert,
				setValue: value => Config.PercentChanceOfBossInDesert = value,
				min: 0,
				max: 100
			);
			if (SHelper.ModRegistry.IsLoaded("FlashShifter.SVECode"))
			{
				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.CrimsonBadlands.Name"),
					getValue: () => Config.PercentChanceOfBossInCrimsonBadlands,
					setValue: value => Config.PercentChanceOfBossInCrimsonBadlands = value,
					min: 0,
					max: 100
				);
			}
			configMenu.AddSectionTitle(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.BossProbabilityWeights.Name")
			);
			configMenu.AddParagraph(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.BossProbabilityWeights.Desc")
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.BugBoss.Name"),
				getValue: () => Config.WeightBugBossChance,
				setValue: value => Config.WeightBugBossChance = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.GhostBoss.Name"),
				getValue: () => Config.WeightGhostBossChance,
				setValue: value => Config.WeightGhostBossChance = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.SerpentBoss.Name"),
				getValue: () => Config.WeightSerpentBossChance,
				setValue: value => Config.WeightSerpentBossChance = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.SkeletonBoss.Name"),
				getValue: () => Config.WeightSkeletonBossChance,
				setValue: value => Config.WeightSkeletonBossChance = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.SkullBoss.Name"),
				getValue: () => Config.WeightSkullBossChance,
				setValue: value => Config.WeightSkullBossChance = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.SquidKidBoss.Name"),
				getValue: () => Config.WeightSquidBossChance,
				setValue: value => Config.WeightSquidBossChance = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.SlimeBoss.Name"),
				getValue: () => Config.WeightSlimeBossChance,
				setValue: value => Config.WeightSlimeBossChance = value
			);
			configMenu.AddPage(
				mod: ModManifest,
				pageId: "Difficulty",
				pageTitle: () => SHelper.Translation.Get("GMCM.Difficulty.Name")
			);
			configMenu.AddSectionTitle(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.UndergroundDifficulty.Name")
			);
			configMenu.AddParagraph(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.UndergroundDifficulty.Desc")
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.BaseUndergroundDifficulty.Name"),
				getValue: () => Config.BaseUndergroundDifficulty,
				setValue: value => Config.BaseUndergroundDifficulty = value
			);
			configMenu.AddSectionTitle(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.OverlandDifficulty.Name")
			);
			configMenu.AddParagraph(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.OverlandDifficulty.Desc")
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.MinOverlandDifficulty.Name"),
				getValue: () => Config.MinOverlandDifficulty,
				setValue: value => {
					Config.MinOverlandDifficulty = value;
					Config.MaxOverlandDifficulty = Math.Max(Config.MinOverlandDifficulty, Config.MaxOverlandDifficulty);
				}
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.MaxOverlandDifficulty.Name"),
				getValue: () => Config.MaxOverlandDifficulty,
				setValue: value => {
					Config.MaxOverlandDifficulty = value;
					Config.MinOverlandDifficulty = Math.Min(Config.MinOverlandDifficulty, Config.MaxOverlandDifficulty);
				}
			);
			configMenu.AddPage(
				mod: ModManifest,
				pageId: "Sprites",
				pageTitle: () => SHelper.Translation.Get("GMCM.Sprites.Name")
			);
			configMenu.AddSectionTitle(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.AlternateTextures.Name")
			);
			configMenu.AddParagraph(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.AlternateTextures.Desc")
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.UseAlternateTextures.Name"),
				getValue: () => Config.UseAlternateTextures,
				setValue: value => Config.UseAlternateTextures = value
			);
			configMenu.AddSectionTitle(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.Dimensions.Name")
			);
			configMenu.AddParagraph(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.Dimensions.Desc")
			);
			configMenu.AddSectionTitle(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.BugBoss.Name")
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Scale.Name"),
				getValue: () => Config.BugBossScale,
				setValue: value => Config.BugBossScale = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Height.Name"),
				getValue: () => Config.BugBossHeight,
				setValue: value => Config.BugBossHeight = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Width.Name"),
				getValue: () => Config.BugBossWidth,
				setValue: value => Config.BugBossWidth = value
			);
			configMenu.AddSectionTitle(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.GhostBoss.Name")
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Scale.Name"),
				getValue: () => Config.GhostBossScale,
				setValue: value => Config.GhostBossScale = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Height.Name"),
				getValue: () => Config.GhostBossHeight,
				setValue: value => Config.GhostBossHeight = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Width.Name"),
				getValue: () => Config.GhostBossWidth,
				setValue: value => Config.GhostBossWidth = value
			);
			configMenu.AddSectionTitle(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.SerpentBoss.Name")
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Scale.Name"),
				getValue: () => Config.SerpentBossScale,
				setValue: value => Config.SerpentBossScale = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Height.Name"),
				getValue: () => Config.SerpentBossHeight,
				setValue: value => Config.SerpentBossHeight = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Width.Name"),
				getValue: () => Config.SerpentBossWidth,
				setValue: value => Config.SerpentBossWidth = value
			);
			configMenu.AddSectionTitle(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.SkeletonBoss.Name")
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Scale.Name"),
				getValue: () => Config.SkeletonBossScale,
				setValue: value => Config.SkeletonBossScale = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Height.Name"),
				getValue: () => Config.SkeletonBossHeight,
				setValue: value => Config.SkeletonBossHeight = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Width.Name"),
				getValue: () => Config.SkeletonBossWidth,
				setValue: value => Config.SkeletonBossWidth = value
			);
			configMenu.AddSectionTitle(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.SkullBoss.Name")
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Scale.Name"),
				getValue: () => Config.SkullBossScale,
				setValue: value => Config.SkullBossScale = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Height.Name"),
				getValue: () => Config.SkullBossHeight,
				setValue: value => Config.SkullBossHeight = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Width.Name"),
				getValue: () => Config.SkullBossWidth,
				setValue: value => Config.SkullBossWidth = value
			);
			configMenu.AddSectionTitle(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.SquidKidBoss.Name")
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Scale.Name"),
				getValue: () => Config.SquidKidBossScale,
				setValue: value => Config.SquidKidBossScale = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Height.Name"),
				getValue: () => Config.SquidKidBossHeight,
				setValue: value => Config.SquidKidBossHeight = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Width.Name"),
				getValue: () => Config.SquidKidBossWidth,
				setValue: value => Config.SquidKidBossWidth = value
			);
			configMenu.AddSectionTitle(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.SlimeBoss.Name")
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Scale.Name"),
				getValue: () => Config.SlimeBossScale,
				setValue: value => Config.SlimeBossScale = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Height.Name"),
				getValue: () => Config.SlimeBossHeight,
				setValue: value => Config.SlimeBossHeight = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Width.Name"),
				getValue: () => Config.SlimeBossWidth,
				setValue: value => Config.SlimeBossWidth = value
			);
			configMenu.AddPage(
				mod: ModManifest,
				pageId: "Audio",
				pageTitle: () => SHelper.Translation.Get("GMCM.Audio.Name")
			);
			configMenu.AddTextOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.BattleMusic.Name"),
				getValue: () => Config.BattleMusic,
				setValue: value => Config.BattleMusic = value
			);
			configMenu.AddTextOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.VictorySound.Name"),
				getValue: () => Config.VictorySound,
				setValue: value => Config.VictorySound = value
			);
		}
	}
}
