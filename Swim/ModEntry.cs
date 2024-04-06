using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Shirts;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using xTile;
using xTile.Dimensions;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle; 

namespace Swim
{
    public class ModEntry : Mod
    {
        
        public static ModConfig Config;
        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        //public static IJsonAssetsApi JsonAssets;
        public static ModEntry context;

        public static PerScreen<Texture2D> OxygenBarTexture = new PerScreen<Texture2D>();
        public static readonly PerScreen<string> scubaMaskID = new PerScreen<string>();
        public static readonly PerScreen<string> scubaFinsID = new PerScreen<string>();
        public static readonly PerScreen<string> scubaTankID = new PerScreen<string>();
        public static readonly PerScreen<int> oxygen = new PerScreen<int>(() => 0);
        public static readonly PerScreen<int> lastUpdateMs = new PerScreen<int>(() => 0);
        public static readonly PerScreen<bool> willSwim = new PerScreen<bool>(() => false);
        public static readonly PerScreen<bool> isUnderwater = new PerScreen<bool>(() => false);
        public static readonly PerScreen<NPC> oldMariner = new PerScreen<NPC>();
        public static readonly PerScreen<bool> marinerQuestionsWrongToday = new PerScreen<bool>(() => false);
        public static readonly PerScreen<Random> myRand = new PerScreen<Random>(() => new Random());
        public static PerScreen<bool> locationIsPool = new PerScreen<bool>(() => false);

        //public static readonly PerScreen<Dictionary<string, DiveMap>> diveMaps = new PerScreen<Dictionary<string, DiveMap>>(() => new Dictionary<string, DiveMap>());
        public static Dictionary<string, DiveMap> diveMaps = new Dictionary<string, DiveMap>();

        public static Dictionary<string,bool> changeLocations = new Dictionary<string, bool> {
            {"Custom_UnderwaterMountain", false },
            {"Mountain", false },
            {"Town", false },
            {"Forest", false },
            {"Custom_UnderwaterBeach", false },
            {"Beach", false },
        };

        public static readonly PerScreen<List<Vector2>> bubbles = new PerScreen<List<Vector2>>(() => new List<Vector2>());

        private string[] diveLocations = new string[] {
            "Beach",
            "Forest",
            "Mountain",
            "Custom_UnderwaterBeach",
            "Custom_UnderwaterMountain",
            "Custom_ScubaCave",
            "Custom_ScubaAbigailCave",
            "Custom_ScubaCrystalCave",
        };

