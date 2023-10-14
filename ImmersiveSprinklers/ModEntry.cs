using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;
using Color = Microsoft.Xna.Framework.Color;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ImmersiveSprinklers
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static string prefixKey = "aedenthorn.ImmersiveSprinklers/";
        public static string sprinklerKey = "aedenthorn.ImmersiveSprinklers/sprinkler";
        public static string bigCraftableKey = "aedenthorn.ImmersiveSprinklers/bigCraftable";
        public static string guidKey = "aedenthorn.ImmersiveSprinklers/guid";
        public static string enricherKey = "aedenthorn.ImmersiveSprinklers/enricher";
        public static string fertilizerKey = "aedenthorn.ImmersiveSprinklers/fertilizer";
        public static string nozzleKey = "aedenthorn.ImmersiveSprinklers/nozzle";
        public static string altTexturePrefix = "aedenthorn.ImmersiveSprinklers/AlternativeTexture";
        public static string altTextureKey = "AlternativeTexture";

        public static Dictionary<string, Object> sprinklerDict = new();
        public static object atApi;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.Display.RenderedWorld += Display_RenderedWorld;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsPlayerFree || !Helper.Input.IsDown(Config.ShowRangeButton) || Game1.currentLocation?.terrainFeatures is null)
                return;
            List<Vector2> tiles = new List<Vector2>();
            foreach(var kvp in Game1.currentLocation.terrainFeatures.Pairs)
            {
                for(int i = 0; i < 4; i++)
                {
                    var which = i;
                    var sprinklerTile = kvp.Key;
                    if (TileSprinklerString(Game1.currentLocation, sprinklerTile, which) is null)
                    {
                        continue;
                    }
                    var tf = Game1.currentLocation.terrainFeatures[sprinklerTile];
                    Object obj = GetSprinklerCached(tf, which, tf.modData.ContainsKey(nozzleKey + which));
                    if (obj is not null)
                    {
                        tiles.AddRange(GetSprinklerTiles(sprinklerTile, which, GetSprinklerRadius(obj)));

                        if (tf.modData.ContainsKey(enricherKey + which) && tf.modData.TryGetValue(fertilizerKey + which, out string fertString))
                        {
                            Vector2 pos = sprinklerTile + GetSprinklerCorner(which) * 0.5f;
                            var f = GetFertilizer(fertString);
                            var xy = Game1.GlobalToLocal(pos * 64) + new Vector2(0, -64);
                            e.SpriteBatch.Draw(Game1.objectSpriteSheet, xy, GameLocation.getSourceRectForObject(f.ParentSheetIndex), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (pos.Y + 1) / 10000f);
                            var scaleFactor = 1f;
                            Utility.drawTinyDigits(f.Stack, e.SpriteBatch, xy + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(f.Stack, 3f * scaleFactor)) + 3f * scaleFactor, 64f - 18f * scaleFactor + 1f), 3f * scaleFactor, 1f, Color.White);
                        }
                    }
                }

            }
            foreach (var tile in tiles.Distinct())
            {
                e.SpriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2((float)((int)tile.X * 64), (float)((int)tile.Y * 64))), new Rectangle?(new Rectangle(194, 388, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (e.Button == Config.PickupButton && Context.CanPlayerMove)
            {
                int which = GetMouseCorner();
                if (ReturnSprinkler(Game1.player, Game1.currentLocation, Game1.currentCursorTile, which))
                {
                    Helper.Input.Suppress(e.Button);
                }
            }
            if (e.Button == Config.ActivateButton && Context.CanPlayerMove)
            {
                int which = GetMouseCorner();
                Vector2 tile = Game1.currentCursorTile;
                
                if (GetSprinklerTileBool(Game1.currentLocation, ref tile, ref which, out string sprinklerString))
                {
                    var obj = GetSprinkler(Game1.currentLocation.terrainFeatures[tile], which, Game1.currentLocation.terrainFeatures[tile].modData.ContainsKey(nozzleKey + which));
                    if (obj is not null)
                    {
                        ActivateSprinkler(Game1.currentLocation, tile, obj, which, false);
                        Helper.Input.Suppress(e.Button);
                    }
                }
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            sprinklerDict.Clear();
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            atApi = Helper.ModRegistry.GetApi("PeacefulEnd.AlternativeTextures");
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

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Range When Placing",
                getValue: () => Config.ShowRangeWhenPlacing,
                setValue: value => Config.ShowRangeWhenPlacing = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Pickup Key",
                getValue: () => Config.PickupButton,
                setValue: value => Config.PickupButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Activate Key",
                getValue: () => Config.ActivateButton,
                setValue: value => Config.ActivateButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Show Range Key",
                getValue: () => Config.ShowRangeButton,
                setValue: value => Config.ShowRangeButton = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Scale",
                getValue: () => Config.Scale + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.Scale = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Alpha",
                getValue: () => Config.Alpha + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.Alpha = f; } }
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Offset X",
                getValue: () => Config.DrawOffsetX,
                setValue: value => Config.DrawOffsetX = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Offset Y",
                getValue: () => Config.DrawOffsetY,
                setValue: value => Config.DrawOffsetY = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Offset Z",
                getValue: () => Config.DrawOffsetZ,
                setValue: value => Config.DrawOffsetZ = value
            );
        }
    }
}
