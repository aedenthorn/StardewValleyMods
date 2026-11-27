using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;

namespace CropHat
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static string seedKey = "aedenthorn.CropHat/seed";
        public static string daysKey = "aedenthorn.CropHat/days";
        public static string phaseKey = "aedenthorn.CropHat/phase";
        public static string phasesKey = "aedenthorn.CropHat/phases";
        public static string rowKey = "aedenthorn.CropHat/row";
        public static string grownKey = "aedenthorn.CropHat/grownKey";
        public static string xKey = "aedenthorn.CropHat/x";
        public static string yKey = "aedenthorn.CropHat/y";
        
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
            helper.Events.Display.RenderingWorld += Display_RenderingWorld;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if(Config.EnableMod && Context.CanPlayerMove && e.Button == Config.CheatButton)
            {
                Monitor.Log("Pressed key");
                NewDay(Game1.player.hat.Value);
            }
        }

        private void Display_RenderingWorld(object sender, RenderingWorldEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (Config.AllowOthersToPick)
            {
                foreach (Farmer farmer in Game1.getAllFarmers())
                {
                    var loc = farmer.Position + new Vector2(32, -88);
                    if (Game1.player.currentLocation == farmer.currentLocation && farmer.hat.Value is not null && Vector2.Distance(Game1.GlobalToLocal(loc), Game1.getMousePosition().ToVector2()) < 32 && farmer.hat.Value.modData.ContainsKey(seedKey))
                    {
                        if (ReadyToHarvest(farmer.hat.Value))
                        {
                            Game1.mouseCursor = 6;
                            if (!Utility.withinRadiusOfPlayer((int)farmer.Position.X, (int)farmer.Position.Y, 1, Game1.player))
                            {
                                Game1.mouseCursorTransparency = 0.5f;
                            }
                        }
                    }
                }
            }
            else
            {
                var loc = Game1.player.Position + new Vector2(32, -88);
                if (Vector2.Distance(Game1.GlobalToLocal(loc), Game1.getMousePosition().ToVector2()) < 32 && Game1.player.hat.Value is not null && Game1.player.hat.Value.modData.TryGetValue(phaseKey, out string phaseString))
                {
                    if (ReadyToHarvest(Game1.player.hat.Value))
                    {
                        Game1.mouseCursor = 6;
                    }
                }

            }
        }

        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            if (Game1.player?.hat?.Value?.modData.ContainsKey(seedKey) == true)
            {
                NewDay(Game1.player.hat.Value);
            }
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
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_AllowOthersToPick_Name"),
                tooltip: () => ModEntry.SHelper.Translation.Get("GMCM_Option_AllowOthersToPick_Tooltip"),
                getValue: () => Config.AllowOthersToPick,
                setValue: value => Config.AllowOthersToPick = value
            );
        }
    }
}