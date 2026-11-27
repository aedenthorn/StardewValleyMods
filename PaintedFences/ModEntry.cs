using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Color = Microsoft.Xna.Framework.Color;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace PaintedFences
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static string colorKey = "aedenthorn.PaintedFences/color";
        public static string dictPath = "aedenthorn.PaintedFences/dict";

        public static PerScreen<List<Vector2>> bulkFences = new PerScreen<List<Vector2>>(() => new());
        public static Dictionary<string, Texture2D> fenceTextureDict = new Dictionary<string, Texture2D>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.Input.MouseWheelScrolled += Input_MouseWheelScrolled;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.Input.ButtonReleased += Input_ButtonReleased;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }


        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            fenceTextureDict.Clear();
            foreach(var kvp in SHelper.GameContent.Load<Dictionary<string, string>>(dictPath))
            {
                fenceTextureDict[kvp.Key] = SHelper.GameContent.Load<Texture2D>(kvp.Value);
            }
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if(!Config.EnableMod)
                return;
            if(e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom( () => new Dictionary<string, string>()
                {
                    { "322", "aedenthorn.PaintedFences/Fence1" },
                    { "323", "aedenthorn.PaintedFences/Fence2" },
                    { "324", "aedenthorn.PaintedFences/Fence3" },
                    { "298", "aedenthorn.PaintedFences/Fence5" },
                }, 
                StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if(e.NameWithoutLocale.StartsWith("aedenthorn.PaintedFences/Fence"))
            {
                e.LoadFromModFile<Texture2D>($"assets/{e.NameWithoutLocale.ToString().Substring("aedenthorn.PaintedFences/".Length)}.png", StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        private void Input_MouseWheelScrolled(object sender, StardewModdingAPI.Events.MouseWheelScrolledEventArgs e)
        {
            if(!Config.EnableMod || !SHelper.Input.IsDown(Config.ModKey) || !Context.IsPlayerFree || !Game1.currentLocation.Objects.TryGetValue(Game1.currentCursorTile, out var obj) || obj is not Fence fence)
                return;

            if(SHelper.Input.IsDown(Config.BulkModKey))
            {
                if (!bulkFences.Value.Contains(fence.TileLocation))
                {
                    bulkFences.Value.Clear();
                    GetBulkFences(fence);
                }
                ColorFences(e.Delta > 0);
            }
            else
            {
                IncrementColor(e.Delta > 0, fence);
            }
            SHelper.Input.SuppressScrollWheel();
        }
        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod || e.Button != Config.DeleteKey || !Context.IsPlayerFree || !Game1.currentLocation.Objects.TryGetValue(Game1.currentCursorTile, out var obj) || obj is not Fence fence)
                return;

            if (SHelper.Input.IsDown(Config.BulkModKey))
            {
                int color = GetWhichColor(fence);
                UncolorFences(fence, color);
            }
            else
            {
                fence.modData.Remove(colorKey);
            }
            SHelper.Input.Suppress(e.Button);
        }
        private void Input_ButtonReleased(object sender, StardewModdingAPI.Events.ButtonReleasedEventArgs e)
        {
            if (!Config.EnableMod || e.Button != Config.BulkModKey)
                return;
            bulkFences.Value.Clear();
        }

        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
                name: () => SHelper.Translation.Get("EnableMod"),
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );

            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("ModKey"),
                getValue: () => Config.ModKey,
                setValue: value => Config.ModKey = value
            );

            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("BulkModKey"),
                getValue: () => Config.BulkModKey,
                setValue: value => Config.BulkModKey = value
            );

            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("DeleteKey"),
                getValue: () => Config.DeleteKey,
                setValue: value => Config.DeleteKey = value
            );
        }
    }
}
