using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace AdvancedDrumBlocks
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        private static Dictionary<string, Texture2D> textureDict = new Dictionary<string, Texture2D>();

        public static ModEntry context;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.EnableMod)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Input.MouseWheelScrolled += Input_MouseWheelScrolled;

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), nameof(Game1.pressSwitchToolButton)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Game1_pressSwitchToolButton_Prefix))
            );
        }

        private void Input_MouseWheelScrolled(object sender, StardewModdingAPI.Events.MouseWheelScrolledEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsPlayerFree || !Helper.Input.IsDown(Config.IndexModKey) || Game1.soundBank == null || !Game1.currentLocation.objects.TryGetValue(Game1.currentCursorTile, out Object obj) || !obj.Name.Equals("Drum Block"))
                return;

            int newIndex;
            if (e.Delta > 0)
            {
                newIndex = (obj.preservedParentSheetIndex.Value + 1) % 7;
            }
            else if (e.Delta < 0)
            {
                newIndex = obj.preservedParentSheetIndex.Value >= 1 ? obj.preservedParentSheetIndex.Value - 1 : 6;
            }
            else
                return;
            Monitor.Log($"Setting index to {newIndex}");
            Game1.currentLocation.objects[Game1.currentCursorTile].preservedParentSheetIndex.Value = newIndex;
            Config.CurrentPitch = newIndex;
            Game1.currentLocation.objects[Game1.currentCursorTile].internalSound?.Stop(AudioStopOptions.Immediate);
            Game1.currentLocation.objects[Game1.currentCursorTile].farmerAdjacentAction(Game1.currentLocation);
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
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_IndexModKey_Name"),
                getValue: () => Config.IndexModKey,
                setValue: value => Config.IndexModKey = value
            );
        }
    }
}
