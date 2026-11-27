using System;
using System.Collections.Generic;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Objects;
using Object = StardewValley.Object;

namespace WeatherTotem
{
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			helper.Events.Content.AssetRequested += Content_AssetRequested;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Object), "rainTotem"),
					prefix: new HarmonyMethod(typeof(Object_rainTotem_Patch), nameof(Object_rainTotem_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Object), nameof(Object.performUseAction), new Type[] { typeof(GameLocation) }),
					postfix: new HarmonyMethod(typeof(Object_performUseAction_Patch), nameof(Object_performUseAction_Patch.Postfix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, ObjectData> data = asset.AsDictionary<string, ObjectData>().Data;

					data["681"].Name = "Weather Totem";
					data["681"].DisplayName = SHelper.Translation.Get("item.weather_totem.name");
					data["681"].Description = SHelper.Translation.Get("item.weather_totem.description");
				});
			}
		}

		public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			TokensUtility.Register();

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
						Config.ModEnabled = value;
						SHelper.GameContent.InvalidateCache("Data/Objects");
					}
				);
				gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.InvokeSound.Name"),
					getValue: () => Config.InvokeSound,
					setValue: value => Config.InvokeSound = value
				);
				gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.SunSound.Name"),
					getValue: () => Config.SunSound,
					setValue: value => Config.SunSound = value
				);
				gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.RainSound.Name"),
					getValue: () => Config.RainSound,
					setValue: value => Config.RainSound = value
				);
				gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.GreenRainSound.Name"),
					getValue: () => Config.GreenRainSound,
					setValue: value => Config.GreenRainSound = value
				);
				gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.StormSound.Name"),
					getValue: () => Config.StormSound,
					setValue: value => Config.StormSound = value
				);
				gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.SnowSound.Name"),
					getValue: () => Config.SnowSound,
					setValue: value => Config.SnowSound = value
				);
				gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.WindSound.Name"),
					getValue: () => Config.WindSound,
					setValue: value => Config.WindSound = value
				);
			}
		}
	}
}
