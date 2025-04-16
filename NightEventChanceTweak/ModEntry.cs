using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace NightEventChanceTweak
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

			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Utility), nameof(Utility.pickFarmEvent)),
					prefix: new HarmonyMethod(typeof(Utility_pickFarmEvent_Patch), nameof(Utility_pickFarmEvent_Patch.Prefix)),
					postfix: new HarmonyMethod(typeof(Utility_pickFarmEvent_Patch), nameof(Utility_pickFarmEvent_Patch.Postfix))
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
				name: () => Helper.Translation.Get("GMCM.ModEnabled.Name"),
				getValue: () => Config.ModEnabled,
				setValue: value => Config.ModEnabled = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.IgnoreEventConditions.Name"),
				getValue: () => Config.IgnoreEventConditions,
				setValue: value => Config.IgnoreEventConditions = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.CumulativeChance.Name"),
				getValue: () => Config.CumulativeChance,
				setValue: value => Config.CumulativeChance = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.CropFairyChance.Name"),
				getValue: () => Config.CropFairyChance,
				setValue: value => Config.CropFairyChance = value,
				min: 0f,
				max: 100f,
				interval: 0.1f
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.WitchChance.Name"),
				getValue: () => Config.WitchChance,
				setValue: value => Config.WitchChance = value,
				min: 0f,
				max: 100f,
				interval: 0.1f
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.MeteorChance.Name"),
				getValue: () => Config.MeteorChance,
				setValue: value => Config.MeteorChance = value,
				min: 0f,
				max: 100f,
				interval: 0.1f
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.StoneOwlChance.Name"),
				getValue: () => Config.StoneOwlChance,
				setValue: value => Config.StoneOwlChance = value,
				min: 0f,
				max: 100f,
				interval: 0.1f
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.StrangeCapsuleChance.Name"),
				getValue: () => Config.StrangeCapsuleChance,
				setValue: value => Config.StrangeCapsuleChance = value,
				min: 0f,
				max: 100f,
				interval: 0.1f
			);
		}
	}

}
