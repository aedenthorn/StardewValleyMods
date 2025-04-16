using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using xTile.Layers;
using xTile.ObjectModel;
using StardewModdingAPI;
using StardewValley;

namespace DynamicMapTiles
{
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static ModConfig Config;
		internal static ModEntry context;

		internal static string dictPath = "aedenthorn.DynamicMapTiles/dictionary";
		internal static Dictionary<string, List<PushedTile>> pushingDict = new();
		internal static Dictionary<string, DynamicTileInfo> dynamicDict = new();

		internal const string addLayerKey = "DMT/addLayer";
		internal const string addTilesheetKey = "DMT/addTilesheet";
		internal const string changeIndexKey = "DMT/changeIndex";
		internal const string changeMultipleIndexKey = "DMT/changeMultipleIndex";
		internal const string changePropertiesKey = "DMT/changeProperties";
		internal const string changeMultiplePropertiesKey = "DMT/changeMultipleProperties";
		internal const string triggerKey = "DMT/trigger";
		internal const string explodeKey = "DMT/explode";
		internal const string explosionKey = "DMT/explosion";
		internal const string pushKey = "DMT/push";
		internal const string pushableKey = "DMT/pushable";
		internal const string pushAlsoKey = "DMT/pushAlso";
		internal const string pushOthersKey = "DMT/pushOthers";
		internal const string soundKey = "DMT/sound";
		internal const string teleportKey = "DMT/teleport";
		internal const string teleportTileKey = "DMT/teleportTile";
		internal const string giveKey = "DMT/give";
		internal const string takeKey = "DMT/take";
		internal const string chestKey = "DMT/chest";
		internal const string chestAdvancedKey = "DMT/chestAdvanced";
		internal const string messageKey = "DMT/message";
		internal const string eventKey = "DMT/event";
		internal const string mailKey = "DMT/mail";
		internal const string mailRemoveKey = "DMT/mailRemove";
		internal const string mailBoxKey = "DMT/mailbox";
		internal const string invalidateKey = "DMT/invalidate";
		internal const string musicKey = "DMT/music";
		internal const string healthKey = "DMT/health";
		internal const string staminaKey = "DMT/stamina";
		internal const string healthPerSecondKey = "DMT/healthPerSecond";
		internal const string staminaPerSecondKey = "DMT/staminaPerSecond";
		internal const string buffKey = "DMT/buff";
		internal const string speedKey = "DMT/speed";
		internal const string moveKey = "DMT/move";
		internal const string emoteKey = "DMT/emote";
		internal const string animationKey = "DMT/animation";
		internal const string slipperyKey = "DMT/slippery";

		internal static List<string> actionKeys = new()
		{
			addLayerKey,
			addTilesheetKey,
			changeIndexKey,
			changeMultipleIndexKey,
			changePropertiesKey,
			changeMultiplePropertiesKey,
			explodeKey,
			explosionKey,
			pushKey,
			pushOthersKey,
			soundKey,
			teleportKey,
			teleportTileKey,
			giveKey,
			takeKey,
			chestKey,
			chestAdvancedKey,
			messageKey,
			eventKey,
			mailKey,
			mailRemoveKey,
			mailBoxKey,
			invalidateKey,
			musicKey,
			healthKey,
			staminaKey,
			healthPerSecondKey,
			staminaPerSecondKey,
			buffKey,
			speedKey,
			moveKey,
			emoteKey,
			animationKey
		};

		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;

