using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace SprinklerMod
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        public static string dictPath = "aedenthorn.SprinklerMod/dictionary";
        public static Dictionary<string, string> sprinklerDict = new();
        public static PerScreen<Dictionary<Object, ActiveSprinklerData>> activeSprinklers = new();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            activeSprinklers.Value = new();

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if(!Config.EnableMod || !Context.IsWorldReady)
            {
                activeSprinklers.Value.Clear();
                return;
            }
            foreach(var key in activeSprinklers.Value.Keys.ToArray())
            {
                if (activeSprinklers.Value[key].ticks < 60)
                {
                    activeSprinklers.Value[key].ticks++;
                    continue;
                }
                else
                {
                    foreach (Vector2 v2 in key.GetSprinklerTiles())
                    {
                        key.ApplySprinkler(activeSprinklers.Value[key].location, v2);
                    }
                    activeSprinklers.Value.Remove(key);
                }
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsPlayerFree || !Context.CanPlayerMove)
                return;
            if(e.Button == Config.SprinkleAllButton)
            {
                bool found = false;
                foreach(var obj in Game1.currentLocation.objects.Pairs)
                {
                    if (obj.Value.IsSprinkler())
                    {
                        ActivateSprinkler(obj.Value, Game1.currentLocation);
                        found = true;
                    }
                }
                if(found)
                {
                    Game1.currentLocation.playSound("bigSelect");
                }
            }
            else if(e.Button == Config.SprinkleButton && Game1.currentLocation.objects.TryGetValue(Game1.currentCursorTile, out var obj) && obj.IsSprinkler())
            {
                ActivateSprinkler(obj, Game1.currentLocation);
                Game1.currentLocation.playSound("bigSelect");
            }
        }
        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            sprinklerDict = Helper.GameContent.Load<Dictionary<string, string>>(dictPath);
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if(Config.EnableMod && e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                Dictionary<string, string> dict = new();
                foreach (var kvp in Config.SprinklerRadii)
                {
                    dict.Add(kvp.Key, kvp.Value+"");
                };
                e.LoadFrom(() => dict, StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
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
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Sprinkle Key",
                getValue: () => Config.SprinkleButton,
                setValue: value => Config.SprinkleButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Sprinkle All Key",
                getValue: () => Config.SprinkleAllButton,
                setValue: value => Config.SprinkleAllButton = value
            );

        }

    }
}
