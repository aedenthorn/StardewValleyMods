using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PersonalTravellingCart
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static string mapAssetKey;
        public static string dataPath = "aedenthorn.PersonalTravellingCart/dictionary";
        public static string cartKey = "aedenthorn.PersonalTravellingCart/whichCart";
        public static string locKey = "aedenthorn.PersonalTravellingCart/loc";
        public static string parkedKey = "aedenthorn.PersonalTravellingCart/parked";
        public static string locPrefix = "PersonalCart";
        public static string defaultKey = "_default";
        public static Dictionary<string, PersonalCartData> cartDict = new Dictionary<string, PersonalCartData>();
        private static bool skip;
        private static bool drawingExterior;
        private static ParkedCart clickableCart;

        private static string cartLocationFilePath;
        private static string thisPlayerCartLocation;

        private static GameTime deltaTime;
        private static RenderTarget2D screen;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            cartLocationFilePath = Path.Combine(Helper.DirectoryPath, "this_player_cart_location.txt");

            if (File.Exists(cartLocationFilePath))
            {
                thisPlayerCartLocation = File.ReadAllText(cartLocationFilePath);
            }
            else
            {
                if (Config.ThisPlayerCartLocationName is null)
                {
                    thisPlayerCartLocation = locPrefix + Guid.NewGuid().ToString("N");
                }
                else
                {
                    thisPlayerCartLocation = Config.ThisPlayerCartLocationName;
                    Monitor.Log($"Got cart location from config {thisPlayerCartLocation}");
                }
                File.WriteAllText(cartLocationFilePath, thisPlayerCartLocation);
            }
            Monitor.Log($"Saved cart location {thisPlayerCartLocation} to {cartLocationFilePath}");

            context = this;


            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            if (!Config.ModEnabled || !Game1.player.modData.ContainsKey(parkedKey))
                return;
            foreach(var l in Game1.locations)
            {
                if (!l.modData.TryGetValue(parkedKey, out string parkedString))
                    continue;
                List<ParkedCart> carts = JsonConvert.DeserializeObject<List<ParkedCart>>(parkedString);
                for (int i = 0; i < carts.Count; i++)
                {
                    var cart = carts[i];
                    if (cart.location == thisPlayerCartLocation)
                    {
                        SMonitor.Log($"Found player cart in {l}");
                        return;
                    }
                }
            }
            Game1.player.modData.Remove(parkedKey);
        }


        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree || !Game1.player.isRidingHorse() || !Game1.player.modData.TryGetValue(cartKey, out string which) || !cartDict.TryGetValue(which, out PersonalCartData data))
                return;

            if (e.Button == Config.HitchButton)
            {
                var hasParked = Game1.player.currentLocation.modData.TryGetValue(parkedKey, out string parkedString);
                List<ParkedCart> carts = hasParked ? JsonConvert.DeserializeObject<List<ParkedCart>>(parkedString) : new List<ParkedCart>();

                if (!Game1.player.modData.ContainsKey(parkedKey))
                {
                    carts.Add(new ParkedCart() { facing = Game1.player.FacingDirection, location = thisPlayerCartLocation, whichCart = which, position = Game1.player.Position });
                    Game1.player.modData[parkedKey] = "true";
                    SMonitor.Log($"Parked player cart in {Game1.player.currentLocation}");
                }
                else if (hasParked)
                {
                    for (int i = 0; i < carts.Count; i++)
                    {
                        var cart = carts[i];
                        if (cart.location != thisPlayerCartLocation)
                            continue;

                        var cdata = GetCartData(cart.whichCart);
                        var cddata = cdata.GetDirectionData(cart.facing);
                        Rectangle box = new Rectangle(Utility.Vector2ToPoint(cart.position + cddata.cartOffset) + new Point(cddata.hitchRect.Location.X * 4, cddata.hitchRect.Location.Y * 4 + 64), new Point(cddata.hitchRect.Size.X * 4, cddata.hitchRect.Size.Y * 4));
                        Rectangle horseBox = Game1.player.mount.GetBoundingBox();
                        if (box.Intersects(horseBox))
                        {
                            SMonitor.Log($"Hitching to cart in {Game1.player.currentLocation}");
                            Game1.player.Position = cart.position;
                            Game1.player.faceDirection(cart.facing);
                            Game1.player.modData.Remove(parkedKey);
                            carts.RemoveAt(i);
                            break;
                        }
                    }
                }
                else return;

                for (int i = 0; i < carts.Count; i++)
                {
                    carts[i].data = null;
                }
                Game1.player.currentLocation.modData[parkedKey] = JsonConvert.SerializeObject(carts);
                return;
            }


            if (e.Button == SButton.PageUp)
            {
                var keys = cartDict.Keys.ToList();
                var idx = keys.IndexOf(which);
                idx--;
                if(idx < 0)
                    idx = keys.Count - 1;
                SMonitor.Log($"Switching cart to {keys[idx]}");
                var loc = Game1.getLocationFromName(Game1.player.modData[locKey]);
                loc.mapPath.Value = cartDict[keys[idx]].mapPath;
                Game1.player.modData[cartKey] = keys[idx];
                return;
            }
            if (e.Button == SButton.PageDown)
            {
                var keys = cartDict.Keys.ToList();
                var idx = keys.IndexOf(which);
                idx++;
                idx %= keys.Count;
                SMonitor.Log($"Switching cart to {keys[idx]}");
                var loc = Game1.getLocationFromName(Game1.player.modData[locKey]);
                loc.mapPath.Value = cartDict[keys[idx]].mapPath;
                Game1.player.modData[cartKey] = keys[idx];
                return;
            }
            if (!Config.Debug)
                return;
            DirectionData ddata = GetDirectionData(data, Game1.player.FacingDirection);
            if (e.Button == SButton.F5)
            {
                SMonitor.Log("Saving to json file");
                File.WriteAllText(Path.Combine(Helper.DirectoryPath, "assets", "cart_data.json"), JsonConvert.SerializeObject(cartDict, Formatting.Indented));
                return;
            }
            else if(e.Button == SButton.F7)
            {
                SMonitor.Log("Loading from json file");
                string jsonPath = Path.Combine(Helper.DirectoryPath, "assets", "cart_data.json");
                if (!File.Exists(jsonPath))
                {
                    SMonitor.Log("File not found");
                    return;
                }
                var tex = cartDict[which].spriteSheet;
                cartDict = JsonConvert.DeserializeObject<Dictionary<string, PersonalCartData>>(File.ReadAllText(jsonPath));
                cartDict[which].spriteSheet = tex;
                return;
            }
            int mult = Helper.Input.IsDown(SButton.LeftAlt) ? 4 : 1;
            if (Helper.Input.IsDown(SButton.LeftShift))
            {
                switch (e.Button)
                {
                    case SButton.Left:
                        ddata.playerOffset += new Vector2(1 * mult, 0);
                        break;
                    case SButton.Right:
                        ddata.playerOffset -= new Vector2(1 * mult, 0);
                        break;
                    case SButton.Up:
                        ddata.playerOffset += new Vector2(0, 4 * mult);
                        break;
                    case SButton.Down:
                        ddata.playerOffset -= new Vector2(0, 4 * mult);
                        break;
                    default:
                        return;
                }
            }
            else
            {
                switch (e.Button)
                {
                    case SButton.Left:
                        ddata.cartOffset -= new Vector2(4 * mult, 0);
                        break;
                    case SButton.Right:
                        ddata.cartOffset += new Vector2(4 * mult, 0);
                        break;
                    case SButton.Up:
                        ddata.cartOffset -= new Vector2(0, 4 * mult);
                        break;
                    case SButton.Down:
                        ddata.cartOffset += new Vector2(0, 4 * mult);
                        break;
                    default:
                        return;
                }
            }
            switch (Game1.player.FacingDirection)
            {
                case 0:
                    data.up = ddata;
                    break;
                case 1:
                    data.right = ddata;
                    break;
                case 2:
                    data.down = ddata;
                    break;
                case 3:
                    data.left = ddata;
                    break;
            }
            cartDict[which] = data;
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(dataPath))
            {
                e.LoadFrom(() => new Dictionary<string, PersonalCartData>(), AssetLoadPriority.Exclusive);
            }
        }
        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            Game1.player.modData[locKey] = thisPlayerCartLocation;
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
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
                name: () => "Draw Cart Exterior",
                getValue: () => Config.DrawCartExterior,
                setValue: value => Config.DrawCartExterior = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Draw Cart Exterior Weather",
                getValue: () => Config.DrawCartExteriorWeather,
                setValue: value => Config.DrawCartExteriorWeather = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Debug",
                getValue: () => Config.Debug,
                setValue: value => Config.Debug = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Hitch Button",
                getValue: () => Config.HitchButton,
                setValue: value => Config.HitchButton = value
            );
        }
    }
}