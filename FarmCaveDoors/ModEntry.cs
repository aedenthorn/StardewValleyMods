using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;

namespace FarmCaveDoors
{
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static ICue buzz;

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;  

            //var harmony = new Harmony(ModManifest.UniqueID);
            //harmony.PatchAll();

        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo("Maps/FarmCave"))
            {
                e.Edit(delegate (IAssetData data)
                {
                    var mapData = data.AsMap();
                    var ts1 = Helper.GameContent.Load<Texture2D>("Maps/spring_outdoorsTileSheet");
                    var ts2 = Helper.GameContent.Load<Texture2D>("Maps/Mines/mine_desert");
                    mapData.Data.AddTileSheet(new xTile.Tiles.TileSheet("outdoors", mapData.Data, "Maps/spring_outdoorsTileSheet", new xTile.Dimensions.Size(ts1.Width, ts1.Height), new xTile.Dimensions.Size(16, 16)));
                    mapData.Data.AddTileSheet(new xTile.Tiles.TileSheet("skullCave", mapData.Data, "Maps/Mines/mine_desert", new xTile.Dimensions.Size(ts2.Width, ts2.Height), new xTile.Dimensions.Size(16, 16)));
                });
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

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Mine Door X",
                getValue: () => Config.MineDoorX,
                setValue: value => Config.MineDoorX = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Mine Door Y",
                getValue: () => Config.MineDoorY,
                setValue: value => Config.MineDoorY = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Skull Door X",
                getValue: () => Config.SkullDoorX,
                setValue: value => Config.SkullDoorX = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Skull Door Y",
                getValue: () => Config.SkullDoorY,
                setValue: value => Config.SkullDoorY = value
            );
        }
    }
}