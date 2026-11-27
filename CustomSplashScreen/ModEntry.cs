using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.IO;
using System.Linq;

namespace CustomSplashScreen
{
	public partial class ModEntry : Mod
	{
		public static IMonitor SMonitor;
		public static IModHelper SHelper;
		public static IManifest SModManifest;
		public static ModConfig Config;
		public static ModEntry context;
		public static Texture2D splashBackground;
		

        public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            Harmony harmony = new Harmony(ModManifest.UniqueID);
			harmony.PatchAll();
			ReloadTextures();
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
			if(Config.ModEnabled && Game1.activeClickableMenu is TitleMenu tm && e.Button == SButton.OemCloseBrackets)
			{
				tm.logoFadeTimer = 5000;
				tm.fadeFromWhiteTimer = 4000;
                ReloadTextures();
            }
        }

        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
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
                gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.StartMusicAtSplash.Name"),
					getValue: () => Config.StartMusicAtSplash,
					setValue: value => Config.StartMusicAtSplash = value
				);
                gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.MenuMusic.Name"),
					getValue: () => Config.MenuMusic,
					setValue: value => Config.MenuMusic = value
				);
                gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.AltSurpriseChance.Name"),
					getValue: () => Config.AltSurpriseChance.ToString(),
					setValue: value => Config.AltSurpriseChance = double.TryParse(value, out var d) ? d : Config.AltSurpriseChance
				);

                var configMenuExt = Helper.ModRegistry.GetApi<IGMCMOptionsAPI>("jltaylor-us.GMCMOptions");
                if (configMenuExt is not null)
                {
                    configMenuExt.AddColorOption(
                        mod: ModManifest,
                        getValue: () => Config.BackgroundColor,
                        setValue: (c) => Config.BackgroundColor = c,
                        name: () => SHelper.Translation.Get("GMCM.BackgroundColor.Name")
                    );
                }
            }
		}
	}

}
