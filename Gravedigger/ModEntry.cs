using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;

namespace Gravedigger
{
	public partial class ModEntry : Mod
	{
		public static IMonitor SMonitor;
		public static IModHelper SHelper;
		public static IManifest SModManifest;
		public static ModConfig Config;
		public static ModEntry context;
        public static Dictionary<string, List<Point>> graveTiles = new ();

        public const string modKey = "aedenthorn.Gravedigger";
		
        public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            Harmony harmony = new Harmony(ModManifest.UniqueID);
			harmony.PatchAll();
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            Helper.GameContent.InvalidateCache("Maps/Town");
        }


        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
		{
            if(!Config.ModEnabled) 
                return;
			if (e.NameWithoutLocale.IsEquivalentTo("Maps/Town"))
			{
                e.Edit((IAssetData data) =>
				{
                    graveTiles.Clear();
                    var mapData = data.AsMap();
                    var tiles = mapData.Data.GetLayer("Buildings").Tiles;
                    for (int x = 0; x < tiles.Array.GetLength(0); x++)
                    {
                        for (int y = 0; y < tiles.Array.GetLength(1); y++)
                        {
                            try
                            {
                                if (tiles[x, y]?.TileSheet.Id == "Town")
                                {
                                    if (tiles[x, y].TileIndex == 320 || tiles[x, y].TileIndex == 321)
                                    {
                                        graveTiles.Add(new Point(x, y + 1));
                                        graveTiles.Add(new Point(x, y + 2));
                                        mapData.Data.GetLayer("Back").Tiles[x, y + 1].Properties["Diggable"] = "T";
                                        mapData.Data.GetLayer("Back").Tiles[x, y + 2].Properties["Diggable"] = "T";
                                    }
                                    else if (tiles[x, y].TileIndex == 384)
                                    {
                                        graveTiles.Add(new Point(x + 1, y));
                                        graveTiles.Add(new Point(x + 2, y));
                                        mapData.Data.GetLayer("Back").Tiles[x + 1, y].Properties["Diggable"] = "T";
                                        mapData.Data.GetLayer("Back").Tiles[x + 2, y].Properties["Diggable"] = "T";
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                });
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
					name: () => SHelper.Translation.Get("GMCM.NPCReactAsGarbage.Name"),
					getValue: () => Config.NPCReactAsGarbage,
					setValue: value => Config.NPCReactAsGarbage = value
				);
                gmcm.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.GhostChance.Name"),
                    getValue: () => Config.GhostChance,
                    setValue: value => Config.GhostChance = value
                );

                gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.DigChance.Name"),
					getValue: () => Config.DigChance,
					setValue: value => Config.DigChance = value
				);
                
                
                gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ArtifactChance.Name"),
					getValue: () => Config.ArtifactChance,
					setValue: value => Config.ArtifactChance = value
				);
            }
		}
	}

}
