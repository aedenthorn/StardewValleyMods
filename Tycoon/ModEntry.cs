using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Minecarts;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;

namespace Tycoon
{
	public partial class ModEntry : Mod
	{
		public static IMonitor SMonitor;
		public static IModHelper SHelper;
		public static IManifest SModManifest;
		public static ModConfig Config;
		public static ModEntry context;
        public static string dictPath = "aedenthorn.Tycoon/dict";
        public static string deedPath = "aedenthorn.Tycoon_deed";
        public static Dictionary<string, bool> ownedProperties;
        public static PerScreen<Vector2> lastMouseTile = new PerScreen<Vector2>();
        public static PerScreen<bool> hovering = new PerScreen<bool>();
        public static Dictionary<string, TycoonData> dataDict 
        {
            get
            {
                return SHelper.GameContent.Load<Dictionary<string, TycoonData>>(dictPath);
            }
        }

        public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.Saving += GameLoop_Saving;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.Display.RenderedWorld += Display_RenderedWorld;

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            Harmony harmony = new Harmony(ModManifest.UniqueID);
			harmony.PatchAll();
        }

        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree)
                return;
            if (Game1.currentCursorTile == lastMouseTile.Value && !hovering.Value)
                return;
            lastMouseTile.Value = Game1.currentCursorTile;
            var mousePos = new Point(Game1.getMousePosition().X + Game1.viewport.X, Game1.getMousePosition().Y + Game1.viewport.Y);
            foreach (var f in Game1.currentLocation.furniture)
            {
                if (f.ItemId.StartsWith(SHelper.ModRegistry.ModID))
                {
                    var a = f.boundingBox.Value.Contains(mousePos);
                    var name = f.ItemId.Substring(SHelper.ModRegistry.ModID.Length + 1);
                    var b = dataDict.TryGetValue(name, out var data);
                    if (a && b)
                    {
                        hovering.Value = true;
                        IClickableMenu.drawToolTip(e.SpriteBatch, data.Description ?? string.Format(SHelper.Translation.Get("deed-description-x"), data.Name), data.Name, null);
                        return;
                    }
                }
            }
            hovering.Value = false;
        }

        private void GameLoop_Saving(object sender, StardewModdingAPI.Events.SavingEventArgs e)
        {
            if (Config.ModEnabled && Context.IsMainPlayer)
            {
                Helper.Data.WriteSaveData("owned-properties", ownedProperties);
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            if (Config.ModEnabled && Context.IsMainPlayer)
            {
                ownedProperties = Helper.Data.ReadSaveData<Dictionary<string, bool>>("owned-properties") ?? new();
            }
            Helper.GameContent.InvalidateCache("Data/Shops");
            Helper.GameContent.InvalidateCache("Data/Minecarts");
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, TycoonData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Minecarts"))
            {
                e.Edit((IAssetData data) =>
                {
                    var dict = data.AsDictionary<string, MinecartNetworkData>().Data;
                    foreach (var kvp in dataDict)
                    {
                        if (ownedProperties.TryGetValue(kvp.Key, out var b) && b && dict.TryGetValue(kvp.Value.Network, out var network) && kvp.Value.MinecartData != null)
                        {
                            network.Destinations.Add(kvp.Value.MinecartData);
                        }
                    }
                });
            }
            else if (Config.ModEnabled && e.NameWithoutLocale.IsEquivalentTo("Data/Furniture"))
            {
                e.Edit((IAssetData data) =>
                {
                    var dict = data.AsDictionary<string, string>().Data;
                    foreach (var kvp in dataDict)
                    {
                        dict[$"aedenthorn.Tycoon_{kvp.Key}"] = $"{$"aedenthorn.Tycoon_{kvp.Key}"}/painting/1 1/1 2/1/0/-1/{string.Format(SHelper.Translation.Get("deed-x"), kvp.Value.Name)}/0/{kvp.Value.DeedPath ?? deedPath}";
                    }
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
            {
                e.Edit((IAssetData data) =>
                {
                    var dict = data.AsDictionary<string, ShopData>().Data;
                    foreach (var kvp in dataDict)
                    {
                        if ((!ownedProperties.TryGetValue(kvp.Key, out var b) || !b) && dict.TryGetValue(kvp.Value.Shop, out var shopData))
                        {
                            shopData.Items.Add(new ShopItemData()
                            {
                                Id = $"aedenthorn.Tycoon_{kvp.Key}",
                                ItemId = $"aedenthorn.Tycoon_{kvp.Key}",
                                MaxItems = 1,
                                AvailableStock = 1,
                                AvailableStockLimit = LimitedStockMode.Global,
                                Price = kvp.Value.Price,
                            });
                        }
                    }
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(deedPath))
            {
                e.LoadFromModFile<Texture2D>("assets/deed.png", StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
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
