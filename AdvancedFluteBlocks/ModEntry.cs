using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace AdvancedFluteBlocks
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
               original: AccessTools.Method(typeof(Object), nameof(Object.checkForAction)),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_FluteBlock_Transpiler))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.farmerAdjacentAction)),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_FluteBlock_Transpiler))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), nameof(Game1.pressSwitchToolButton)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Game1_pressSwitchToolButton_Prefix))
            );
        }
        public override object GetApi()
        {
            return new AdvancedFluteBlocksApi();
        }

        private void Input_MouseWheelScrolled(object sender, StardewModdingAPI.Events.MouseWheelScrolledEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsPlayerFree || Config.ToneList.Length == 0 || (!Helper.Input.IsDown(Config.ToneModKey) && !Helper.Input.IsDown(Config.PitchModKey)) || Game1.soundBank == null || !Game1.currentLocation.objects.TryGetValue(Game1.currentCursorTile, out Object obj) || !obj.Name.Equals("Flute Block"))
                return;

            if (Helper.Input.IsDown(Config.PitchModKey))
            {
                int newPitch;
                if (e.Delta > 0)
                {
                    newPitch = (obj.preservedParentSheetIndex.Value + Config.PitchStep) % 2400;
                }
                else if (e.Delta < 0)
                {
                    newPitch = obj.preservedParentSheetIndex.Value >= Config.PitchStep ? obj.preservedParentSheetIndex.Value - Config.PitchStep : 2400 - Config.PitchStep;
                }
                else
                    return;
                Monitor.Log($"Setting pitch to {newPitch}");
                Game1.currentLocation.objects[Game1.currentCursorTile].preservedParentSheetIndex.Value = newPitch;
                Config.CurrentPitch = newPitch;
                Game1.currentLocation.objects[Game1.currentCursorTile].internalSound?.Stop(AudioStopOptions.Immediate);
                Game1.currentLocation.objects[Game1.currentCursorTile].farmerAdjacentAction(Game1.currentLocation);
            }
            else
            {
                string[] tones = Config.ToneList.Split(',');
                obj.modData.TryGetValue("aedenthorn.AdvancedFluteBlocks/tone", out string tone);
                for (int i = 0; i < tones.Length; i++)
                {
                    if (tone == null || tone == tones[i])
                    {
                        string newTone = null;
                        if (e.Delta > 0)
                            newTone = tones[(i + 1) % tones.Length];
                        else if (e.Delta < 0)
                            newTone = tones[i > 0 ? i - 1 : tones.Length - 1];
                        else
                            return;
                        Monitor.Log($"Setting tone to {newTone}");
                        Game1.currentLocation.objects[Game1.currentCursorTile].modData["aedenthorn.AdvancedFluteBlocks/tone"] = newTone;
                        Config.CurrentTone = newTone;
                        Game1.currentLocation.objects[Game1.currentCursorTile].internalSound?.Stop(AudioStopOptions.Immediate);
                        Game1.currentLocation.objects[Game1.currentCursorTile].farmerAdjacentAction(Game1.currentLocation);
                        return;
                    }
                }
                Game1.currentLocation.objects[Game1.currentCursorTile].modData["aedenthorn.AdvancedFluteBlocks/tone"] = tones[0];
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
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => ModEntry.SHelper.Translation.Get("GMCM_SectionTitle_KeyBinds_Text")
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_PitchModKey_Name"),
                getValue: () => Config.PitchModKey,
                setValue: value => Config.PitchModKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ToneModKey_Name"),
                getValue: () => Config.ToneModKey,
                setValue: value => Config.ToneModKey = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_PitchStep_Name"),
                getValue: () => Config.PitchStep,
                setValue: value => Config.PitchStep = value
            );
        }
    }
}
