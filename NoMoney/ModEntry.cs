using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Menus;

namespace NoMoney
{
	public partial class ModEntry : Mod
	{
		public static IMonitor SMonitor;
		public static IModHelper SHelper;
		public static IManifest SModManifest;
		public static ModConfig Config;
		public static ModEntry context;
		public static bool IsEnabled;

        public const string modKey = "aedenthorn.NoMoney";
		
        public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            Harmony harmony = new Harmony(ModManifest.UniqueID);
			harmony.PatchAll();
			foreach(var t in typeof(Game1).Assembly.GetTypes())
			{
				var m = t.GetMethod("salePrice");
				if(m != null && m.DeclaringType == t)
				{
					try
					{
                        harmony.Patch(
                            original: AccessTools.Method(t, "salePrice"),
                            prefix: new HarmonyMethod(typeof(ModEntry), nameof(salePrice_Prefix))
                        );
						SMonitor.Log($"Patched salePrice for {t.Name}");
                    }
					catch
                    {
					}
                }
            }
        }

        private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
			IsEnabled = false;
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
			IsEnabled = Config.ModEnabled && (Config.EnableGlobally || Game1.player?.modData.ContainsKey(modKey) == true);

            SHelper.GameContent.InvalidateCache("LooseSprites/Cursors");
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
		{
            if (!IsEnabled)
                return;
			if (e.NameWithoutLocale.IsEquivalentTo("LooseSprites/Cursors"))
			{
                e.Edit((IAssetData data) =>
				{
					var tex = SHelper.ModContent.Load<Texture2D>("assets/nomoney.png");
					data.AsImage().PatchImage(tex, null, new Rectangle(340, 471, 65, 18), PatchMode.Replace);
				});
			}
			else if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
			{
                e.Edit((IAssetData data) =>
				{
					foreach(var kvp in data.AsDictionary<string, ShopData>().Data)
					{
						foreach(var item in kvp.Value.Items)
						{
							item.Id = item.Id?.Replace(" @requirePrice", "").Replace("@requirePrice ", "").Replace("@requirePrice", "");
							item.ItemId = item.ItemId?.Replace(" @requirePrice", "").Replace("@requirePrice ", "").Replace("@requirePrice", "");
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
            }
		}
	}

}
