using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Weeds
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;
		internal static string modKey = "aedenthorn.Weeds/weed";
		internal static string modFlippedKey = "aedenthorn.Weeds/weedFlipped";
		internal static string texPath = "aedenthorn.Weeds/weed";
        internal static Texture2D weedTex;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
			
            Harmony harmony = new(ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(Crop), nameof(Crop.newDay)),
                prefix: new(typeof(ModEntry), nameof(Crop_newDay_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.canGrabSomethingFromHere)),
                postfix: new(typeof(ModEntry), nameof(Utility_canGrabSomethingFromHere_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.performUseAction)),
                prefix: new(typeof(ModEntry), nameof(HoeDirt_performUseAction_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.dayUpdate)),
                postfix: new(typeof(ModEntry), nameof(HoeDirt_dayUpdate_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.draw)),
                postfix: new(typeof(ModEntry), nameof(HoeDirt_draw_Postfix))
            );
        }

        private void Content_AssetRequested(object? sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(texPath))
            {
                e.LoadFrom(() => SHelper.ModContent.Load<Texture2D>("assets/weeds.png"), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        private void GameLoop_GameLaunched(object? sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
            weedTex = SHelper.GameContent.Load<Texture2D>(texPath);

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
                    name: () => ModEntry.SHelper.Translation.Get("GMCM.ModEnabled.Name"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.WeedsStopGrowth.Name"),
                    getValue: () => Config.WeedsStopGrowth,
                    setValue: value => Config.WeedsStopGrowth = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.WeededDoubleGrowth.Name"),
                    getValue: () => Config.WeededDoubleGrowth,
                    setValue: value => Config.WeededDoubleGrowth = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.WeedExp.Name"),
                    getValue: () => Config.WeedExp,
                    setValue: value => Config.WeedExp = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.WeedGrowthPerDayMin.Name"),
                    getValue: () => Config.WeedGrowthPerDayMin,
                    setValue: value => Config.WeedGrowthPerDayMin = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.WeedGrowthPerDayMax.Name"),
                    getValue: () => Config.WeedGrowthPerDayMax,
                    setValue: value => Config.WeedGrowthPerDayMax = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.WeedStaminaUse.Name"),
                    getValue: () => Config.WeedStaminaUse,
                    setValue: value => Config.WeedStaminaUse = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.WeedTintR.Name"),
                    getValue: () => Config.WeedTintR,
                    setValue: value => Config.WeedTintR = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.WeedTintG.Name"),
                    getValue: () => Config.WeedTintG,
                    setValue: value => Config.WeedTintG = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.WeedTintB.Name"),
                    getValue: () => Config.WeedTintB,
                    setValue: value => Config.WeedTintB = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.WeedTintA.Name"),
                    getValue: () => Config.WeedTintA,
                    setValue: value => Config.WeedTintA = value
                );
            }

		}
	}
}
