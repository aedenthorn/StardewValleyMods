using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace MapTokens
{
	public partial class ModEntry : Mod
	{
		public static IMonitor SMonitor;
		public static IModHelper SHelper;
		public static IManifest SModManifest;
		public static ModConfig Config;
		public static ModEntry context;


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
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.OemCloseBrackets)
            {
                Monitor.Log(Helper.GameContent.Load<Dictionary<string, string>>("Data/Furniture")["asdf"]);
            }
        }

        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
            var api = Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");

            api.RegisterToken(ModManifest, "FarmHouseEntryTile", new MainFarmHouseEntryTile(0, " "));
            api.RegisterToken(ModManifest, "FarmHouseEntryTileComma", new MainFarmHouseEntryTile(0, ","));
            api.RegisterToken(ModManifest, "FarmHouseEntryTileX", new MainFarmHouseEntryTile(1));
            api.RegisterToken(ModManifest, "FarmHouseEntryTileY", new MainFarmHouseEntryTile(2));

            api.RegisterToken(ModManifest, "MainMailboxTile", new MainMailboxTile(0, " "));
            api.RegisterToken(ModManifest, "MainMailboxTileComma", new MainMailboxTile(0, ","));
            api.RegisterToken(ModManifest, "MainMailboxTileX", new MainMailboxTile(1));
            api.RegisterToken(ModManifest, "MainMailboxTileY", new MainMailboxTile(2));

            api.RegisterToken(ModManifest, "GrampaShrineTile", new GrampaShrineTile(0, " "));
            api.RegisterToken(ModManifest, "GrampaShrineTileComma", new GrampaShrineTile(0, ","));
            api.RegisterToken(ModManifest, "GrampaShrineTileX", new GrampaShrineTile(1));
            api.RegisterToken(ModManifest, "GrampaShrineTileY", new GrampaShrineTile(2));

            api.RegisterToken(ModManifest, "MapPropertyTile", new MapPropertyTile(0, " "));
            api.RegisterToken(ModManifest, "MapPropertyTileComma", new MapPropertyTile(0, ","));
            api.RegisterToken(ModManifest, "MapPropertyTileX", new MapPropertyTile(1));
            api.RegisterToken(ModManifest, "MapPropertyTileY", new MapPropertyTile(2));

            api.RegisterToken(ModManifest, "SpousePatioTile", new SpousePatioTile(0, " "));
            api.RegisterToken(ModManifest, "SpousePatioTileComma", new SpousePatioTile(0, ","));
            api.RegisterToken(ModManifest, "SpousePatioTileX", new SpousePatioTile(1));
            api.RegisterToken(ModManifest, "SpousePatioTileY", new SpousePatioTile(2));

            api.RegisterToken(ModManifest, "FarmTotemTile", new TotemTile(0, " "));
            api.RegisterToken(ModManifest, "FarmTotemTileComma", new TotemTile(0, ","));
            api.RegisterToken(ModManifest, "FarmTotemTileX", new TotemTile(1));
            api.RegisterToken(ModManifest, "FarmTotemTileY", new TotemTile(2));

            api.RegisterToken(ModManifest, "PetBowlTile", new PetBowlTile(0, " "));
            api.RegisterToken(ModManifest, "PetBowlComma", new PetBowlTile(0, ","));
            api.RegisterToken(ModManifest, "PetBowlX", new PetBowlTile(1));
            api.RegisterToken(ModManifest, "PetBowlY", new PetBowlTile(2));

            api.RegisterToken(ModManifest, "FarmHouseEntryTiles", new FarmHouseEntryTiles(0, " "));
            api.RegisterToken(ModManifest, "FarmHouseEntryTilesComma", new FarmHouseEntryTiles(0, ","));

            api.RegisterToken(ModManifest, "MailboxTiles", new MailboxTiles(0, " "));
            api.RegisterToken(ModManifest, "MailboxTilesComma", new MailboxTiles(0, ","));

            api.RegisterToken(ModManifest, "WarpTiles", new WarpTiles(0, " "));
            api.RegisterToken(ModManifest, "WarpTilesComma", new WarpTiles(0, ","));

            api.RegisterToken(ModManifest, "MapPath", new MapPath());

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
