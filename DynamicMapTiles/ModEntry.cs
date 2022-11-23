using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using xTile.ObjectModel;
using static System.Net.Mime.MediaTypeNames;

namespace DynamicMapTiles
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        public static Dictionary<string, List<PushedTile>> pushingDict = new Dictionary<string, List<PushedTile>>();
        
        public static string changeIndexKey = "DMT/changeIndex";
        public static string changeIndexOffKey = "DMT/changeIndexOff";
        public static string changeMultipleIndexKey = "DMT/changeMultipleIndex";
        public static string changeMultipleIndexOffKey = "DMT/changeMultipleIndexOff";
        public static string changePropertiesKey = "DMT/changeProperties";
        public static string changePropertiesOffKey = "DMT/changePropertiesOff";
        public static string changeMultiplePropertiesKey = "DMT/changeMultipleProperties";
        public static string changeMultiplePropertiesOffKey = "DMT/changeMultiplePropertiesOff";
        public static string explodeKey = "DMT/explode";
        public static string pushKey = "DMT/push";
        public static string pushSoundKey = "DMT/pushSound";
        public static string pushedKey = "DMT/pushed";
        public static string soundKey = "DMT/sound";
        public static string soundOnceKey = "DMT/soundOnce";
        public static string soundOffKey = "DMT/soundOff";
        public static string soundOffOnceKey = "DMT/soundOffOnce";
        public static string teleportKey = "DMT/teleport";
        public static string giveKey = "DMT/give";
        public static string messageKey = "DMT/message";
        public static string messageOnceKey = "DMT/messageOnce";
        public static string eventKey = "DMT/event";
        public static string eventOnceKey = "DMT/eventOnce";
        public static string mailKey = "DMT/mail";
        public static string musicKey = "DMT/music";
        public static string healthKey = "DMT/health";
        public static string staminaKey = "DMT/stamina";
        public static string healthPerSecondKey = "DMT/healthPerSecond";
        public static string staminaPerSecondKey = "DMT/staminaPerSecond";
        public static string buffKey = "DMT/buff";
        public static string speedKey = "DMT/speed";

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            Helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }
        public override object GetApi()
        {
            return new DynamicMapTilesApi();
        }
        private void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            if(!Config.ModEnabled || !Context.IsPlayerFree)
                return;
            var center = Game1.player.GetBoundingBox().Center;
            var tile = Game1.player.currentLocation.Map.GetLayer("Back").PickTile(new xTile.Dimensions.Location(center.X, center.Y), Game1.viewport.Size);
            if (tile is null)
                return;
            PropertyValue value;
            int number;
            if(tile.Properties.TryGetValue(healthPerSecondKey, out value) && int.TryParse(value, out number)){
                if(number < 0)
                {
                    Game1.player.takeDamage(Math.Abs(number), false, null);

                }
                else
                {
                    Game1.player.health = Math.Min(Game1.player.health + number, Game1.player.maxHealth);
                    Game1.player.currentLocation.debris.Add(new Debris(number, new Vector2((float)(Game1.player.getStandingX() + 8), (float)Game1.player.getStandingY()), Color.LimeGreen, 1f, Game1.player));
                }
            }
            if (tile.Properties.TryGetValue(staminaPerSecondKey, out value) && int.TryParse(value, out number))
            {
                Game1.player.Stamina += number;
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if(e.Button == SButton.Delete)
            {
                var tile = Game1.currentLocation.Map.GetLayer("Buildings").Tiles[51, 86];
                if(tile != null)
                {
                    tile.Properties[explodeKey] = "T";
                    tile.Properties[pushKey] = "3";
                    tile.Properties[pushedKey] = "1";
                }
                tile = Game1.currentLocation.Map.GetLayer("Back").Tiles[20, 57];
                if(tile != null)
                {
                    tile.Properties[changeIndexKey] = "380";
                    tile.Properties[soundKey] = "shwip";
                    tile.Properties[soundOffKey] = "shwip";
                    tile.Properties[messageKey] = "You found a pair of purple shorts!";
                    tile.Properties[giveKey] = "789";
                    //tile.Properties[teleportKey] = $"{30 * 64} {57 * 64}";
                    //tile.Properties.Remove(teleportKey);
                    tile.Properties.Remove(eventKey);
                    //tile.Properties[eventKey] = "playful/20 57/Harvey 25 57 3/skippable/pause 200/speak Harvey \"Hey, why are you stepping there, @?\"/pause 500/end";
                }
                tile = Game1.currentLocation.Map.GetLayer("Back").Tiles[26, 59];
                if(tile != null)
                {
                    tile.Properties[changeMultipleIndexKey] = "26 59 1064,25 59 1064,27 59 1064,26 58 1064,25 58 1064,27 58 1064,26 60 1064,25 60 1064,27 60 1064";
                    tile.Properties[changeMultipleIndexOffKey] = "26 59 736,25 59 736,27 59 736,26 58 736,25 58 736,27 58 736,26 60 736,25 60 736,27 60 736";
                    tile.Properties[soundKey] = "shwip";
                    tile.Properties[soundOffKey] = "shwip";
                }
                tile = Game1.currentLocation.Map.GetLayer("Back").Tiles[22, 57];
                if(tile != null)
                {
                    tile.Properties[soundKey] = "slime";
                    tile.Properties[buffKey] = "13";
                    tile.Properties[messageKey] = "You have been slimed!";
                }
                tile = Game1.currentLocation.Map.GetLayer("Back").Tiles[20, 58];
                if(tile != null)
                {
                    tile.Properties[healthPerSecondKey] = "-5";
                }
                tile = Game1.currentLocation.Map.GetLayer("Back").Tiles[19, 58];
                if(tile != null)
                {
                    tile.Properties[healthPerSecondKey] = "5";
                }
                tile = Game1.currentLocation.Map.GetLayer("Back").Tiles[25, 57];
                if(tile != null)
                {
                    tile.Properties[speedKey] = "5";
                }
                tile = Game1.currentLocation.Map.GetLayer("Back").Tiles[27, 57];
                if(tile != null)
                {
                    tile.Properties[speedKey] = "0.5";
                }
            }
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree || !pushingDict.TryGetValue(Game1.currentLocation.Name, out List<PushedTile> tiles))
                return;
            for(int i = tiles.Count - 1; i >= 0; i--)
            {
                var tile = tiles[i];
                tile.position += GetNextTile(tile.dir);
                if(tile.position.X % 64 == 0 && tile.position.Y % 64 == 0)
                {
                    var pushed = tile.tile.Properties[pushedKey];
                    tile.tile.Properties[pushedKey] = tile.tile.Properties[pushKey];
                    tile.tile.Properties[pushKey] = pushed;

                    Game1.currentLocation.Map.GetLayer("Buildings").Tiles[tile.position.X / 64, tile.position.Y / 64] = tile.tile;
                    tiles.RemoveAt(i);
                }
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
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
        }
    }
}