			SMonitor = Monitor;
			SHelper = helper;

			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
			Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
			Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
			Helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
			Helper.Events.Content.AssetRequested += Content_AssetRequested;
			Helper.Events.Player.Warped += Player_Warped;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.explode)),
					prefix: new HarmonyMethod(typeof(GameLocation_explode_Patch), nameof(GameLocation_explode_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.explosionAt)),
					postfix: new HarmonyMethod(typeof(GameLocation_explosionAt_Patch), nameof(GameLocation_explosionAt_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character) }),
					prefix: new HarmonyMethod(typeof(GameLocation_isCollidingPosition_Patch), nameof(GameLocation_isCollidingPosition_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.draw)),
					postfix: new HarmonyMethod(typeof(GameLocation_draw_Patch), nameof(GameLocation_draw_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performToolAction)),
					prefix: new HarmonyMethod(typeof(GameLocation_performToolAction_Patch), nameof(GameLocation_performToolAction_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
					prefix: new HarmonyMethod(typeof(GameLocation_checkAction_Patch), nameof(GameLocation_checkAction_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Farmer), nameof(Farmer.getMovementSpeed)),
					postfix: new HarmonyMethod(typeof(Farmer_getMovementSpeed_Patch), nameof(Farmer_getMovementSpeed_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Farmer), nameof(Farmer.MovePosition)),
					prefix: new HarmonyMethod(typeof(Farmer_MovePosition_Patch), nameof(Farmer_MovePosition_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Farmer), nameof(Farmer.MovePosition)),
					postfix: new HarmonyMethod(typeof(Farmer_MovePosition_Patch), nameof(Farmer_MovePosition_Patch.Postfix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;
			Stopwatch s = new();
			s.Start();
			var dict = Helper.GameContent.Load<Dictionary<string, DynamicTileInfo>>(dictPath);
			foreach (var kvp in dict)
			{
				Monitor.Log($"Adding properties from {kvp.Key}");
				var info = kvp.Value;
				int count = 0;
				if (info.properties is null)
					continue;
				if (info.locations is not null && !info.locations.Contains(e.NewLocation.Name))
					continue;
				if (info.tileSheets is not null && !info.tileSheets.Exists(s => e.NewLocation.Map.TileSheets.ToList().Exists(ss => ss.Id == s)) && !info.tileSheets.Exists(s => e.NewLocation.Map.TileSheets.ToList().Exists(ss => ss.ImageSource.Contains(s))))
					continue;
				if (info.tileSheetPaths is not null && !info.tileSheetPaths.Exists(s => e.NewLocation.Map.TileSheets.ToList().Exists(ss => ss.ImageSource.Contains(s))))
					continue;
				foreach (var layer in e.NewLocation.Map.Layers)
				{
					if (info.layers is not null && !info.layers.Contains(layer.Id))
						continue;
					for (int x = 0; x < layer.Tiles.Array.GetLength(0); x++)
					{
						for (int y = 0; y < layer.Tiles.Array.GetLength(1); y++)
						{
							if (layer.Tiles[x, y] is not null)
							{
								if (info.tileSheets is not null && !info.tileSheets.Contains(layer.Tiles[x, y].TileSheet.Id))
									continue;
								if (info.tileSheetPaths is not null && !info.tileSheetPaths.Exists(s => layer.Tiles[x, y].TileSheet.ImageSource.Contains(s)))
									continue;
								if (info.indexes is not null && !info.indexes.Contains(layer.Tiles[x, y].TileIndex))
									continue;
								Point point = new(x, y);
								if (info.rectangles is not null && !info.rectangles.Exists(r => r.Contains(point)))
									continue;
								count++;
								foreach (var prop in info.properties)
								{
									layer.Tiles[x, y].Properties[prop.Key] = prop.Value;
								}
							}
						}
					}
				}
				Monitor.Log($"Added properties from {kvp.Key} to {count} tiles in {e.NewLocation.Name}");
			}
			s.Stop();
			Monitor.Log($"{s.Elapsed} elapsed");
		}

		private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
		{

		}

		private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;
			if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
			{
				e.LoadFrom(() => new Dictionary<string, DynamicTileInfo>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
			}
		}

		public override object GetApi()
		{
			return new DynamicMapTilesApi();
		}
		private void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
		{
			if(!Config.ModEnabled || !Context.IsPlayerFree)
				return;
			var center = Game1.player.GetBoundingBox().Center;
			var tile = Game1.player.currentLocation.Map.GetLayer("Back").PickTile(new xTile.Dimensions.Location(center.X, center.Y), Game1.viewport.Size);
			if (tile is null)
				return;
			if (tile.Properties.TryGetValue(healthPerSecondKey, out PropertyValue value) && int.TryParse(value, out int number))
			{
				if (number < 0)
				{
					Game1.player.takeDamage(Math.Abs(number), false, null);

				}
				else
				{
					Game1.player.health = Math.Min(Game1.player.health + number, Game1.player.maxHealth);
					Game1.player.currentLocation.debris.Add(new Debris(number, new Vector2(Game1.player.StandingPixel.X + 8, Game1.player.StandingPixel.Y), Color.LimeGreen, 1f, Game1.player));
				}
			}
			if (tile.Properties.TryGetValue(staminaPerSecondKey, out value) && int.TryParse(value, out number))
			{
				Game1.player.Stamina += number;
			}
		}

		private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
		{
			if (!Config.ModEnabled || !Context.IsPlayerFree || !pushingDict.TryGetValue(Game1.currentLocation.Name, out List<PushedTile> tiles))
				return;
			for(int i = tiles.Count - 1; i >= 0; i--)
			{
				var tile = tiles[i];
				tile.position += GetNextTile(tile.dir);
				if(tile.position.X % 64 == 0 && tile.position.Y % 64 == 0)
				{
					tile.tile.Layer.Tiles[tile.position.X / 64, tile.position.Y / 64] = tile.tile;
					foreach (var l in Game1.currentLocation.map.Layers)
					{
						List<string> actions = new();
						var t = l.PickTile(new xTile.Dimensions.Location(tile.position.X, tile.position.Y), Game1.viewport.Size);
						if (t is not null)
						{
							TriggerActions(new List<Layer>() { t.Layer }, tile.farmer, new Point(tile.position.X / 64, tile.position.Y / 64), new List<string>() { "Pushed" });
						}
					}
					tiles.RemoveAt(i);
				}
			}
		}

		private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
		{
		}
		private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
				setValue: value => Config.ModEnabled = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.TriggerDuringEvents.Name"),
				getValue: () => Config.TriggerDuringEvents,
				setValue: value => Config.TriggerDuringEvents = value
			);
		}
	}
}
