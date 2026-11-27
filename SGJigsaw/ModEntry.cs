using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.GameData.Shops;

namespace SGJigsaw
{
	public partial class ModEntry : Mod
	{
		public static IMonitor SMonitor;
		public static IModHelper SHelper;
		public static IManifest SModManifest;
		public static ModConfig Config;
		public static ModEntry context;
		public static string boxId = "aedenthorn.SGJigsaw_puzzle_box";
		public static string infoKey = "aedenthorn.SGJigsaw/info";
        public static ISGJigsawAPI sgapi;

        public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();
            ModEntry.Config.Music.Sort();

            context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            Harmony harmony = new Harmony(ModManifest.UniqueID);
			harmony.PatchAll();
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
			if(Config.ModEnabled && e.NameWithoutLocale.IsEquivalentTo("Data/Furniture"))
			{
				e.Edit((IAssetData data) =>
				{
					data.AsDictionary<string, string>().Data[boxId] = $"{boxId}/decor/1 1/1 1/1/1000/-1/{SHelper.Translation.Get("box")}/0/{boxId}";
				});
			}
			if(Config.ModEnabled && e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
			{
				e.Edit((IAssetData data) =>
				{
					data.AsDictionary<string, ShopData>().Data["SeedShop"].Items.Add(new ShopItemData()
					{
						Id = boxId,
						ItemId = boxId,
					});
				});
			}
			else if (e.NameWithoutLocale.IsEquivalentTo(boxId))
			{
				e.LoadFromModFile<Texture2D>("assets/box.png", StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
			}
        }

        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			sgapi = SHelper.ModRegistry.GetApi<ISGJigsawAPI>("aedenthorn.StardewGames");
            // Get Generic Mod Config Menu's API
            var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (sgapi != null)
            {
                sgapi.AddGame(context.ModManifest.UniqueID, LoadGame, DrawMenuSlot);
            }

            if (gmcm is not null)
			{
				// Register mod
				gmcm.Register(
					mod: ModManifest,
					reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );
                gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
					getValue: () => Config.ModEnabled,
					setValue: value => Config.ModEnabled = value
				);
                gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.FuseConnected.Name"),
					getValue: () => Config.FuseConnected,
					setValue: value => Config.FuseConnected = value
				);
                gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ShuffleMusic.Name"),
					getValue: () => Config.ShuffleMusic,
					setValue: value => Config.ShuffleMusic = value
				);
                gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Snap.Name"),
					getValue: () => Config.Snap,
					setValue: value => Config.Snap = value
				);
                gmcm.AddKeybind(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.SnapKey.Name"),
					getValue: () => Config.SnapKey,
					setValue: value => Config.SnapKey = value
				);
                gmcm.AddKeybind(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.SolveKey.Name"),
					getValue: () => Config.SolveKey,
					setValue: value => Config.SolveKey = value
				);
                gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.SnapSound.Name"),
					getValue: () => Config.SnapSound,
					setValue: value => Config.SnapSound = value
				);
                gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.SolveSound.Name"),
					getValue: () => Config.SolveSound,
					setValue: value => Config.SolveSound = value
				);
			}
		}

    }
}
