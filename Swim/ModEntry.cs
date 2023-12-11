using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using xTile;
using xTile.Dimensions;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle; 

namespace Swim
{
    public class ModEntry : Mod
    {
        
        public static ModConfig config;
        public static IMonitor SMonitor;
        public static IJsonAssetsApi JsonAssets;
        public static ModEntry context;

        public static PerScreen<Texture2D> OxygenBarTexture = new PerScreen<Texture2D>();
        public static readonly PerScreen<int> scubaMaskID = new PerScreen<int>();
        public static readonly PerScreen<int> scubaFinsID = new PerScreen<int>();
        public static readonly PerScreen<int> scubaTankID = new PerScreen<int>();
        public static readonly PerScreen<bool> myButtonDown = new PerScreen<bool>(() => false);
        public static readonly PerScreen<int> oxygen = new PerScreen<int>(() => 0);
        public static readonly PerScreen<int> lastUpdateMs = new PerScreen<int>(() => 0);
        public static readonly PerScreen<bool> willSwim = new PerScreen<bool>(() => false);
        public static readonly PerScreen<bool> isUnderwater = new PerScreen<bool>(() => false);
        public static readonly PerScreen<NPC> oldMariner = new PerScreen<NPC>();
        public static readonly PerScreen<bool> marinerQuestionsWrongToday = new PerScreen<bool>(() => false);
        public static readonly PerScreen<Random> myRand = new PerScreen<Random>(() => new Random());

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
            config = Helper.ReadConfig<ModConfig>();
            if (!config.EnableMod)
                return;

            context = this;

            SMonitor = Monitor;
           
            SwimPatches.Initialize(Monitor, helper, config);
            SwimDialog.Initialize(Monitor, helper, config);
            SwimMaps.Initialize(Monitor, helper, config);
            SwimHelperEvents.Initialize(Monitor, helper, config);
            SwimUtils.Initialize(Monitor, helper, config);

            helper.Events.GameLoop.UpdateTicked += SwimHelperEvents.GameLoop_UpdateTicked;
            helper.Events.Input.ButtonPressed += SwimHelperEvents.Input_ButtonPressed;
            helper.Events.Input.ButtonReleased += SwimHelperEvents.Input_ButtonReleased;
            helper.Events.GameLoop.DayStarted += SwimHelperEvents.GameLoop_DayStarted;
            helper.Events.GameLoop.GameLaunched += SwimHelperEvents.GameLoop_GameLaunched;
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
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performTouchAction)),
               prefix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.GameLocation_performTouchAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
               prefix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.GameLocation_checkAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character), typeof(bool), typeof(bool), typeof(bool) }),
               postfix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.GameLocation_isCollidingPosition_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.sinkDebris)),
               postfix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.GameLocation_sinkDebris_Postfix))
            );
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.StartsWith("aedenthorn.Swim/Fishies"))
            {
                e.LoadFromModFile<Texture2D>($"assets/{e.NameWithoutLocale.ToString().Substring("aedenthorn.Swim/".Length)}.png", StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        public override object GetApi()
        {
            return new SwimModApi(Monitor, this);
        }
    }
}
