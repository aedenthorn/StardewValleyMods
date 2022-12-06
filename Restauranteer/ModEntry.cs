using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using xTile;
using xTile.Dimensions;
using xTile.ObjectModel;
using Object = StardewValley.Object;

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
        public static string fridgeKey = "aedenthorn.Restauranteer/fridge";
        public static Texture2D emoteSprite;
        public static Vector2 fridgeHideTile = new Vector2(-42000, -42000);
        public static PerScreen<Dictionary<string, int>> npcOrderNumbers = new PerScreen<Dictionary<string, int>>();
        public static Dictionary<string, NetRef<Chest>> fridgeDict = new();
        private Harmony harmony;

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

            harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            npcOrderNumbers.Value = new Dictionary<string, int>();
        }


        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            fridgeDict.Clear();
            npcOrderNumbers.Value.Clear();
            emoteSprite = SHelper.ModContent.Load<Texture2D>(Path.Combine("assets", "emote.png"));
            foreach (var name in Config.RestaurantLocations)
            {
                var l = Game1.getLocationFromName(name);
                if (l is not null && l is not FarmHouse)
                {
                    for (int x = 0; x < l.Map.GetLayer("Buildings").Tiles.Array.GetLength(0); x++)
                    {
                        for (int y = 0; y < l.Map.GetLayer("Buildings").Tiles.Array.GetLength(1); y++)
                        {
                            if (l.Map.GetLayer("Buildings").Tiles[x, y] is not null && l.Map.GetLayer("Buildings").Tiles[x, y].Properties.TryGetValue("Action", out PropertyValue p) && (p == "fridge" || p == "DropBox GusFridge"))
                            {
                                Vector2 v = new Vector2(x, y);
                                if (Config.AddFridgeObjects && !l.objects.TryGetValue(v, out Object obj))
                                {
                                    Chest fridge = new Chest(216, v, 217, 2)
                                    {
                                        shakeTimer = 50
                                    };
                                    fridge.modData[fridgeKey] = "true";
                                    fridge.fridge.Value = true;
                                    l.objects[v] = fridge;
                                }
                                else if (!Config.AddFridgeObjects && l.objects.TryGetValue(v, out obj) && obj.modData.ContainsKey(fridgeKey))
                                {
                                    l.objects.Remove(v);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            if(Config.ModEnabled && Context.IsPlayerFree && Config.RestaurantLocations.Contains(Game1.player.currentLocation.Name) && (!Config.RequireEvent || Game1.player.eventsSeen.Contains(980558)))
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
            else if (e.NameWithoutLocale.IsEquivalentTo("Maps/Saloon"))
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
                }, StardewModdingAPI.Events.AssetEditPriority.Late);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Characters/schedules/Emily") && !string.IsNullOrEmpty(Config.EmilySaloonString))
            {
                e.Edit(delegate (IAssetData data)
                {
                    var dict = data.AsDictionary<string, string>();
                    Regex ex = new Regex(@"Saloon [0-9]+ [0-9]+[^/]*", RegexOptions.Compiled);
                    Monitor.Log($"Replacing Emily saloon string with {Config.EmilySaloonString}");
                    foreach (var key in dict.Data.Keys)
                    {
                        dict.Data[key] = ex.Replace(dict.Data[key], Config.EmilySaloonString);
                    }
                }, StardewModdingAPI.Events.AssetEditPriority.Late);
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {

            object obj = Helper.ModRegistry.GetApi("blueberry.LoveOfCooking");
            if (obj is not null)
            {
                harmony.Patch( 
                    original: AccessTools.Constructor(obj.GetType().Assembly.GetType("LoveOfCooking.Objects.CookingMenu"), new Type[] { typeof(List<CraftingRecipe>), typeof(List<Chest>), typeof(string)  }),
                    prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.LoveOfCooking_CookingMenu_Prefix))
                );
            }

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
                name: () => "Require Event for Saloon",
                getValue: () => Config.RequireEvent,
                setValue: value => Config.RequireEvent = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Auto Fill Fridge",
                getValue: () => Config.AutoFillFridge,
                setValue: value => Config.AutoFillFridge = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Add Fridge Objects",
                getValue: () => Config.AddFridgeObjects,
                setValue: value => Config.AddFridgeObjects = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Reveal Gift Taste",
                getValue: () => Config.RevealGiftTaste,
                setValue: value => Config.RevealGiftTaste = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Sell Current Recipes",
                getValue: () => Config.SellCurrentRecipes,
                setValue: value => Config.SellCurrentRecipes = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Patch Saloon Map",
                getValue: () => Config.PatchSaloonMap,
                setValue: value => Config.PatchSaloonMap = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Show Dish Name Key",
                getValue: () => Config.ModKey,
                setValue: value => Config.ModKey = value
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
        }

        private static void LoveOfCooking_CookingMenu_Prefix(ref List<Chest> materialContainers)
        {
            if (!Config.ModEnabled || !Config.RestaurantLocations.Contains(Game1.currentLocation.Name))
                return;
            var fridge = GetFridge(Game1.currentLocation);
            if(materialContainers is null)
                materialContainers = new List<Chest>();
            materialContainers.Add( fridge );
        }
    }
}