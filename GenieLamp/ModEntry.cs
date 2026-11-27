using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace GenieLamp
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static ModConfig Config;

		internal static ModEntry context;

		public const string modKey = "aedenthorn.GenieLamp";

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
					original: AccessTools.Method(typeof(Object), nameof(Object.performUseAction)),
					prefix: new HarmonyMethod(typeof(Object_performUseAction_Patch), nameof(Object_performUseAction_Patch.Prefix))
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
			if (configMenu is not null)
			{
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
				configMenu.AddTextOption(
					mod: ModManifest,
					name: () => Helper.Translation.Get("GMCM.LampItem.Name"),
					getValue: () => Config.LampItem,
					setValue: value => Config.LampItem = value
				);
				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => Helper.Translation.Get("GMCM.WishesPerItem.Name"),
					getValue: () => Config.WishesPerItem,
					setValue: value => Config.WishesPerItem = value
				);
				configMenu.AddTextOption(
					mod: ModManifest,
					name: () => Helper.Translation.Get("GMCM.MenuSound.Name"),
					getValue: () => Config.MenuSound,
					setValue: value => Config.MenuSound = value
				);
				configMenu.AddTextOption(
					mod: ModManifest,
					name: () => Helper.Translation.Get("GMCM.WishSound.Name"),
					getValue: () => Config.WishSound,
					setValue: value => Config.WishSound = value
				);
			}
		}
	}
}
