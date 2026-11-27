using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace StardewVN
{
	public partial class ModEntry : Mod
	{
		public static IMonitor SMonitor;
		public static IModHelper SHelper;
		public static IManifest SModManifest;
		public static ModConfig Config;
		public static ModEntry context;
        public static string dictPath = "aedenthorn.StardewVN/dict";
        public static Dictionary<string, VisualNovelData> vnDict = new Dictionary<string, VisualNovelData>();
        public static IStardewGamesAPI sgapi;

        public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            Harmony harmony = new Harmony(ModManifest.UniqueID);
			harmony.PatchAll();
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, VisualNovelData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
        }

        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            sgapi = SHelper.ModRegistry.GetApi<IStardewGamesAPI>("aedenthorn.StardewGames");
            if (sgapi != null)
            {
                sgapi.AddGame(context.ModManifest.UniqueID, LoadGame, DrawMenuSlot);
            }

            // Get Generic Mod Config Menu's API
            var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

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
					setValue: value => Config.ModEnabled = value
				);
			}
		}

    }
}
