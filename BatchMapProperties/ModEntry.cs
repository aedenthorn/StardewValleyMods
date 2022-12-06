using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Layers;
using xTile.ObjectModel;
using static System.Net.Mime.MediaTypeNames;

namespace DynamicMapTiles
{
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        public static string dictPath = "aedenthorn.DynamicMapTiles/dictionary";
        public static Dictionary<string, List<PushedTile>> pushingDict = new Dictionary<string, List<PushedTile>>();
        public static Dictionary<string, DynamicTileInfo> dynamicDict = new Dictionary<string, DynamicTileInfo>();
        
        public static string addLayerKey = "DMT/addLayer";
        public static string addTilesheetKey = "DMT/addTilesheet";
        public static string changeIndexKey = "DMT/changeIndex";
        public static string changeMultipleIndexKey = "DMT/changeMultipleIndex";
        public static string changePropertiesKey = "DMT/changeProperties";
        public static string changeMultiplePropertiesKey = "DMT/changeMultipleProperties";
        public static string triggerKey = "DMT/trigger";
        public static string explodeKey = "DMT/explode";
        public static string explosionKey = "DMT/explosion";
        public static string pushKey = "DMT/push";
        public static string pushableKey = "DMT/pushable";
        public static string pushAlsoKey = "DMT/pushAlso";
        public static string pushOthersKey = "DMT/pushOthers";
        public static string soundKey = "DMT/sound";
        public static string teleportKey = "DMT/teleport";
        public static string teleportTileKey = "DMT/teleportTile";
        public static string giveKey = "DMT/give";
        public static string takeKey = "DMT/take";
        public static string chestKey = "DMT/chest";
        public static string chestAdvancedKey = "DMT/chestAdvanced";
        public static string messageKey = "DMT/message";
        public static string eventKey = "DMT/event";
        public static string mailKey = "DMT/mail";
        public static string mailRemoveKey = "DMT/mailRemove";
        public static string mailBoxKey = "DMT/mailbox";
        public static string invalidateKey = "DMT/invalidate";
        public static string musicKey = "DMT/music";
        public static string healthKey = "DMT/health";
        public static string staminaKey = "DMT/stamina";
        public static string healthPerSecondKey = "DMT/healthPerSecond";
        public static string staminaPerSecondKey = "DMT/staminaPerSecond";
        public static string buffKey = "DMT/buff";
        public static string speedKey = "DMT/speed";
        public static string moveKey = "DMT/move";
        public static string emoteKey = "DMT/emote";
        public static string animationKey = "DMT/animation";
        public static string slipperyKey = "DMT/slippery";

