using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
using System.IO;

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
        public static NetRef<Chest> fridge;

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

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
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
                    dict.Data["980558/t 600 1130/w sunny/f Gus 1250"] = dict.Data["980558/t 600 1130/w sunny/f Gus 1250"].Replace("\"/pause 500/end", $"{Helper.Translation.Get("gus-event-string")}\"/pause 500/end");
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Maps/Saloon"))
            {
                e.Edit(delegate (IAssetData data)
                {
                    var map = data.AsMap();
                    map.Data.GetLayer("Buildings").Tiles[14, 16].Properties["Action"] = "kitchen";
                    map.Data.GetLayer("Buildings").Tiles[15, 16].Properties["Action"] = "kitchen";
                    map.Data.GetLayer("Buildings").Tiles[17, 16].Properties["Action"] = "fridge";
                    map.Data.GetLayer("Buildings").Tiles[18, 16].Properties["Action"] = "fridge";
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
        }
    }
}