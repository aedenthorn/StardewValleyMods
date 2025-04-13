using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using xTile.Dimensions;
using xTile.Layers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using StardewValley.GameData.Locations;
using StardewValley.GameData;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData.WildTrees;
using Object = StardewValley.Object;

namespace Dreamscape
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
			helper.Events.Content.AssetRequested += Content_AssetRequested;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(EmilysParrot), nameof(EmilysParrot.doAction)),
					prefix: new HarmonyMethod(typeof(EmilysParrot_doAction_Patch), nameof(EmilysParrot_doAction_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), "resetLocalState"),
					postfix: new HarmonyMethod(typeof(GameLocation_resetLocalState_Patch), nameof(GameLocation_resetLocalState_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
					prefix: new HarmonyMethod(typeof(GameLocation_checkAction_Patch), nameof(GameLocation_checkAction_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.UpdateWhenCurrentLocation), new Type[] { typeof(GameTime) }),
					postfix: new HarmonyMethod(typeof(GameLocation_UpdateWhenCurrentLocation_Patch), nameof(GameLocation_UpdateWhenCurrentLocation_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.doesTileSinkDebris)),
					prefix: new HarmonyMethod(typeof(GameLocation_doesTileSinkDebris_Patch), nameof(GameLocation_doesTileSinkDebris_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.sinkDebris)),
					prefix: new HarmonyMethod(typeof(GameLocation_sinkDebris_Patch), nameof(GameLocation_sinkDebris_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.draw)),
					prefix: new HarmonyMethod(typeof(HoeDirt_draw_Patch), nameof(HoeDirt_draw_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(FarmHouse), "resetLocalState"),
					postfix: new HarmonyMethod(typeof(FarmHouse_resetLocalState_Patch), nameof(FarmHouse_resetLocalState_Patch.Postfix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			if (e.NameWithoutLocale.IsEquivalentTo("Data/Locations"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, LocationData> data = asset.AsDictionary<string, LocationData>().Data;

					data.Add($"{ModManifest.UniqueID}_Dreamscape", new LocationData()
					{
						DisplayName = $"[{ModManifest.UniqueID}_i18n location.dreamscape.name]",
						DefaultArrivalTile = new Point(21, 15),
						CreateOnLoad = new CreateLocationData() {
							MapPath = SHelper.ModContent.GetInternalAssetName("assets/Dreamscape.tmx").Name,
							Type = "StardewValley.GameLocation",
							AlwaysActive = false
						},
						CanPlantHere = true,
						ExcludeFromNpcPathfinding = true,
						ArtifactSpots = null,
						FishAreas = null,
						Fish = null,
						Forage = null,
						MinDailyWeeds = 0,
						MaxDailyWeeds = 0,
						FirstDayWeedMultiplier = 0,
						MinDailyForageSpawn = 0,
						MaxDailyForageSpawn = 0,
						MaxSpawnedForageAtOnce = 0,
						ChanceForClay = 0,
						Music = null,
						MusicDefault = "EmilyDream",
						MusicContext = MusicContext.SubLocation,
						MusicIgnoredInRain = false,
						MusicIgnoredInSpring = false,
						MusicIgnoredInSummer = false,
						MusicIgnoredInFall = false,
						MusicIgnoredInWinter = false,
						MusicIgnoredInFallDebris = false,
						MusicIsTownTheme = false
					});
				});
			}
			if (e.Name.IsEquivalentTo("Data/WildTrees"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, WildTreeData> data = asset.AsDictionary<string, WildTreeData>().Data;

					data.Add($"{ModManifest.UniqueID}_DreamTree", new WildTreeData() {
						Textures = new List<WildTreeTextureData> () {
							new() {
								Texture = SHelper.ModContent.GetInternalAssetName("assets/DreamTree.png").Name,
							}
						},
						GrowthChance = 0.5f
					});
				});
			}
		}

		private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
		{
			GameLocation location = Game1.getLocationFromName($"{SModManifest.UniqueID}_Dreamscape");
			Layer layer = location.map.GetLayer("Back");
			MineShaft mine = new()
			{
				mineLevel = 200
			};
			HashSet<Vector2> trees = location.terrainFeatures.Values.Where(tf => tf is Tree).Select(tf => (tf as Tree).Tile).ToHashSet();

			for (int x = 0; x < layer.LayerWidth; x++)
			{
				for (int y = 0; y < layer.LayerHeight; y++)
				{
					Vector2 tile = new(x, y);

					if (tile == new Vector2(21, 15))
					{
						continue;
					}
					if (!location.objects.ContainsKey(tile) && !location.terrainFeatures.ContainsKey(tile) && layer.Tiles[new Location(x, y)]?.TileIndex == 26)
					{
						if (trees.Count < Config.MaxTrees && Game1.random.NextDouble() < Config.TreeSpawnChance / 100f && !trees.Contains(new Vector2(x - 1, y - 1)) && !trees.Contains(new Vector2(x - 1, y)) && !trees.Contains(new Vector2(x - 1, y + 1)) && !trees.Contains(new Vector2(x, y - 1)) && !trees.Contains(new Vector2(x, y + 1)) && !trees.Contains(new Vector2(x + 1, y - 1)) && !trees.Contains(new Vector2(x + 1, y)) && !trees.Contains(new Vector2(x + 1, y + 1)))
						{
							trees.Add(tile);
							location.terrainFeatures.Add(tile, new Tree($"{ModManifest.UniqueID}_DreamTree", Config.TreeGrowthStage));
						}
						else if (Game1.random.NextDouble() < Config.ObjectSpawnChance / 100f)
						{
							double chance = Game1.random.NextDouble();

							if (chance < 0.5)
							{
								location.objects.Add(tile, new Object((319 + Game1.random.Next(3)).ToString(), 1)
								{
								});
							}
							else if (chance < 0.65)
							{
								location.objects[tile] = new Object("80", 1) {
									IsSpawnedObject = true
								};
							}
							else if (chance < 0.74)
							{
								location.objects[tile] = new Object("86", 1) {
									IsSpawnedObject = true
								};
							}
							else if (chance < 0.83)
							{
								location.objects[tile] = new Object("84", 1) {
									IsSpawnedObject = true
								};
							}
							else if (chance < 0.90)
							{
								location.objects[tile] = new Object("82", 1) {
									IsSpawnedObject = true
								};
							}
							else
							{
								location.objects.Add(tile, new Object(mine.getRandomGemRichStoneForThisLevel(200), 1));
							}
						}
					}
				}
			}
		}

		private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
			TokensUtility.Register();

			// get Generic Mod Config Menu's API (if it's installed)
			var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			if (configMenu is null)
				return;

			// register mod
			configMenu.Register(
				mod: ModManifest,
				reset: () => Config = new ModConfig(),
				save: () => {
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Data/WildTrees"));
					Helper.WriteConfig(Config);
				}
			);

			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.ModEnabled.Name"),
				getValue: () => Config.ModEnabled,
				setValue: value => Config.ModEnabled = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.MaxTrees.Name"),
				tooltip: () => Helper.Translation.Get("GMCM.MaxTrees.Tooltip"),
				getValue: () => Config.MaxTrees,
				setValue: value => {
					value = Math.Clamp(value, 0, 100);
					Config.MaxTrees = value;
				}
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.TreeGrowthStage.Name"),
				tooltip: () => Helper.Translation.Get("GMCM.TreeGrowthStage.Tooltip"),
				getValue: () => Config.TreeGrowthStage,
				setValue: value => Config.TreeGrowthStage = value,
				min: 0,
				max: 5
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.TreeSpawnChance.Name"),
				tooltip: () => Helper.Translation.Get("GMCM.TreeSpawnChance.Tooltip"),
				getValue: () => Config.TreeSpawnChance,
				setValue: value => Config.TreeSpawnChance = value,
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.ObjectSpawnChance.Name"),
				tooltip: () => Helper.Translation.Get("GMCM.ObjectSpawnChance.Tooltip"),
				getValue: () => Config.ObjectSpawnChance,
				setValue: value => Config.ObjectSpawnChance = value,
				min: 0,
				max: 100
			);
		}

	}
}
