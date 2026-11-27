using System;
using HarmonyLib;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Tools;

namespace AFKPause
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static ModConfig Config;

		internal static ModEntry context;
		internal static int elapsedTicks;

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			if (!Config.ModEnabled)
				return;

			context = this;

			SMonitor = Monitor;
			SHelper = helper;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
			helper.Events.Display.Rendered += Display_Rendered;
			helper.Events.Input.CursorMoved += PlayerInput;
			helper.Events.Input.MouseWheelScrolled += PlayerInput;
			helper.Events.Input.ButtonPressed += PlayerInput;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Game1), nameof(Game1.UpdateGameClock)),
					prefix: new HarmonyMethod(typeof(Game1_UpdateGameClock_Patch), nameof(Game1_UpdateGameClock_Patch.Prefix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void Display_Rendered(object sender, RenderedEventArgs e)
		{
			if (!Config.ModEnabled || !Config.ShowAFKText || Config.FreezeGame || elapsedTicks < Config.TicksTilAFK || !Context.IsPlayerFree)
				return;
			SpriteText.drawStringWithScrollCenteredAt(e.SpriteBatch, Config.AFKText, Game1.viewport.Width / 2, Game1.viewport.Height / 2);
		}

		private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
		{
			if (Game1.activeClickableMenu is AFKMenu)
				return;
			if (!Config.ModEnabled || !Context.CanPlayerMove || Game1.player.movementDirections.Count > 0 || (Game1.player.CurrentTool is FishingRod && (Game1.player.CurrentTool as FishingRod).inUse()) || Game1.input.GetKeyboardState().GetPressedKeys().Length > 0 || (byte)AccessTools.Field(typeof(MouseState), "_buttons").GetValue(Game1.input.GetMouseState()) > 0)
			{
				elapsedTicks = 0;
				return;
			}
			if (elapsedTicks >= Config.TicksTilAFK && Config.FreezeGame)
			{
				SMonitor.Log("Going AFK");
				Game1.activeClickableMenu = new AFKMenu();
			}
			else if (elapsedTicks < Config.TicksTilAFK)
				elapsedTicks++;
		}

		private void PlayerInput(object sender, object e)
		{
			if (e is CursorMovedEventArgs && !Config.WakeOnMouseMove)
				return;
			elapsedTicks = 0;
			if (Game1.activeClickableMenu is AFKMenu)
				Game1.activeClickableMenu = null;
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
				getValue: () => Config.ModEnabled,
				setValue: value => Config.ModEnabled = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.FreezeGame.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.FreezeGame.Tooltip"),
				getValue: () => Config.FreezeGame,
				setValue: value => Config.FreezeGame = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.TicksTilAFK.Name"),
				getValue: () => Config.TicksTilAFK,
				setValue: value => Config.TicksTilAFK = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ShowAFKText.Name"),
				getValue: () => Config.ShowAFKText,
				setValue: value => Config.ShowAFKText = value
			);
			configMenu.AddTextOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.AFKText.Name"),
				getValue: () => Config.AFKText,
				setValue: value => Config.AFKText = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.WakeOnMouseMove.Name"),
				getValue: () => Config.WakeOnMouseMove,
				setValue: value => Config.WakeOnMouseMove = value
			);
		}
	}
}
