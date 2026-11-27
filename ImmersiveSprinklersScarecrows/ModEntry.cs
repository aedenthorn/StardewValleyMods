using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using xTile.Dimensions;
using Color = Microsoft.Xna.Framework.Color;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ImmersiveSprinklersScarecrows
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static string sprinklerKey = "aedenthorn.ImmersiveSprinklersScarecrows/sprinkler";
        public static string scarecrowKey = "aedenthorn.ImmersiveSprinklersScarecrows/scarecrow";
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
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.Display.RenderedWorld += Display_RenderedWorld;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }
        public override object GetApi()
        {
            return new ImmersiveApi();
        }
        public void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsPlayerFree || Game1.currentLocation?.terrainFeatures is null)
                return;

            var sc = Helper.Input.IsDown(Config.ShowScarecrowRangeButton);
            var sp = Helper.Input.IsDown(Config.ShowSprinklerRangeButton);
            if (!sc && !sp)
                return;

            HashSet<Vector2> sprinklerTiles = new();
            HashSet<Vector2> scarecrowTiles = new();
            foreach (var kvp in Game1.currentLocation.Objects.Pairs)
            {
                if (sp && kvp.Value?.IsSprinkler() == true)
                {
                    foreach (var t in GetSprinklerTiles(kvp.Key, GetSprinklerRadius(kvp.Value)))
                        sprinklerTiles.Add(t);
                }
                if (sc && kvp.Value?.IsScarecrow() == true)
                {
                    foreach (var t in GetScarecrowTiles(kvp.Key, kvp.Value.GetRadiusForScarecrow()))
                        scarecrowTiles.Add(t);
                }
            }
            foreach(var tile in sprinklerTiles)
            {
                e.SpriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2((float)((int)tile.X * 64), (float)((int)tile.Y * 64))), new Rectangle?(new Rectangle(194, 388, 16, 16)), (scarecrowTiles.Contains(tile) ? Config.BothRangeTint : Config.SprinklerRangeTint) * Config.RangeAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
            }
            foreach(var tile in scarecrowTiles)
            {
                if (sprinklerTiles.Contains(tile))
                    continue;
                e.SpriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2((float)((int)tile.X * 64), (float)((int)tile.Y * 64))), new Rectangle?(new Rectangle(194, 388, 16, 16)), Config.ScarecrowRangeTint * Config.RangeAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
            }
        }


        public void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if(e.Button == SButton.OemOpenBrackets && Context.IsWorldReady)
            {
                //foreach(var key in Game1.currentLocation.terrainFeatures.Keys)
                //{
                //    if (Game1.currentLocation.terrainFeatures[key] is HoeDirt dirt && dirt.crop != null)
                //    {
                //        dirt.crop.growCompletely();
                //        Game1.currentLocation.terrainFeatures[key] = dirt;
                //    }
                //}
            }
            if (e.Button == Config.ActivateButton && Context.CanPlayerMove)
            {
                Vector2 tile = GetMouseTile();
                
                if (TryGetSprinkler(Game1.currentLocation, tile, out var sprinkler))
                {
                    ActivateSprinkler(Game1.currentLocation, tile, sprinkler, false);
                    Helper.Input.Suppress(e.Button);
                }
                else if (Config.ActivateNearby || Constants.TargetPlatform == GamePlatform.Android)
                {
                    foreach (var kvp in Game1.currentLocation.Objects.Pairs)
                    {
                        if (kvp.Value?.modData.ContainsKey(sprinklerKey) == true)
                        {
                            if (Config.ActivateNearbyRange > 0)
                            {
                                var distance = Vector2.Distance(kvp.Key * 64, Game1.player.position.Value);
                                if (distance <= 64 * Config.ActivateNearbyRange)
                                {
                                    ActivateSprinkler(Game1.currentLocation, kvp.Key, sprinkler, false);
                                    Helper.Input.Suppress(e.Button);
                                }
                            }
                        }
                    }
                }
            }
        }


        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
                name: () => "Activate Key",
                getValue: () => Config.ActivateButton,
                setValue: value => Config.ActivateButton = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Activate Nearby",
                getValue: () => Config.ActivateNearby,
                setValue: value => Config.ActivateNearby = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Activation Range",
                getValue: () => Config.ActivateNearbyRange,
                setValue: value => Config.ActivateNearbyRange = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Show Scarecrow Range",
                getValue: () => Config.ShowScarecrowRangeButton,
                setValue: value => Config.ShowScarecrowRangeButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Show Sprinkler Range",
                getValue: () => Config.ShowSprinklerRangeButton,
                setValue: value => Config.ShowSprinklerRangeButton = value
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
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "RangeAlpha",
                getValue: () => Config.RangeAlpha + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.RangeAlpha = f; } }
            );
        }
    }
}
