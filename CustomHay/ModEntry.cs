using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;

namespace CustomHay
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

			if (!Config.ModEnabled)
				return;

			context = this;

			SMonitor = Monitor;
			SHelper = helper;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Grass), nameof(Grass.TryDropItemsOnCut)),
					transpiler: new HarmonyMethod(typeof(Grass_TryDropItemsOnCut_Patch), nameof(Grass_TryDropItemsOnCut_Patch.Transpiler))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			//MakeHatData();

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

			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.OrdinaryHayChance.Name"),
				getValue: () => Config.OrdinaryHayChance * 100f,
				setValue: value => Config.OrdinaryHayChance = value / 100f,
				min: 0f,
				max: 100f,
				interval: 1f
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.GoldHayChance.Name"),
				getValue: () => Config.GoldHayChance * 100f,
				setValue: value => Config.GoldHayChance = value / 100f,
				min: 0f,
				max: 100f,
				interval: 1f
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.IridiumHayChance.Name"),
				getValue: () => Config.IridiumHayChance * 100f,
				setValue: value => Config.IridiumHayChance = value / 100f,
				min: 0f,
				max: 100f,
				interval: 1f
			);
		}
	}
}