        public override void Entry(IModHelper helper)
        {           
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            // Without the config only option, the player would not be able to re-enable the mod after they disabled it because we never added the config options.
            helper.Events.GameLoop.GameLaunched += Config.EnableMod ? SwimHelperEvents.GameLoop_GameLaunched : GameLoop_GameLaunched_ConfigOnly;

            if (!Config.EnableMod)
                return;

            SwimPatches.Initialize(Monitor, helper, Config);
            SwimDialog.Initialize(Monitor, helper, Config);
            SwimMaps.Initialize(Monitor, helper, Config);
            SwimHelperEvents.Initialize(Monitor, helper, Config);
            SwimUtils.Initialize(Monitor, helper, Config);

            helper.Events.GameLoop.UpdateTicked += SwimHelperEvents.GameLoop_UpdateTicked;
            helper.Events.Input.ButtonPressed += SwimHelperEvents.Input_ButtonPressed;
            helper.Events.GameLoop.DayStarted += SwimHelperEvents.GameLoop_DayStarted;
            helper.Events.GameLoop.SaveLoaded += SwimHelperEvents.GameLoop_SaveLoaded;
            helper.Events.GameLoop.Saving += SwimHelperEvents.GameLoop_Saving;
            helper.Events.Display.RenderedHud += SwimHelperEvents.Display_RenderedHud;
            helper.Events.Display.RenderedWorld += SwimHelperEvents.Display_RenderedWorld;
            helper.Events.Player.InventoryChanged += SwimHelperEvents.Player_InventoryChanged;
            helper.Events.Player.Warped += SwimHelperEvents.Player_Warped;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(this.ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmerRenderer), nameof(FarmerRenderer.draw), new Type[] { typeof(SpriteBatch), typeof(FarmerSprite.AnimationFrame), typeof(int), typeof(Rectangle), typeof(Vector2), typeof(Vector2), typeof(float), typeof(int), typeof(Color), typeof(float), typeof(float), typeof(Farmer) }),
               prefix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.FarmerRenderer_draw_Prefix)),
               postfix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.FarmerRenderer_draw_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(FarmerSprite), "checkForFootstep"),
               prefix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.FarmerSprite_checkForFootstep_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.startEvent)),
               postfix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.GameLocation_StartEvent_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Event), nameof(Event.exitEvent)),
               postfix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.Event_exitEvent_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), "updateCommon"),
               prefix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.Farmer_updateCommon_Prefix)),
               postfix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.Farmer_updateCommon_Postfix)),
               transpiler: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.Farmer_updateCommon_Transpiler))
            );
            
            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.setRunning)),
               prefix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.Farmer_setRunning_Prefix)),
               postfix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.Farmer_setRunning_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.changeIntoSwimsuit)),
               postfix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.Farmer_changeIntoSwimsuit_Postfix))
            );
            
            harmony.Patch(
               original: AccessTools.Method(typeof(Toolbar), nameof(Toolbar.draw), new Type[] { typeof(SpriteBatch) }),
               prefix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.Toolbar_draw_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Wand), nameof(Wand.DoFunction)),
               prefix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.Wand_DoFunction_Prefix)),
               postfix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.Wand_DoFunction_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.draw)),
               prefix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.GameLocation_draw_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.UpdateWhenCurrentLocation)),
               postfix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.GameLocation_UpdateWhenCurrentLocation_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.resetForPlayerEntry)),
               prefix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.GameLocation_resetForPlayerEntry_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character) }),
               prefix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.GameLocation_isCollidingPosition_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performTouchAction), new Type[] {typeof(string), typeof(Vector2)}),
               prefix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.GameLocation_performTouchAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performTouchAction), new Type[] { typeof(string[]), typeof(Vector2) }),
               prefix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.GameLocation_performTouchAction_PrefixArray))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
               prefix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.GameLocation_checkAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character), typeof(bool), typeof(bool), typeof(bool), typeof(bool) }),
               postfix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.GameLocation_isCollidingPosition_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.sinkDebris)),
               postfix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.GameLocation_sinkDebris_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmerRenderer), nameof(FarmerRenderer.drawHairAndAccesories)),
               transpiler: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.FarmerRenderer_drawHairAndAccessories_Transpiler))
            );
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.StartsWith("aedenthorn.Swim/Fishies"))
            {
                e.LoadFromModFile<Texture2D>($"assets/{e.NameWithoutLocale.ToString().Substring("aedenthorn.Swim/".Length)}.png", AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Portraits\\Mariner"))
            {
                e.LoadFrom(() => {return Game1.content.Load<Texture2D>("Portraits\\Gil");}, AssetLoadPriority.Low);
            }
        }

        public static void GameLoop_GameLaunched_ConfigOnly(object sender, GameLaunchedEventArgs e)
        {
            setupModConfig();
        }

        public static void setupModConfig()
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = SHelper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu != null)
            {
                // Register mod.
                configMenu.Register(
                    mod: ModEntry.context.ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => SHelper.WriteConfig(Config)
                );

                #region Region: Basic Options.

                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "Mod Enabled?",
                    tooltip: () => "Enables and Disables mod. Requires game restart to go into effect.",
                    getValue: () => Config.EnableMod,
                    setValue: value => Config.EnableMod = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "Auto-Swim enabled?",
                    tooltip: () => "Allow character to jump to the water automatically, when you walk to land edge.",
                    getValue: () => Config.ReadyToSwim,
                    setValue: value => Config.ReadyToSwim = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "ShowOxygenBar",
                    tooltip: () => "Define, will oxygen bar draw or not, when you dive to the water.",
                    getValue: () => Config.ShowOxygenBar,
                    setValue: value => Config.ShowOxygenBar = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "SwimSuitAlways",
                    tooltip: () => "If set's true, your character will always wear a swimsuit.",
                    getValue: () => Config.SwimSuitAlways,
                    setValue: value => Config.SwimSuitAlways = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "NoAutoSwimSuit",
                    tooltip: () => "If set's false, character will NOT wear a swimsuit automatically when you enter the water.",
                    getValue: () => Config.NoAutoSwimSuit,
                    setValue: value => Config.NoAutoSwimSuit = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "DisplayHatWithSwimsuit",
                    tooltip: () => "If set to true, will display your hat while you are wearing your swimming suit.",
                    getValue: () => Config.DisplayHatWithSwimsuit,
                    setValue: value => Config.DisplayHatWithSwimsuit = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "AllowActionsWhileInSwimsuit",
                    tooltip: () => "Allow you to use items, while you're swimming (may cause some visual bugs).",
                    getValue: () => Config.AllowActionsWhileInSwimsuit,
                    setValue: value => Config.AllowActionsWhileInSwimsuit = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "AllowRunningWhileInSwimsuit",
                    tooltip: () => "Allow you to run, while you're swimming (may cause some visual bugs).",
                    getValue: () => Config.AllowRunningWhileInSwimsuit,
                    setValue: value => Config.AllowRunningWhileInSwimsuit = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "EnableClickToSwim",
                    tooltip: () => "Enables or Disables possibility to manual jump to the water (by clicking certain key).",
                    getValue: () => Config.EnableClickToSwim,
                    setValue: value => Config.EnableClickToSwim = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "MustClickOnOppositeTerrain",
                    tooltip: () => "Whether you must click on land to leave the water (or vice versa) or can just click in the direction of land (when using click to swim).",
                    getValue: () => Config.MustClickOnOppositeTerrain,
                    setValue: value => Config.MustClickOnOppositeTerrain = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "SwimRestoresVitals",
                    tooltip: () => "If set's true, your HP and Energy will restore, while you're swimming (like in Bath).",
                    getValue: () => Config.SwimRestoresVitals,
                    setValue: value => Config.SwimRestoresVitals = value
                );
                #endregion

                #region Region: Key Binds.

                configMenu.AddKeybind(
                    mod: ModEntry.context.ModManifest,
                    name: () => "Enable Auto-Swimming",
                    tooltip: () => "Enables and Disables auto-swimming option.",
                    getValue: () => Config.SwimKey,
                    setValue: value => Config.SwimKey = value
                );
                configMenu.AddKeybind(
                    mod: ModEntry.context.ModManifest,
                    name: () => "Toggle Swimsuit",
                    tooltip: () => "Change character cloth to swimsuit and vice versa.",
                    getValue: () => Config.SwimSuitKey,
                    setValue: value => Config.SwimSuitKey = value
                );
                configMenu.AddKeybind(
                    mod: ModEntry.context.ModManifest,
                    name: () => "Dive",
                    tooltip: () => "Change character cloth to swimsuit and vice versa.",
                    getValue: () => Config.DiveKey,
                    setValue: value => Config.DiveKey = value
                );
                configMenu.AddKeybind(
                    mod: ModEntry.context.ModManifest,
                    name: () => "Manual Jump",
                    tooltip: () => "Allow you to jump into the water by clicking a certain key.",
                    getValue: () => Config.ManualJumpButton,
                    setValue: value => Config.ManualJumpButton = value
                );
                #endregion

                #region Region: Advanced Tweaks.

                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "JumpTimeInMilliseconds",
                    tooltip: () => "Sets jumping animation time.",
                    getValue: () => Config.JumpTimeInMilliseconds,
                    setValue: value => Config.JumpTimeInMilliseconds = value
                );

                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "OxygenMult",
                    tooltip: () => "Sets oxygen multiplier (Energy * Mult = O2).",
                    getValue: () => Config.OxygenMult,
                    setValue: value => Config.OxygenMult = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "BubbleMult",
                    tooltip: () => "Set's quantity multiplier of bubbles.",
                    getValue: () => Config.BubbleMult,
                    setValue: value => Config.BubbleMult = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "AddFishies",
                    tooltip: () => "Allow fishes to spawn in underwater.",
                    getValue: () => Config.AddFishies,
                    setValue: value => Config.AddFishies = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "AddCrabs",
                    tooltip: () => "Allow crabs to spawn in underwater.",
                    getValue: () => Config.AddCrabs,
                    setValue: value => Config.AddCrabs = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "BreatheSound",
                    tooltip: () => "If sets true, while you're underwater you will hear breathe sound.",
                    getValue: () => Config.BreatheSound,
                    setValue: value => Config.BreatheSound = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "MineralPerThousandMin",
                    tooltip: () => "Sets minimal quantity, that can be meet underwater.",
                    getValue: () => Config.MineralPerThousandMin,
                    setValue: value => Config.MineralPerThousandMin = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "MineralPerThousandMax",
                    tooltip: () => "Sets maximal quantity, that can be meet underwater.",
                    getValue: () => Config.MineralPerThousandMax,
                    setValue: value => Config.MineralPerThousandMax = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "CrabsPerThousandMin",
                    tooltip: () => "Sets minimal quantity, that can be meet underwater.",
                    getValue: () => Config.CrabsPerThousandMin,
                    setValue: value => Config.CrabsPerThousandMin = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "CrabsPerThousandMax",
                    tooltip: () => "Sets maximal quantity, that can be meet underwater.",
                    getValue: () => Config.CrabsPerThousandMax,
                    setValue: value => Config.CrabsPerThousandMax = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "PercentChanceCrabIsMimic",
                    tooltip: () => "Sets chance to change crab by the mimic one.",
                    getValue: () => Config.PercentChanceCrabIsMimic,
                    setValue: value => Config.PercentChanceCrabIsMimic = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "MinSmolFishies",
                    tooltip: () => "Sets minimal quantity, that can be meet underwater.",
                    getValue: () => Config.MinSmolFishies,
                    setValue: value => Config.MinSmolFishies = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "MaxSmolFishies",
                    tooltip: () => "Sets maximal quantity, that can be meet underwater.",
                    getValue: () => Config.MaxSmolFishies,
                    setValue: value => Config.MaxSmolFishies = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "BigFishiesPerThousandMin",
                    tooltip: () => "Sets minimal quantity, that can be meet underwater.",
                    getValue: () => Config.BigFishiesPerThousandMin,
                    setValue: value => Config.BigFishiesPerThousandMin = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "BigFishiesPerThousandMax",
                    tooltip: () => "Sets maximal quantity, that can be meet underwater.",
                    getValue: () => Config.BigFishiesPerThousandMax,
                    setValue: value => Config.BigFishiesPerThousandMax = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "OceanForagePerThousandMin",
                    tooltip: () => "Sets minimal quantity, that can be meet underwater.",
                    getValue: () => Config.OceanForagePerThousandMin,
                    setValue: value => Config.OceanForagePerThousandMin = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "OceanForagePerThousandMax",
                    tooltip: () => "Sets maximal quantity, that can be meet underwater.",
                    getValue: () => Config.OceanForagePerThousandMax,
                    setValue: value => Config.OceanForagePerThousandMax = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "MinOceanChests",
                    tooltip: () => "Sets minimal quantity, that can be meet in underwater biome ocean.",
                    getValue: () => Config.MinOceanChests,
                    setValue: value => Config.MinOceanChests = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "MaxOceanChests",
                    tooltip: () => "Sets maximal quantity, that can be meet in underwater biome ocean",
                    getValue: () => Config.MaxOceanChests,
                    setValue: value => Config.MaxOceanChests = value
                );
                configMenu.AddTextOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "JumpDistanceMult",
                    tooltip: () => "Multiply jump sensitivity by this amount",
                    getValue: () => Config.TriggerDistanceMult + "",
                    setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var f)) { Config.TriggerDistanceMult = f; } }
                );
                #endregion
            }
        }

        public override object GetApi()
        {
            return new SwimModApi(Monitor, this);
        }
    }
}
