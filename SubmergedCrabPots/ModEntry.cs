using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Objects;

namespace SubmergedCrabPots
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
					original: AccessTools.Method(typeof(CrabPot), nameof(CrabPot.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
					prefix: new HarmonyMethod(typeof(CrabPot_draw_Patch), nameof(CrabPot_draw_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(CrabPot), nameof(CrabPot.performObjectDropInAction)),
					postfix: new HarmonyMethod(typeof(CrabPot_performObjectDropInAction_Patch), nameof(CrabPot_performObjectDropInAction_Patch.Postfix))
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
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.SubmergeHarvestable.Name"),
				getValue: () => Config.SubmergeHarvestable,
				setValue: value => Config.SubmergeHarvestable = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ShowRipples.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.ShowRipples.Tooltip"),
				getValue: () => Config.ShowRipples,
				setValue: value => Config.ShowRipples = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.BobberScale.Name"),
				getValue: () => Config.BobberScale,
				setValue: value => Config.BobberScale = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.BobberTintR.Name"),
				getValue: () => Config.BobberTint.R,
				setValue: value => Config.BobberTint = new Color(value, Config.BobberTint.G, Config.BobberTint.B, Config.BobberTint.A),
				min: 0,
				max: 255
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.BobberTintG.Name"),
				getValue: () => Config.BobberTint.G,
				setValue: value => Config.BobberTint = new Color(Config.BobberTint.R, value, Config.BobberTint.B, Config.BobberTint.A),
				min: 0,
				max: 255
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.BobberTintB.Name"),
				getValue: () => Config.BobberTint.B,
				setValue: value => Config.BobberTint = new Color(Config.BobberTint.R, Config.BobberTint.G, value, Config.BobberTint.A),
				min: 0,
				max: 255
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.BobberOpacity.Name"),
				getValue: () => Config.BobberOpacity,
				setValue: value => Config.BobberOpacity = value,
				min: 0,
				max: 100
			);
		}
	}
}
