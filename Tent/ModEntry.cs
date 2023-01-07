using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System.Collections.Generic;

namespace Tent
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        private static IDynamicGameAssetsApi apiDGA;

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
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.GameLocation_checkAction_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.GameLocation_answerDialogueAction_Prefix))
            );
            
            harmony.Patch(
               original: AccessTools.Method(typeof(BedFurniture), nameof(BedFurniture.ApplyWakeUpPosition)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.BedFurniture_ApplyWakeUpPosition_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.clearActiveMines)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.MineShaft_clearActiveMines_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(VolcanoDungeon), nameof(VolcanoDungeon.ClearAllLevels)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.VolcanoDungeon_ClearAllLevels_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.draw), new System.Type[] {typeof(SpriteBatch) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Farmer_draw_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(SaveGameMenu), nameof(SaveGameMenu.draw), new System.Type[] {typeof(SpriteBatch) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SaveGameMenu_draw_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(SaveGameMenu), nameof(SaveGameMenu.update)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SaveGameMenu_update_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Constructor(typeof(SaveGameMenu)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SaveGameMenu_Prefix))
            );


        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            if (isTenting)
            {
                Helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
                Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            isTenting = false;
            Helper.Events.Input.ButtonPressed -= Input_ButtonPressed;

        }

        private void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            isTenting = false;
            Helper.Events.GameLoop.TimeChanged -= GameLoop_TimeChanged;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            apiDGA = Helper.ModRegistry.GetApi<IDynamicGameAssetsApi>("spacechase0.DynamicGameAssets");
            if(apiDGA != null)
            {
                IManifest manifest = new MyManifest("aedenthorn.TentDGA", "TentDGA", "aedenthorn", "TentDGA", new SemanticVersion("0.1.0"))
                {
                    ContentPackFor = new MyManifestContentPackFor
                    {
                        UniqueID = "spacechase0.DynamicGameAssets"
                    },
                    ExtraFields = new Dictionary<string, object>() { { "DGA.FormatVersion", 2 }, { "DGA.ConditionsFormatVersion", "1.23.0" } }
                };

                apiDGA.AddEmbeddedPack(manifest, $"{Helper.DirectoryPath}/dga");
            }


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
                name: () => "Mod Enabled?",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Save On Tent?",
                getValue: () => Config.SaveOnTent,
                setValue: value => Config.SaveOnTent = value
            );
        }
    }

}