        public static List<string> actionKeys = new List<string>()
        {
            addLayerKey,
            addTilesheetKey,
            changeIndexKey,
            changeMultipleIndexKey,
            changePropertiesKey,
            changeMultiplePropertiesKey,
            explodeKey,
            explosionKey,
            pushKey,
            pushOthersKey,
            soundKey,
            teleportKey,
            teleportTileKey,
            giveKey,
            takeKey,
            chestKey,
            chestAdvancedKey,
            messageKey,
            eventKey,
            mailKey,
            mailRemoveKey,
            mailBoxKey,
            invalidateKey,
            musicKey,
            healthKey,
            staminaKey,
            healthPerSecondKey,
            staminaPerSecondKey,
            buffKey,
            speedKey,
            moveKey,
            emoteKey,
            animationKey
        };

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            Helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            var dict = Helper.GameContent.Load<Dictionary<string, DynamicTileInfo>>(dictPath);
            foreach(var info in dict.Values)
            {
                foreach(var loc in Game1.locations)
                {
                    if (info.locations is not null && !info.locations.Contains(loc.Name))
                        continue;
                    foreach(var layer in loc.Map.Layers)
                    {
                        if (info.layers is not null && !info.layers.Contains(layer.Id))
                            continue;
                        for (int x = 0; x < layer.Tiles.Array.GetLength(0); x++)
                        {
                            for (int y = 0; y < layer.Tiles.Array.GetLength(1); y++)
                            {
                                if (layer.Tiles[x, y] is not null)
                                {
                                    if(info.tileSheets is not null && !info.tileSheets.Contains(layer.Tiles[x, y].TileSheet.Id))
                                        continue;
                                    if(info.indexes is not null && !info.indexes.Contains(layer.Tiles[x, y].TileIndex))
                                        continue;
                                    foreach(var prop in info.properties)
                                    {

                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, DynamicTileInfo>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
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
            if(Config.IsDebug && e.Button == SButton.Delete)
            {
                return;
                Game1.player.IsEmoting = false;
                    var tile = Game1.currentLocation.Map.GetLayer("Buildings").Tiles[51, 86];
                if(tile != null)
                {
                    tile.Properties[pushKey] = "51 86,50 86,50 87,51 87";
                }
                tile = Game1.currentLocation.Map.GetLayer("Buildings").Tiles[47, 86];
                if(tile != null)
                {
                    tile.Properties[explodeKey] = "T";
                }
                tile = Game1.currentLocation.Map.GetLayer("Back").Tiles[47, 87];
                if(tile != null)
                {
                    tile.Properties[changeIndexKey] = "380";
                    tile.Properties[soundKey] = "slime";
                    tile.Properties[buffKey] = "13";
                    tile.Properties[messageKey] = "You have been slimed!";
                    //tile.Properties[teleportKey] = $"{30 * 64} {57 * 64}";
                    //tile.Properties.Remove(teleportKey);
                    //tile.Properties[eventKey] = "playful/20 57/Harvey 25 57 3/skippable/pause 200/speak Harvey \"Hey, why are you stepping there, @?\"/pause 500/end";
                }

                tile = Game1.currentLocation.Map.GetLayer("Back").Tiles[42, 86];
                if(tile != null)
                {
                    tile.Properties[changeMultipleIndexKey] = "Buildings 41 85=Landscape/1140|Buildings 43 85=Landscape/1141|Front 41 84=Landscape/1117|Front 43 84=Landscape/1118|Front 41 83=Landscape/1090|Front 43 83=Landscape/1091";
                    tile.Properties[changeMultiplePropertiesKey] = "Buildings,42,85,Action=kitchen";
                    tile.Properties[soundKey] = "shwip";
                }
                tile = Game1.currentLocation.Map.GetLayer("Back").Tiles[42, 91];
                if(tile != null)
                {
                    tile.Properties[teleportKey] = $"{47*64} {87 * 64}";
                }
                tile = Game1.currentLocation.Map.GetLayer("Back").Tiles[42, 91];
                if(tile != null)
                {
                    tile.Properties[teleportTileKey] = "51 87";
                    tile.Properties[soundKey] = "shwip";
                }
                tile = Game1.currentLocation.Map.GetLayer("Back").Tiles[51, 87];
                if(tile != null)
                {
                    tile.Properties[teleportTileKey] = "42 91";
                    tile.Properties[soundKey] = "shwip";
                }
                tile = Game1.currentLocation.Map.GetLayer("Back").Tiles[47, 88];
                if(tile != null)
                {
                    tile.Properties[healthPerSecondKey] = "-5";
                }
                tile = Game1.currentLocation.Map.GetLayer("Back").Tiles[42, 87];
                if(tile != null)
                {
                    tile.Properties[healthPerSecondKey] = "5";
                }
                tile = Game1.currentLocation.Map.GetLayer("Back").Tiles[51, 86];
                if(tile != null)
                {
                    tile.Properties[giveKey] = "Weapon/Rusty Sword";
                    tile.Properties[messageKey+"Once"] = "You found an old sword!";
                }
                for (int i = 0; i < 10; i++)
                {
                    tile = Game1.currentLocation.Map.GetLayer("Back").Tiles[42 + i, 95];
                    if (tile != null)
                    {
                        tile.Properties[moveKey] = "1 0";
                        if(i == 9)
                        {
                            tile.Properties[emoteKey] = "4";
                        }
                    }
                }
                for(int i = 0; i < 6; i++)
                {
                    tile = Game1.currentLocation.Map.GetLayer("Back").Tiles[47, 89 + i];
                    if (tile != null)
                    {
                        tile.Properties[speedKey] = "0.5";
                    }
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
                    tile.tile.Layer.Tiles[tile.position.X / 64, tile.position.Y / 64] = tile.tile;
                    foreach (var l in Game1.currentLocation.map.Layers)
                    {
                        List<string> actions = new List<string>();
                        var t = l.PickTile(new xTile.Dimensions.Location(tile.position.X, tile.position.Y), Game1.viewport.Size);
                        if (t is not null)
                        {
                            TriggerActions(new List<Layer>() { t.Layer }, tile.farmer, new Point(tile.position.X / 64, tile.position.Y / 64), new List<string>() { "Pushed" });
                        }
                    }
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
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Debug",
                getValue: () => Config.IsDebug,
                setValue: value => Config.IsDebug = value
            );
        }
    }
}