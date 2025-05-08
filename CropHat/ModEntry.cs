using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace CropHat
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static ModConfig Config;

		internal static ModEntry context;
		public const string seedKey = "aedenthorn.CropHat/seed";
		public const string daysKey = "aedenthorn.CropHat/days";
		public const string phaseKey = "aedenthorn.CropHat/phase";
		public const string phasesKey = "aedenthorn.CropHat/phases";
		public const string rowKey = "aedenthorn.CropHat/row";
		public const string grownKey = "aedenthorn.CropHat/grownKey";
		public const string xKey = "aedenthorn.CropHat/x";
		public const string yKey = "aedenthorn.CropHat/y";

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;

			SMonitor = Monitor;
			SHelper = helper;
			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
			helper.Events.Display.RenderingWorld += Display_RenderingWorld;
			helper.Events.Input.ButtonPressed += Input_ButtonPressed;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(FarmerRenderer), nameof(FarmerRenderer.drawHairAndAccesories)),
					prefix: new HarmonyMethod(typeof(FarmerRenderer_drawHairAndAccesories_Patch), nameof(FarmerRenderer_drawHairAndAccesories_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(FarmerRenderer), nameof(FarmerRenderer.drawHairAndAccesories)),
					postfix: new HarmonyMethod(typeof(FarmerRenderer_drawHairAndAccesories_Patch), nameof(FarmerRenderer_drawHairAndAccesories_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Hat), nameof(Hat.draw)),
					prefix: new HarmonyMethod(typeof(Hat_draw_Patch), nameof(Hat_draw_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Hat), nameof(Hat.drawInMenu), new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) }),
					prefix: new HarmonyMethod(typeof(Hat_drawInMenu_Patch), nameof(Hat_drawInMenu_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Hat), "loadDisplayFields"),
					prefix: new HarmonyMethod(typeof(Hat_loadDisplayFields_Patch), nameof(Hat_loadDisplayFields_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
					prefix: new HarmonyMethod(typeof(GameLocation_checkAction_Patch), nameof(GameLocation_checkAction_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(InventoryPage), nameof(InventoryPage.receiveLeftClick)),
					prefix: new HarmonyMethod(typeof(InventoryPage_receiveLeftClick_Patch), nameof(InventoryPage_receiveLeftClick_Patch.Prefix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if(Config.EnableMod && Context.CanPlayerMove && e.Button == Config.CheatButton)
			{
				Monitor.Log("Pressed key");
				NewDay(Game1.player.hat.Value);
			}
		}

		private void Display_RenderingWorld(object sender, RenderingWorldEventArgs e)
		{
			if (!Config.EnableMod)
				return;

			if (Config.AllowOthersToPick)
			{
				foreach (Farmer farmer in Game1.getAllFarmers())
				{
					var loc = farmer.Position + new Vector2(32, -88);

					if (Game1.player.currentLocation == farmer.currentLocation && farmer.hat.Value is not null && Vector2.Distance(Game1.GlobalToLocal(loc), Game1.getMousePosition().ToVector2()) < 32 && farmer.hat.Value.modData.ContainsKey(seedKey))
					{
						if (ReadyToHarvest(farmer.hat.Value))
						{
							Game1.mouseCursor = 6;
							if (!Utility.withinRadiusOfPlayer((int)farmer.Position.X, (int)farmer.Position.Y, 1, Game1.player))
							{
								Game1.mouseCursorTransparency = 0.5f;
							}
						}
					}
				}
			}
			else
			{
				var loc = Game1.player.Position + new Vector2(32, -88);

				if (Vector2.Distance(Game1.GlobalToLocal(loc), Game1.getMousePosition().ToVector2()) < 32 && Game1.player.hat.Value is not null && Game1.player.hat.Value.modData.TryGetValue(phaseKey, out _))
				{
					if (ReadyToHarvest(Game1.player.hat.Value))
					{
						Game1.mouseCursor = 6;
					}
				}
			}
		}

		private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
		{
			if (Game1.player?.hat?.Value?.modData.ContainsKey(seedKey) == true)
			{
				NewDay(Game1.player.hat.Value);
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
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.AllowOthersToPick.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.AllowOthersToPick.Tooltip"),
				getValue: () => Config.AllowOthersToPick,
				setValue: value => Config.AllowOthersToPick = value
			);
		}
	}
}
