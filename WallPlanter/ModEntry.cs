using System;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace WallPlanters
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static ModConfig Config;
		internal static ModEntry context;

		private static Texture2D wallPotTexture;
		private static Texture2D wallPotTextureWet;

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;

			SMonitor = Monitor;
			SHelper = helper;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
			helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Utility), nameof(Utility.playerCanPlaceItemHere)),
					prefix: new HarmonyMethod(typeof(Utility_playerCanPlaceItemHere_Patch), nameof(Utility_playerCanPlaceItemHere_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(IndoorPot), nameof(IndoorPot.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
					prefix: new HarmonyMethod(typeof(IndoorPot_draw_Patch), nameof(IndoorPot_draw_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(IndoorPot), nameof(IndoorPot.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
					postfix: new HarmonyMethod(typeof(IndoorPot_draw_Patch), nameof(IndoorPot_draw_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(IndoorPot), nameof(IndoorPot.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
					transpiler: new HarmonyMethod(typeof(IndoorPot_draw_Patch), nameof(IndoorPot_draw_Patch.Transpiler))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
		{
			try
			{
				wallPotTexture = Game1.content.Load<Texture2D>("aedenthorn.WallPlanters/wall_pot");
			}
			catch
			{
				wallPotTexture = Helper.ModContent.Load<Texture2D>("assets/wall_pot.png");
			}

			try
			{
				wallPotTextureWet = Game1.content.Load<Texture2D>("aedenthorn.WallPlanters/wall_pot_wet");
			}
			catch
			{
				wallPotTextureWet = Helper.ModContent.Load<Texture2D>("assets/wall_pot_wet.png");
			}
		}

		private void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e)
		{
			if (!Config.EnableMod || !Context.IsWorldReady)
				return;

			int delta = Helper.Input.IsDown(Config.UpKey) ? 1 : (Helper.Input.IsDown(Config.DownKey) ? -1 : 0);

			if (delta != 0 && typeof(DecoratableLocation).IsAssignableFrom(Game1.currentLocation.GetType()) && Game1.currentLocation.objects.TryGetValue(Game1.currentCursorTile, out Object obj) && obj is IndoorPot && (Game1.currentLocation as DecoratableLocation).isTileOnWall((int)obj.TileLocation.X, (int)obj.TileLocation.Y))
			{
				int offset = Config.OffsetY;
				string key = Helper.Input.IsDown(Config.ContentOffsetKey) ? "aedenthorn.WallPlanters/innerOffset" : "aedenthorn.WallPlanters/offset";

				if (obj.modData.TryGetValue(key, out string offsetString))
				{
					_ = int.TryParse(offsetString, out offset);
				}
				obj.modData[key] = offset + delta + "";
			}
		}

		private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
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
				getValue: () => Config.EnableMod,
				setValue: value => Config.EnableMod = value
			);
			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ContentOffsetKey.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.ContentOffsetKey.Tooltip"),
				getValue: () => Config.ContentOffsetKey,
				setValue: value => Config.ContentOffsetKey = value
			);
			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.UpKey.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.UpKey.Tooltip"),
				getValue: () => Config.UpKey,
				setValue: value => Config.UpKey = value
			);
			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.DownKey.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.DownKey.Tooltip"),
				getValue: () => Config.DownKey,
				setValue: value => Config.DownKey = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.OffsetY.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.OffsetY.Tooltip"),
				getValue: () => Config.OffsetY,
				setValue: value => Config.OffsetY = value
			);
		}
	}
}
