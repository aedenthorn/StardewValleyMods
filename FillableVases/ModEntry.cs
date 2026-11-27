using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Objects;
using System;
using System.Collections.Generic;

namespace FillableVases
{
	public partial class ModEntry : Mod
	{
		public static IMonitor SMonitor;
		public static IModHelper SHelper;
		public static IManifest SModManifest;
		public static ModConfig Config;
		public static ModEntry context;
        public static string dictPath = "aedenthorn.FillableVases/dict";
        public static string flowerKey = "aedenthorn.FillableVases/flowers";
        public static string prismaticKey = "aedenthorn.PrismaticFlowers/prismatic";
		public static IPrismaticFlowersAPI prismaticFlowersAPI;

        public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            Harmony harmony = new Harmony(ModManifest.UniqueID);
			harmony.PatchAll();
        }

        private void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
			cachedFlowerData.Clear();
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
				e.LoadFrom(() => new Dictionary<string, VaseData>()
				{
					{ 
						"ElegantVase", 
						new VaseData()
						{
							Flowers = new FlowerData[]
							{
								new FlowerData() { X = 20, Y = 20, Rotation = -0.4f, Origin = new Vector2(9,9) },
								new FlowerData() { X = 52, Y = 20, Rotation = 0.4f, Origin = new Vector2(9,9) },
								new FlowerData() { X = 36, Y = 20, Rotation = 0, Origin = new Vector2(9,9) },
							}
						}
					},
					{
						"1299", 
						new VaseData()
						{
							Flowers = new FlowerData[]
							{
								new FlowerData() { X = 20, Y = 28, Rotation = -0.4f, Origin = new Vector2(9,9) },
								new FlowerData() { X = 52, Y = 28, Rotation = 0.4f, Origin = new Vector2(9,9) },
								new FlowerData() { X = 36, Y = 28, Rotation = 0, Origin = new Vector2(9,9) },
							}
						} 
					}
				}, StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
                
            }
        }


        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			// Get Generic Mod Config Menu's API
			var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            prismaticFlowersAPI = Helper.ModRegistry.GetApi<IPrismaticFlowersAPI>("aedenthorn.PrismaticFlowers");

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
