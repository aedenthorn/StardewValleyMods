using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Quests;
using System;
using System.Collections.Generic;

namespace StardewGames
{
	public partial class ModEntry : Mod
	{
		public static IMonitor SMonitor;
		public static IModHelper SHelper;
		public static IManifest SModManifest;
		public static ModConfig Config;
		public static ModEntry context;
		public static Dictionary<string, GamesGameData> gameDataDict = new Dictionary<string, GamesGameData>();
        
		public static bool returnToMenu;

		public enum CurrentMiniGame
		{
			None,
            PrairieKing,
			JunimoKart
        }
        public static CurrentMiniGame currentMiniGame;

        public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Display.RenderingStep += Display_RenderingStep;
            Harmony harmony = new Harmony(ModManifest.UniqueID);
			harmony.PatchAll();
        }

        private void Display_RenderingStep(object sender, StardewModdingAPI.Events.RenderingStepEventArgs e)
        {
            if (!Config.ModEnabled || currentMiniGame == CurrentMiniGame.None || Game1.currentMinigame == null || e.Step != StardewValley.Mods.RenderSteps.Minigame)
            {
                return;
            }
			//e.SpriteBatch.End();
   //         Game1.currentMinigame.draw(e.SpriteBatch);
			//e.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);

        }

        public override object GetApi()
		{
			return new StardewGamesAPI();
		}
        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			if (Config.ModEnabled)
			{
				if (Config.ShowPrairieKing)
				{
					gameDataDict["aedenthorn.PrarieKing"] = new GamesGameData(ClickPrairieKing, DrawPrairieKing);
				}
				if (Config.ShowJunimo)
				{
					gameDataDict["aedenthorn.JuniomKart"] = new GamesGameData(ClickJunimo, DrawJunimo);
                }
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
                gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ShowJunimo.Name"),
					getValue: () => Config.ShowJunimo,
					setValue: value => Config.ShowJunimo = value
				);
                gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ShowPrairieKing.Name"),
					getValue: () => Config.ShowPrairieKing,
					setValue: value => Config.ShowPrairieKing = value
				);
			}
		}

    }
}
