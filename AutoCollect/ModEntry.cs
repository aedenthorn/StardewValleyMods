using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;

namespace AutoCollect
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static PerScreen<Vector2> lastTile = new();
        
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;

            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
            
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.CanPlayerMove)
                return;
            if (Game1.player.getTileLocation() != lastTile.Value)
            {
                lastTile.Value = Game1.player.getTileLocation();
                var tile = Game1.player.getTileLocation();
                foreach (var kvp in Game1.player.currentLocation.Objects.Pairs)
                {
                    if(kvp.Value.bigCraftable.Value && kvp.Value.heldObject.Value is not null && kvp.Value.readyForHarvest.Value && Math.Abs(kvp.Key.X - tile.X) <= Config.MaxDistance && Math.Abs(kvp.Key.Y - tile.Y) <= Config.MaxDistance)
                    {
                        kvp.Value.checkForAction(Game1.player);
                    }
                }
            }
        }


        private void Input_ButtonsChanged(object sender, StardewModdingAPI.Events.ButtonsChangedEventArgs e)
        {
            if(!Context.CanPlayerMove || !Config.ToggleKey.JustPressed()) 
                return;
            /*
            foreach (var obj in Game1.currentLocation.Objects.Values)
            {
                if (obj.MinutesUntilReady > 0)
                {
                    obj.minutesUntilReady.Value = 1;
                    //obj.DayUpdate(Game1.currentLocation);
                }
            }
            return;
            */
            Config.ModEnabled = !Config.ModEnabled;
            Monitor.Log($"Auto Collect Enabled: {Config.ModEnabled}", LogLevel.Info);
            Helper.WriteConfig(Config);
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
            

            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => "Toggle Key",
                getValue: () => Config.ToggleKey,
                setValue: value => Config.ToggleKey = value
            );
            
        }
    }
}