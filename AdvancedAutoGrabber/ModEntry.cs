using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;

namespace AdvancedAutoGrabber
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;
		internal static int seconds;
		public const string limitKey = "aedenthorn.AdvancedAutoGrabber/limit";

		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
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
                    name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.GrabOnDugUp.Name"),
                    getValue: () => Config.GrabOnDugUp,
                    setValue: value => Config.GrabOnDugUp = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.SendToChests.Name"),
                    tooltip: () => SHelper.Translation.Get("GMCM.SendToChests.Desc"),
                    getValue: () => Config.SendToChests,
                    setValue: value => Config.SendToChests = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.IncludeBuildingChests.Name"),
                    getValue: () => Config.IncludeBuildingChests,
                    setValue: value => Config.IncludeBuildingChests = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.OpacityPercent.Name"),
                    getValue: () => Config.OpacityPercent,
                    setValue: value => Config.OpacityPercent = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.GrabRange.Name"),
                    tooltip: () => SHelper.Translation.Get("GMCM.GrabRange.Desc"),
                    getValue: () => Config.GrabRange,
                    setValue: value => Config.GrabRange = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.ChestRange.Name"),
                    tooltip: () => SHelper.Translation.Get("GMCM.ChestRange.Desc"),
                    getValue: () => Config.ChestRange,
                    setValue: value => Config.ChestRange = value
                );

            }

		}
	}
}
