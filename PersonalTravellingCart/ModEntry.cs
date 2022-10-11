using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
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
        public static string lastXKey = "aedenthorn.PersonalTravellingCart/lastX";
        public static string lastYKey = "aedenthorn.PersonalTravellingCart/lastY";
        public static string lastLocationKey = "aedenthorn.PersonalTravellingCart/lastLocation";
        public static Dictionary<string, PersonalCartData> cartDict = new Dictionary<string, PersonalCartData>();
        private static PerScreen<string> locationName = new PerScreen<string>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.ModEnabled)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree || !Game1.player.isRidingHorse() || cartDict.Count == 0 || !cartDict.TryGetValue(Config.CurrentCart, out PersonalCartData data))
                return;
            if (e.Button == SButton.PageUp)
            {
                var keys = cartDict.Keys.ToList();
                var idx = keys.IndexOf(Config.CurrentCart);
                idx--;
                if(idx < 0)
                    idx = keys.Count - 1;
                SMonitor.Log($"Switching cart to {keys[idx]}");
                Config.CurrentCart = keys[idx];
                Helper.WriteConfig(Config);
                return;
            }
            if (e.Button == SButton.PageDown)
            {
                var keys = cartDict.Keys.ToList();
                var idx = keys.IndexOf(Config.CurrentCart);
                idx++;
                idx %= keys.Count;
                SMonitor.Log($"Switching cart to {keys[idx]}");
                Config.CurrentCart = keys[idx];
                Helper.WriteConfig(Config);
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
                var tex = cartDict[Config.CurrentCart].spriteSheet;
                cartDict = JsonConvert.DeserializeObject<Dictionary<string, PersonalCartData>>(File.ReadAllText(jsonPath));
                cartDict[Config.CurrentCart].spriteSheet = tex;
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
            cartDict[Config.CurrentCart] = data;
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
        }
    }
}