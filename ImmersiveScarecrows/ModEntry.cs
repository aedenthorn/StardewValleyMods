using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Object = StardewValley.Object;

namespace ImmersiveScarecrows
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static object atApi;
        public static IImmersiveApi sprinklerApi;
        public static ImmersiveApi api;

        public static string prefixKey = "aedenthorn.ImmersiveScarecrows/";
        public static string scarecrowKey = "aedenthorn.ImmersiveScarecrows/scarecrow";
        public static string sprinklerKey = "aedenthorn.ImmersiveSprinklers/sprinkler";
        public static string scaredKey = "aedenthorn.ImmersiveScarecrows/scared";
        public static string hatKey = "aedenthorn.ImmersiveScarecrows/hat";
        public static string guidKey = "aedenthorn.ImmersiveScarecrows/guid";
        public static string altTexturePrefix = "aedenthorn.ImmersiveScarecrows/AlternativeTexture";
        public static string altTextureKey = "AlternativeTexture";

        public static Dictionary<string, Object> scarecrowDict = new();

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

            HarmonyMethod prefix = new(typeof(ModEntry), nameof(ModEntry.Modded_Farm_AddCrows_Prefix));
            Type prismaticPatches = AccessTools.TypeByName("PrismaticTools.Framework.PrismaticPatches");
            if(prismaticPatches is not null)
            {
                MethodInfo prismaticPrefix = AccessTools.Method(prismaticPatches, "Farm_AddCrows");
                if (prismaticPrefix is not null)
                {
                    harmony.Patch(prismaticPrefix, prefix: prefix);
                    Monitor.Log("Found Prismatic Tools, patching for compat", LogLevel.Info);
                }
            }

            Type radioactivePatches = AccessTools.TypeByName("RadioactiveTools.Framework.RadioactivePatches");
            if (radioactivePatches is not null)
            {
                MethodInfo radioactivePrefix = AccessTools.Method(radioactivePatches, "Farm_AddCrows");
                if (radioactivePrefix is not null)
                {
                    harmony.Patch(radioactivePrefix, prefix: prefix);
                    Monitor.Log("Found Radioactive Tools, patching for compat", LogLevel.Info);
                }
            }
        }

        public override object GetApi()
        {
            return new ImmersiveApi();
        }
        public void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsPlayerFree)
                return;
            List<Vector2> tiles = new List<Vector2>();
            if (Helper.Input.IsDown(Config.ShowAllRangeButton))
            {
                foreach (var kvp in Game1.currentLocation.terrainFeatures.Pairs)
                {
                    if (kvp.Value is not HoeDirt)
                        continue;
                    var scarecrowTile = kvp.Key;
                    var tf = kvp.Value;
                    for (int i = 0; i < 4; i++)
                    {
                        var which = i;
                        if (!GetScarecrowTileBool(Game1.currentLocation, ref scarecrowTile, ref which))
                            continue;
                        if (!Game1.currentLocation.terrainFeatures.TryGetValue(scarecrowTile, out tf))
                            continue;
                        var obj = GetScarecrow(tf, which);
                        if (obj is not null)
                        {
                            tiles.AddRange(GetScarecrowTiles(scarecrowTile, which, obj.GetRadiusForScarecrow()));
                        }
                    }
                }
            }
            else if(Helper.Input.IsDown(Config.ShowRangeButton))
            {
                if (Game1.currentLocation?.terrainFeatures?.TryGetValue(Game1.currentCursorTile, out var tf) != true || tf is not HoeDirt)
                    return;
                var which = GetMouseCorner();
                var scarecrowTile = Game1.currentCursorTile;
                if (!GetScarecrowTileBool(Game1.currentLocation, ref scarecrowTile, ref which))
                    return;
                if (!Game1.currentLocation.terrainFeatures.TryGetValue(scarecrowTile, out tf))
                    return;
                var obj = GetScarecrow(tf, which);
                if (obj is not null)
                {
                    tiles = GetScarecrowTiles(scarecrowTile, which, obj.GetRadiusForScarecrow());
                }
            }
            if(tiles?.Any() == true)
            {
                foreach (var tile in tiles.Distinct())
                {
                    e.SpriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2(tile.X * 64, tile.Y * 64)), new Rectangle?(new Rectangle(194, 388, 16, 16)), Config.RangeTint * Config.RangeAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
                }
            }

        }

        public void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod)
                return;

            if(Context.IsPlayerFree && Config.Debug && e.Button == SButton.X)
            {

                Game1.getFarm().addCrows();
                Monitor.Log("Adding crows");
            }
            if (e.Button == Config.PickupButton && Context.CanPlayerMove)
            {
                int which = GetMouseCorner();
                if (ReturnScarecrow(Game1.player, Game1.currentLocation, Game1.currentCursorTile, which))
                {
                    Helper.Input.Suppress(e.Button);
                }
                else if (Config.PickupNearby || Constants.TargetPlatform == GamePlatform.Android)
                {
                    var list = Game1.currentLocation.terrainFeatures.Pairs.Where(t => t.Value is HoeDirt).ToList();
                    if (!list.Any())
                        return;

                    foreach (var kvp in list)
                    {
                        var distance = Vector2.Distance(kvp.Key * 64, Game1.player.position.Value);
                        if (distance > 64)
                            continue;
                        for (int i = 0; i < 4; i++)
                        {
                            if (ReturnScarecrow(Game1.player, Game1.currentLocation, kvp.Key, i))
                            {
                                Helper.Input.Suppress(e.Button);
                                return;
                            }
                        }
                    }
                }
            }
        }

        public void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            scarecrowDict.Clear();
        }

        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            atApi = Helper.ModRegistry.GetApi("PeacefulEnd.AlternativeTextures");
            sprinklerApi = Helper.ModRegistry.GetApi<IImmersiveApi>("aedenthorn.ImmersiveSprinklers");

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
                name: () => "Pickup",
                getValue: () => Config.PickupButton,
                setValue: value => Config.PickupButton = value
            );
            
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Pickup Nearby",
                getValue: () => Config.PickupNearby,
                setValue: value => Config.PickupNearby = value
            );
            
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Show Range",
                getValue: () => Config.ShowRangeButton,
                setValue: value => Config.ShowRangeButton = value
            );
            
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Show All Range",
                getValue: () => Config.ShowAllRangeButton,
                setValue: value => Config.ShowAllRangeButton = value
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
