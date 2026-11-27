using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Monsters;

namespace BossCreatures
{
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

		private static LootList BossLootList;
		private static Texture2D healthBarTexture;
		private static int toggleSprite = 0;
		private static int lastBossHealth;
		private static string defaultWeather;
		private static string islandWeather;
		private static readonly List<string> BossSpawnedLocations = new();

		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += OnGameLaunched;
			helper.Events.GameLoop.DayStarted += OnDayStarted;
			helper.Events.GameLoop.DayEnding += OnDayEnding;
			helper.Events.Player.Warped += Warped;
			helper.Events.GameLoop.UpdateTicked += UpdateTicked;
			helper.Events.Display.WindowResized += WindowResized;

			LoadBossLootList();
		}

		private void OnDayStarted(object sender, DayStartedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			BossSpawnedLocations.Clear();
		}

		private void OnDayEnding(object sender, DayEndingEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			defaultWeather = null;
			islandWeather = null;
			Utility.ForEachLocation(location =>
			{
				for (int i = location.characters.Count - 1; i >= 0; i--)
				{
					if (IsBoss(location.characters[i]) || location.characters[i] is ToughFly or ToughGhost)
					{
						location.characters.RemoveAt(i);
					}
				}
				return true;
			});
		}

		private void Warped(object sender, WarpedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			SMonitor.Log("Entered location: " + e.NewLocation.Name);
			if (!Game1.eventUp)
			{
				if (HasBoss(e.OldLocation) && !HasBoss(e.NewLocation))
				{
					SetDefaultWeather(e.NewLocation);
					RevertMusic(e.NewLocation);
				}
				TryAddBoss(e.NewLocation);
				Game1.updateWeatherIcon();
			}
			else if(Game1.CurrentEvent?.exitLocation is not null)
			{
				GameLocation exitLocation = Game1.CurrentEvent.exitLocation.Location;

				if (HasBoss(e.OldLocation) && !HasBoss(exitLocation))
				{
					SetDefaultWeather(exitLocation);
					RevertMusic(exitLocation);
				}
				Game1.CurrentEvent.onEventFinished += () => {
					TryAddBoss(exitLocation);
					Game1.updateWeatherIcon();
				};
			}
		}

		private void UpdateTicked(object sender, UpdateTickedEventArgs e)
		{
			if (!Config.ModEnabled || !Context.IsWorldReady)
				return;

			Monster boss = GetBoss(Game1.player.currentLocation);

			if (boss is not null)
			{
				MakeVillagersPanic(Game1.player.currentLocation);
			}
		}

		public static void OnRenderedHud(object sender, RenderedHudEventArgs e)
		{
			Monster boss = GetBoss(Game1.player.currentLocation);

			if (boss is not null)
			{
				if (boss.Health != lastBossHealth)
				{
					lastBossHealth = boss.Health;
					GenerateBossHealthBarTexture(boss.Health, boss.MaxHealth);
				}
				e.SpriteBatch.Draw(healthBarTexture, new Vector2((int)Utility.ModifyCoordinateForUIScale(Game1.viewport.Width * 0.125f), 100), Color.White);
				if (!Utility.isOnScreen(boss.Position, 0))
				{
					int x = (int)Math.Max(10, Math.Min(Game1.viewport.X + Game1.viewport.Width - 90, boss.Position.X) - Game1.viewport.X);
					int y = (int)Math.Max(10, Math.Min(Game1.viewport.Y + Game1.viewport.Height - 90, boss.Position.Y) - Game1.viewport.Y);

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
			else
			{
				SHelper.Events.Display.RenderedHud -= OnRenderedHud;
			}
		}

		private void WindowResized(object sender, WindowResizedEventArgs e)
		{
			if (!Config.ModEnabled || !Context.IsWorldReady)
				return;

			Monster boss = GetBoss(Game1.player.currentLocation);

			if (boss is not null)
			{
				GenerateBossHealthBarTexture(boss.Health, boss.MaxHealth);
			}
		}

		private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
		{
			// Get Generic Mod Config Menu's API
			IGenericModConfigMenuApi gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

			if (gmcm is not null)
			{
				// Register mod
				gmcm.Register(
					mod: ModManifest,
					reset: () => Config = new ModConfig(),
					save: () => Helper.WriteConfig(Config)
				);

				// Main section
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
					getValue: () => Config.ModEnabled,
					setValue: value => {
						if (Context.IsWorldReady && !value)
						{
							if (HasBoss(Game1.player.currentLocation))
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
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.BattleWeather.Name"),
					getValue: () => Config.BattleWeather,
					setValue: value => {
						if (Context.IsWorldReady && HasBoss(Game1.player.currentLocation))
						{
							if (!value)
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
				gmcm.AddPageLink(
					mod: ModManifest,
					pageId: "Spawning",
					text: () => SHelper.Translation.Get("GMCM.Spawning.Name")
				);
				gmcm.AddPageLink(
					mod: ModManifest,
					pageId: "Difficulty",
					text: () => SHelper.Translation.Get("GMCM.Difficulty.Name")
				);
				gmcm.AddPageLink(
					mod: ModManifest,
					pageId: "Sprites",
					text: () => SHelper.Translation.Get("GMCM.Sprites.Name")
				);
				gmcm.AddPageLink(
					mod: ModManifest,
					pageId: "Audio",
					text: () => SHelper.Translation.Get("GMCM.Audio.Name")
				);

				// Spawning
				gmcm.AddPage(
					mod: ModManifest,
					pageId: "Spawning",
					pageTitle: () => SHelper.Translation.Get("GMCM.Spawning.Name")
				);
				gmcm.AddSectionTitle(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.BossSpawnPercentChance.Name")
				);
				gmcm.AddParagraph(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.BossSpawnPercentChance.Desc")
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.MonsterArea.Name"),
					getValue: () => Config.PercentChanceOfBossInMonsterArea,
					setValue: value => Config.PercentChanceOfBossInMonsterArea = value,
					min: 0,
					max: 100
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Farm.Name"),
					getValue: () => Config.PercentChanceOfBossInFarm,
					setValue: value => Config.PercentChanceOfBossInFarm = value,
					min: 0,
					max: 100
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Town.Name"),
					getValue: () => Config.PercentChanceOfBossInTown,
					setValue: value => Config.PercentChanceOfBossInTown = value,
					min: 0,
					max: 100
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Forest.Name"),
					getValue: () => Config.PercentChanceOfBossInForest,
					setValue: value => Config.PercentChanceOfBossInForest = value,
					min: 0,
					max: 100
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Mountain.Name"),
					getValue: () => Config.PercentChanceOfBossInMountain,
					setValue: value => Config.PercentChanceOfBossInMountain = value,
					min: 0,
					max: 100
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Desert.Name"),
					getValue: () => Config.PercentChanceOfBossInDesert,
					setValue: value => Config.PercentChanceOfBossInDesert = value,
					min: 0,
					max: 100
				);
				if (SHelper.ModRegistry.IsLoaded("FlashShifter.SVECode"))
				{
					gmcm.AddNumberOption(
						mod: ModManifest,
						name: () => SHelper.Translation.Get("GMCM.CrimsonBadlands.Name"),
						getValue: () => Config.PercentChanceOfBossInCrimsonBadlands,
						setValue: value => Config.PercentChanceOfBossInCrimsonBadlands = value,
						min: 0,
						max: 100
					);
				}
				gmcm.AddSectionTitle(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.BossProbabilityWeights.Name")
				);
				gmcm.AddParagraph(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.BossProbabilityWeights.Desc")
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.BugBoss.Name"),
					getValue: () => Config.WeightBugBossChance,
					setValue: value => Config.WeightBugBossChance = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.GhostBoss.Name"),
					getValue: () => Config.WeightGhostBossChance,
					setValue: value => Config.WeightGhostBossChance = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.SerpentBoss.Name"),
					getValue: () => Config.WeightSerpentBossChance,
					setValue: value => Config.WeightSerpentBossChance = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.SkeletonBoss.Name"),
					getValue: () => Config.WeightSkeletonBossChance,
					setValue: value => Config.WeightSkeletonBossChance = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.SkullBoss.Name"),
					getValue: () => Config.WeightSkullBossChance,
					setValue: value => Config.WeightSkullBossChance = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.SquidKidBoss.Name"),
					getValue: () => Config.WeightSquidBossChance,
					setValue: value => Config.WeightSquidBossChance = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.SlimeBoss.Name"),
					getValue: () => Config.WeightSlimeBossChance,
					setValue: value => Config.WeightSlimeBossChance = value
				);

				// Difficulty
				gmcm.AddPage(
					mod: ModManifest,
					pageId: "Difficulty",
					pageTitle: () => SHelper.Translation.Get("GMCM.Difficulty.Name")
				);
				gmcm.AddSectionTitle(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.UndergroundDifficulty.Name")
				);
				gmcm.AddParagraph(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.UndergroundDifficulty.Desc")
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.BaseUndergroundDifficulty.Name"),
					getValue: () => Config.BaseUndergroundDifficulty,
					setValue: value => Config.BaseUndergroundDifficulty = value
				);
				gmcm.AddSectionTitle(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.OverlandDifficulty.Name")
				);
				gmcm.AddParagraph(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.OverlandDifficulty.Desc")
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.MinOverlandDifficulty.Name"),
					getValue: () => Config.MinOverlandDifficulty,
					setValue: value => {
						Config.MinOverlandDifficulty = value;
						Config.MaxOverlandDifficulty = Math.Max(Config.MinOverlandDifficulty, Config.MaxOverlandDifficulty);
					}
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.MaxOverlandDifficulty.Name"),
					getValue: () => Config.MaxOverlandDifficulty,
					setValue: value => {
						Config.MaxOverlandDifficulty = value;
						Config.MinOverlandDifficulty = Math.Min(Config.MinOverlandDifficulty, Config.MaxOverlandDifficulty);
					}
				);

				// Sprites
				gmcm.AddPage(
					mod: ModManifest,
					pageId: "Sprites",
					pageTitle: () => SHelper.Translation.Get("GMCM.Sprites.Name")
				);
				gmcm.AddSectionTitle(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.AlternateTextures.Name")
				);
				gmcm.AddParagraph(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.AlternateTextures.Desc")
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.UseAlternateTextures.Name"),
					getValue: () => Config.UseAlternateTextures,
					setValue: value => Config.UseAlternateTextures = value
				);
				gmcm.AddSectionTitle(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.Dimensions.Name")
				);
				gmcm.AddParagraph(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.Dimensions.Desc")
				);
				gmcm.AddSectionTitle(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.BugBoss.Name")
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Scale.Name"),
					getValue: () => Config.BugBossScale,
					setValue: value => Config.BugBossScale = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Height.Name"),
					getValue: () => Config.BugBossHeight,
					setValue: value => Config.BugBossHeight = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Width.Name"),
					getValue: () => Config.BugBossWidth,
					setValue: value => Config.BugBossWidth = value
				);
				gmcm.AddSectionTitle(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.GhostBoss.Name")
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Scale.Name"),
					getValue: () => Config.GhostBossScale,
					setValue: value => Config.GhostBossScale = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Height.Name"),
					getValue: () => Config.GhostBossHeight,
					setValue: value => Config.GhostBossHeight = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Width.Name"),
					getValue: () => Config.GhostBossWidth,
					setValue: value => Config.GhostBossWidth = value
				);
				gmcm.AddSectionTitle(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.SerpentBoss.Name")
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Scale.Name"),
					getValue: () => Config.SerpentBossScale,
					setValue: value => Config.SerpentBossScale = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Height.Name"),
					getValue: () => Config.SerpentBossHeight,
					setValue: value => Config.SerpentBossHeight = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Width.Name"),
					getValue: () => Config.SerpentBossWidth,
					setValue: value => Config.SerpentBossWidth = value
				);
				gmcm.AddSectionTitle(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.SkeletonBoss.Name")
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Scale.Name"),
					getValue: () => Config.SkeletonBossScale,
					setValue: value => Config.SkeletonBossScale = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Height.Name"),
					getValue: () => Config.SkeletonBossHeight,
					setValue: value => Config.SkeletonBossHeight = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Width.Name"),
					getValue: () => Config.SkeletonBossWidth,
					setValue: value => Config.SkeletonBossWidth = value
				);
				gmcm.AddSectionTitle(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.SkullBoss.Name")
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Scale.Name"),
					getValue: () => Config.SkullBossScale,
					setValue: value => Config.SkullBossScale = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Height.Name"),
					getValue: () => Config.SkullBossHeight,
					setValue: value => Config.SkullBossHeight = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Width.Name"),
					getValue: () => Config.SkullBossWidth,
					setValue: value => Config.SkullBossWidth = value
				);
				gmcm.AddSectionTitle(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.SquidKidBoss.Name")
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Scale.Name"),
					getValue: () => Config.SquidKidBossScale,
					setValue: value => Config.SquidKidBossScale = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Height.Name"),
					getValue: () => Config.SquidKidBossHeight,
					setValue: value => Config.SquidKidBossHeight = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Width.Name"),
					getValue: () => Config.SquidKidBossWidth,
					setValue: value => Config.SquidKidBossWidth = value
				);
				gmcm.AddSectionTitle(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.SlimeBoss.Name")
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Scale.Name"),
					getValue: () => Config.SlimeBossScale,
					setValue: value => Config.SlimeBossScale = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Height.Name"),
					getValue: () => Config.SlimeBossHeight,
					setValue: value => Config.SlimeBossHeight = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Width.Name"),
					getValue: () => Config.SlimeBossWidth,
					setValue: value => Config.SlimeBossWidth = value
				);

				// Audio
				gmcm.AddPage(
					mod: ModManifest,
					pageId: "Audio",
					pageTitle: () => SHelper.Translation.Get("GMCM.Audio.Name")
				);
				gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.BattleMusic.Name"),
					getValue: () => Config.BattleMusic,
					setValue: value => Config.BattleMusic = value
				);
				gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.VictorySound.Name"),
					getValue: () => Config.VictorySound,
					setValue: value => Config.VictorySound = value
				);
			}
		}
	}
}
