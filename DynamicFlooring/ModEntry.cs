using HarmonyLib;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Linq;

namespace DynamicFlooring
{
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static string flooringKey = "aedenthorn.DynamicFlooring/flooring";
        
        public static PerScreen<Vector2?> startTile = new PerScreen<Vector2?>();
        public static PerScreen<bool> drawingTiles = new(); 

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.Input.ButtonReleased += Input_ButtonReleased;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
            
            
        }

        private void Input_ButtonReleased(object sender, StardewModdingAPI.Events.ButtonReleasedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.Button == Config.ModButton)
            {
                drawingTiles.Value = false;
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree)
                return;
            if(e.Button == Config.PlaceButton && Helper.Input.IsDown(Config.ModButton) && Game1.player.ActiveObject is Wallpaper)
            {
                drawingTiles.Value = true;
                startTile.Value = Game1.currentCursorTile;
                Helper.Input.Suppress(e.Button);
            }
            else if(e.Button == Config.RemoveButton && Game1.player.currentLocation.modData.TryGetValue(flooringKey, out string listString))
            {
                var point = Utility.Vector2ToPoint(Game1.currentCursorTile);
                var list = JsonConvert.DeserializeObject<List<FlooringData>>(listString);
                if (list.Any())
                {
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        if (list[i].area.Contains(point))
                        {
                            list.RemoveAt(i);
                            Game1.currentLocation.modData[flooringKey] = JsonConvert.SerializeObject(list);
                            Game1.currentLocation.loadMap(Game1.currentLocation.mapPath.Value, true);
                            Game1.currentLocation.resetForPlayerEntry();
                            if (Game1.currentLocation is DecoratableLocation)
                            {
                                (Game1.currentLocation as DecoratableLocation).ReadWallpaperAndFloorTileData();
                            }
                            UpdateFloor(Game1.currentLocation, list);
                            Helper.Input.Suppress(e.Button);
                            break;
                        }
                    }
                }
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
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Mod Button",
                tooltip: () => "Hold to enable using the place button",
                getValue: () => Config.ModButton,
                setValue: value => Config.ModButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Place Button",
                getValue: () => Config.PlaceButton,
                setValue: value => Config.PlaceButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Ignore Button",
                tooltip: () => "Hold to ignore floor placing restrictions",
                getValue: () => Config.IgnoreButton,
                setValue: value => Config.IgnoreButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Remove Button",
                getValue: () => Config.RemoveButton,
                setValue: value => Config.RemoveButton = value
            );
        }
    }
}