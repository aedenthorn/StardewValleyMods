using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Permissions;
using Object = StardewValley.Object;

namespace LightMod
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static string dictPath = "aedenthorn.LightMod/dictionary";
        public static string alphaKey = "aedenthorn.LightMod/alpha";
        public static string radiusKey = "aedenthorn.LightMod/radius";
        public static string switchKey = "aedenthorn.LightMod/switch";
        public static Dictionary<string, LightData> lightDataDict = new Dictionary<string, LightData>();
        public static List<TextureData> lightTextureList = new List<TextureData>();
        public static bool suppressingScroll;

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
            //Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.Input.MouseWheelScrolled += Input_MouseWheelScrolled;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            harmony.Patch(
                original: AccessTools.Method(Game1.game1.GetType(), "DrawImpl"),
                transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SGame_DrawImpl_Transpiler))
            );

        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            if(Game1.timeOfDay < GetMorningLightTime())
            {
                Game1.morningSongPlayAction = null;
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree)
                return;
            if(e.Button == SButton.MouseMiddle)
            {
                if (Game1.currentLocation.Objects.TryGetValue(Game1.currentCursorTile, out Object value))
                {
                    ToggleLight(value);
                    return;
                }
                Point tile = Utility.Vector2ToPoint(Game1.currentCursorTile * 64);
                foreach (var f in Game1.currentLocation.furniture)
                {
                    if (f.getBoundingBox(f.TileLocation).Contains(tile))
                    {
                        ToggleLight(f);
                        return;
                    }
                }
            }
        }

        private void Input_MouseWheelScrolled(object sender, StardewModdingAPI.Events.MouseWheelScrolledEventArgs e)
        {
            suppressingScroll = false;
            if (!Config.ModEnabled || !Context.IsPlayerFree)
                return;
            Point tile = Utility.Vector2ToPoint(Game1.currentCursorTile * 64);
            foreach (var l in Game1.currentLocation.lightGlows)
            {
                if (Utility.Vector2ToPoint(l - new Vector2(32,0)) == tile)
                {
                    ChangeLightGlowAlpha(l, e.Delta);
                    return;
                }
            }
            if (Game1.currentLocation.Objects.TryGetValue(Game1.currentCursorTile, out Object value) && value.lightSource is not null)
            {
                if (Helper.Input.IsDown(Config.RadiusModButton))
                {
                    ChangeLightRadius(value, e.Delta);
                }
                else
                {
                    ChangeLightAlpha(value, e.Delta);
                }
                return;
            }
            foreach (var f in Game1.currentLocation.furniture)
            {
                if (f.getBoundingBox(f.TileLocation).Contains(tile))
                {
                    if (Helper.Input.IsDown(Config.RadiusModButton))
                    {
                        ChangeLightRadius(f, e.Delta);
                    }
                    else
                    {
                        ChangeLightAlpha(f, e.Delta);
                    }
                    return;
                }
            }
            foreach (var n in Game1.currentLocation.characters)
            {
                if (n.GetBoundingBox().Contains(tile))
                {
                    if (Helper.Input.IsDown(Config.RadiusModButton))
                    {
                        ChangeLightRadius(n, e.Delta);
                    }
                    else
                    {
                        ChangeLightAlpha(n, e.Delta);
                    }
                    return;
                }
            }
        }


        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {

            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, LightData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            SHelper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;


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
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Radius Mod Key",
                getValue: () => Config.RadiusModButton,
                setValue: value => Config.RadiusModButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Mod Key 1",
                getValue: () => Config.ModButton1,
                setValue: value => Config.ModButton1 = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Mod Key 2",
                getValue: () => Config.ModButton2,
                setValue: value => Config.ModButton2 = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Alpha Shift",
                getValue: () => Config.AlphaAmount,
                setValue: value => Config.AlphaAmount = value,
                min: 1,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Alpha Shift Mod 1",
                getValue: () => Config.Alpha1Amount,
                setValue: value => Config.Alpha1Amount = value,
                min: 1,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Alpha Shift Mod 2",
                getValue: () => Config.Alpha2Amount,
                setValue: value => Config.Alpha2Amount = value,
                min: 1,
                max: 255
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Radius Shift",
                getValue: () => Config.RadiusAmount + "",
                setValue: delegate(string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.RadiusAmount = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Radius Shift Mod 1",
                getValue: () => Config.Radius1Amount + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.Radius1Amount = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Radius Shift Mod 2",
                getValue: () => Config.Radius2Amount + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.Radius2Amount = f; } }
            );
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            lightDataDict = SHelper.GameContent.Load<Dictionary<string, LightData>>(dictPath) ?? new Dictionary<string, LightData>();
            lightTextureList.Clear();
            foreach (var key in lightDataDict.Keys.ToArray())
            {
                if (lightDataDict[key].texturePath != null && lightDataDict[key].texturePath.Length > 0)
                {
                    lightTextureList.Add(new TextureData()
                        {
                            texturePath = lightDataDict[key].texturePath,
                            frames = lightDataDict[key].textureFrames,
                            width = lightDataDict[key].frameWidth,
                            duration = lightDataDict[key].frameSeconds
                    }
                    );
                    lightDataDict[key].textureIndex = 8 + lightTextureList.Count;
                }
                else if(lightDataDict[key].textureIndex == 3 || lightDataDict[key].textureIndex < 1 || lightDataDict[key].textureIndex > 8)
                {
                    SMonitor.Log($"Invalid texture index {lightDataDict[key].textureIndex} for {key}");
                    lightDataDict.Remove(key);
                }
            }
            SMonitor.Log($"Loaded {lightDataDict.Count} custom light sources");
            SHelper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
        }

    }
}