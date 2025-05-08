using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace CropGrowthInfo
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
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

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Crop), nameof(Crop.drawWithOffset)),
					prefix: new HarmonyMethod(typeof(Crop_drawWithOffset_Patch), nameof(Crop_drawWithOffset_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Crop), nameof(Crop.drawWithOffset)),
					postfix: new HarmonyMethod(typeof(Crop_drawWithOffset_Patch), nameof(Crop_drawWithOffset_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Crop), nameof(Crop.draw)),
					prefix: new HarmonyMethod(typeof(Crop_draw_Patch), nameof(Crop_draw_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Crop), nameof(Crop.draw)),
					postfix: new HarmonyMethod(typeof(Crop_draw_Patch), nameof(Crop_draw_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Game1), nameof(Game1.drawMouseCursor)),
					postfix: new HarmonyMethod(typeof(Game1_drawMouseCursor_Patch), nameof(Game1_drawMouseCursor_Patch.Postfix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
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
			configMenu.AddSectionTitle(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.Keys.Text")
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.RequireToggle.Name"),
				getValue: () => Config.RequireToggle,
				setValue: value => Config.RequireToggle = value
			);
			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ToggleButton.Name"),
				getValue: () => Config.ToggleButton,
				setValue: value => Config.ToggleButton = value
			);
			configMenu.AddSectionTitle(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.Information.Text")
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ShowCropName.Name"),
				getValue: () => Config.ShowCropName,
				setValue: value => Config.ShowCropName = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ShowCurrentPhase.Name"),
				getValue: () => Config.ShowCurrentPhase,
				setValue: value => Config.ShowCurrentPhase = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ShowDaysInCurrentPhase.Name"),
				getValue: () => Config.ShowDaysInCurrentPhase,
				setValue: value => Config.ShowDaysInCurrentPhase = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ShowTotalGrowth.Name"),
				getValue: () => Config.ShowTotalGrowth,
				setValue: value => Config.ShowTotalGrowth = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ShowReadyText.Name"),
				getValue: () => Config.ShowReadyText,
				setValue: value => Config.ShowReadyText = value
			);
			configMenu.AddSectionTitle(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.Display.Text")
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.TextScale.Name"),
				getValue: () => Config.TextScale,
				setValue: value => Config.TextScale = value,
				min: 0.5f,
				max: 4.0f,
				interval: 0.1f
			);
			configMenu.AddComplexOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.CropNameColor.Name"),
				draw: DrawNameText
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => "   " + SHelper.Translation.Get("GMCM.Red.Name"),
				getValue: () => Config.NameColor.R,
				setValue: value => Config.NameColor = new Color(value, Config.NameColor.G, Config.NameColor.B),
				min:0,
				max:255
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => "   " + SHelper.Translation.Get("GMCM.Green.Name"),
				getValue: () => Config.NameColor.G,
				setValue: value => Config.NameColor = new Color(Config.NameColor.R, value, Config.NameColor.B),
				min: 0,
				max: 255
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => "   " + SHelper.Translation.Get("GMCM.Blue.Name"),
				getValue: () => Config.NameColor.B,
				setValue: value => Config.NameColor = new Color(Config.NameColor.R, Config.NameColor.G, value),
				min: 0,
				max: 255
			);
			configMenu.AddComplexOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.CurrentPhaseColor.Name"),
				draw: DrawPhaseText
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => "   " + SHelper.Translation.Get("GMCM.Red.Name"),
				getValue: () => Config.CurrentPhaseColor.R,
				setValue: value => Config.CurrentPhaseColor = new Color(value, Config.CurrentPhaseColor.G, Config.CurrentPhaseColor.B),
				min:0,
				max:255
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => "   " + SHelper.Translation.Get("GMCM.Green.Name"),
				getValue: () => Config.CurrentPhaseColor.G,
				setValue: value => Config.CurrentPhaseColor = new Color(Config.CurrentPhaseColor.R, value, Config.CurrentPhaseColor.B),
				min: 0,
				max: 255
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => "   " + SHelper.Translation.Get("GMCM.Blue.Name"),
				getValue: () => Config.CurrentPhaseColor.B,
				setValue: value => Config.CurrentPhaseColor = new Color(Config.CurrentPhaseColor.R, Config.CurrentPhaseColor.G, value),
				min: 0,
				max: 255
			);
			configMenu.AddComplexOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.DaysInCurrentPhaseColor.Name"),
				draw: DrawPhaseDaysText
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => "   " + SHelper.Translation.Get("GMCM.Red.Name"),
				getValue: () => Config.CurrentGrowthColor.R,
				setValue: value => Config.CurrentGrowthColor = new Color(value, Config.CurrentGrowthColor.G, Config.CurrentGrowthColor.B),
				min:0,
				max:255
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => "   " + SHelper.Translation.Get("GMCM.Green.Name"),
				getValue: () => Config.CurrentGrowthColor.G,
				setValue: value => Config.CurrentGrowthColor = new Color(Config.CurrentGrowthColor.R, value, Config.CurrentGrowthColor.B),
				min: 0,
				max: 255
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => "   " + SHelper.Translation.Get("GMCM.Blue.Name"),
				getValue: () => Config.CurrentGrowthColor.B,
				setValue: value => Config.CurrentGrowthColor = new Color(Config.CurrentGrowthColor.R, Config.CurrentGrowthColor.G, value),
				min: 0,
				max: 255
			);
			configMenu.AddComplexOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.TotalGrowthColor.Name"),
				draw: DrawTotalDaysText
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => "   " + SHelper.Translation.Get("GMCM.Red.Name"),
				getValue: () => Config.TotalGrowthColor.R,
				setValue: value => Config.TotalGrowthColor = new Color(value, Config.TotalGrowthColor.G, Config.TotalGrowthColor.B),
				min:0,
				max:255
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => "   " + SHelper.Translation.Get("GMCM.Green.Name"),
				getValue: () => Config.TotalGrowthColor.G,
				setValue: value => Config.TotalGrowthColor = new Color(Config.TotalGrowthColor.R, value, Config.TotalGrowthColor.B),
				min: 0,
				max: 255
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => "   " + SHelper.Translation.Get("GMCM.Blue.Name"),
				getValue: () => Config.TotalGrowthColor.B,
				setValue: value => Config.TotalGrowthColor = new Color(Config.TotalGrowthColor.R, Config.TotalGrowthColor.G, value),
				min: 0,
				max: 255
			);
			configMenu.AddComplexOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ReadyTextColor.Name"),
				draw: DrawReadyText
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => "   " + SHelper.Translation.Get("GMCM.Red.Name"),
				getValue: () => Config.ReadyColor.R,
				setValue: value => Config.ReadyColor = new Color(value, Config.ReadyColor.G, Config.ReadyColor.B),
				min:0,
				max:255
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => "   " + SHelper.Translation.Get("GMCM.Green.Name"),
				getValue: () => Config.ReadyColor.G,
				setValue: value => Config.ReadyColor = new Color(Config.ReadyColor.R, value, Config.ReadyColor.B),
				min: 0,
				max: 255
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => "   " + SHelper.Translation.Get("GMCM.Blue.Name"),
				getValue: () => Config.ReadyColor.B,
				setValue: value => Config.ReadyColor = new Color(Config.ReadyColor.R, Config.ReadyColor.G, value),
				min: 0,
				max: 255
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.CropOpacity.Name"),
				getValue: () => Config.CropOpacity,
				setValue: value => Config.CropOpacity = value,
				min: 0.0f,
				max: 1.0f,
				interval: 0.01f
			);
		}

		private void DrawNameText(SpriteBatch b, Vector2 pos)
		{
			b.DrawString(Game1.dialogueFont, SHelper.Translation.Get("name"), pos, Config.NameColor);
		}

		private void DrawPhaseText(SpriteBatch b, Vector2 pos)
		{
			b.DrawString(Game1.dialogueFont, string.Format(SHelper.Translation.Get("phase"), "x", "y"), pos, Config.CurrentPhaseColor);
		}

		private void DrawPhaseDaysText(SpriteBatch b, Vector2 pos)
		{
			b.DrawString(Game1.dialogueFont, string.Format(SHelper.Translation.Get("current"), "x", "y"), pos, Config.CurrentGrowthColor);
		}

		private void DrawTotalDaysText(SpriteBatch b, Vector2 pos)
		{
			b.DrawString(Game1.dialogueFont, string.Format(SHelper.Translation.Get("total"), "x", "y"), pos, Config.TotalGrowthColor);
		}

		private void DrawReadyText(SpriteBatch b, Vector2 pos)
		{
			b.DrawString(Game1.dialogueFont, SHelper.Translation.Get("ready"), pos, Config.ReadyColor);
		}
	}
}
