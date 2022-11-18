using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using xTile;
using xTile.Dimensions;

namespace Restauranteer
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        
        public static string orderKey = "aedenthorn.Restauranteer/order";
        public static Texture2D emoteSprite;
        public static PerScreen<Dictionary<string, int>> npcOrderNumbers = new PerScreen<Dictionary<string, int>>();
        public static NetRef<Chest> fridge;
        public static PerScreen<Location> fridgePosition = new PerScreen<Location>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            npcOrderNumbers.Value = new Dictionary<string, int>();
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            npcOrderNumbers.Value.Clear();
            fridge = new NetRef<Chest>(new Chest(true, 130));

            emoteSprite = SHelper.ModContent.Load<Texture2D>(Path.Combine("assets", "emote.png"));
        }

        private void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            if(Config.ModEnabled && Context.IsPlayerFree &&  Game1.player.currentLocation.Name == "Saloon" && Game1.player.eventsSeen.Contains(980558))
            {
                UpdateOrders();
            }
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Events/Farm"))
            {
                e.Edit(delegate (IAssetData data)
                {
                    var dict = data.AsDictionary<string, string>();
                    if(dict.Data.TryGetValue(Config.EventKey, out string str))
                    {
                        dict.Data[Config.EventKey] = str.Replace(Config.EventReplacePart, string.Format(Config.EventReplaceWith, Helper.Translation.Get("gus-event-string")));
                    }
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Maps/Saloon"))
            {
                e.Edit(delegate (IAssetData data)
                {
                    var map = data.AsMap();
                    if (Config.PatchSaloonMap)
                    {
                        map.PatchMap(Helper.ModContent.Load<Map>(Path.Combine("assets", "SaloonKitchen.tmx")), targetArea: new Microsoft.Xna.Framework.Rectangle(10, 12, 8, 5), patchMode: PatchMapMode.Replace);
                    }
                    foreach(var tile in Config.KitchenTiles)
                    {
                        try
                        {
                            map.Data.GetLayer("Buildings").Tiles[tile.X, tile.Y].Properties["Action"] = "kitchen";
                        }
                        catch { }
                    }
                    foreach(var tile in Config.FridgeTiles)
                    {
                        try
                        {
                            map.Data.GetLayer("Buildings").Tiles[tile.X, tile.Y].Properties["Action"] = "fridge";
                        }
                        catch { }
                    }
                }, StardewModdingAPI.Events.AssetEditPriority.Late);
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled",
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Auto Fill Fridge",
                getValue: () => Config.AuotFillFridge,
                setValue: value => Config.AuotFillFridge = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Reveal Gift Taste",
                getValue: () => Config.RevealGiftTaste,
                setValue: value => Config.RevealGiftTaste = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Reveal Gift Taste",
                getValue: () => Config.PatchSaloonMap,
                setValue: value => Config.PatchSaloonMap = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Order Chance / s",
                getValue: () => Config.OrderChance + "",
                setValue: delegate(string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float val)){ Config.OrderChance = val; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Loved Dish Order Chance",
                getValue: () => Config.LovedDishChance + "",
                setValue: delegate(string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float val)){ Config.LovedDishChance = val; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Price Multiplier",
                getValue: () => Config.PriceMarkup + "",
                setValue: delegate(string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float val)){ Config.PriceMarkup = val; } }
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Max NPC Orders Per Night",
                getValue: () => Config.MaxNPCOrdersPerNight,
                setValue: value => Config.MaxNPCOrdersPerNight = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Loved Friendship Change",
                getValue: () => Config.LovedFriendshipChange,
                setValue: value => Config.LovedFriendshipChange= value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Liked Friendship Change",
                getValue: () => Config.LikedFriendshipChange,
                setValue: value => Config.LikedFriendshipChange = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Fridge Mod Key",
                getValue: () => Config.FridgeModKey,
                setValue: value => Config.FridgeModKey = value
            );
        }
    }
}