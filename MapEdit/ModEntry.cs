using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.Collections.Generic;
using xTile.Layers;
using xTile.Tiles;

namespace MapEdit
{
    public class ModEntry : Mod
    {
        public static ModEntry context;

        public static ModConfig Config;
        public static IModHelper SHelper;
        public static IMonitor SMonitor;

        public static int modNumber = 189017541;

        public static Texture2D existsTexture;
        public static Texture2D activeTexture;
        public static Texture2D copiedTexture;

        public static List<string> cleanMaps = new List<string>();
        public static MapCollectionData mapCollectionData = new MapCollectionData();

        public static PerScreen<bool> modActive = new PerScreen<bool>();
        public static PerScreen<Vector2> copiedTileLoc = new PerScreen<Vector2>();
        public static PerScreen<Vector2> pastedTileLoc = new PerScreen<Vector2>();
        public static PerScreen<Dictionary<string, Tile>> currentTileDict = new PerScreen<Dictionary<string, Tile>>();
        public static PerScreen<int> currentLayer = new PerScreen<int>();

        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();

            SHelper = Helper;
            SMonitor = Monitor;

            currentTileDict.Value = new Dictionary<string, Tile>();

            HelperEvents.Initialize(Config, Monitor, Helper);

            ModActions.CreateTextures();

            Helper.Events.Display.RenderedWorld += HelperEvents.Display_RenderedWorld;
            Helper.Events.Input.ButtonPressed += HelperEvents.Input_ButtonPressed;
            Helper.Events.GameLoop.UpdateTicked += HelperEvents.GameLoop_UpdateTicked;
            Helper.Events.GameLoop.SaveLoaded += HelperEvents.GameLoop_SaveLoaded;
            Helper.Events.GameLoop.ReturnedToTitle += HelperEvents.GameLoop_ReturnedToTitle;
            Helper.Events.Player.Warped += HelperEvents.Player_Warped;

            Helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            
            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), nameof(Game1.pressSwitchToolButton)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.pressSwitchToolButton_Prefix))
            );
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.EnableMod)
                return;

            if (e.NameWithoutLocale.StartsWith("Maps"))
            {
                foreach (string name in mapCollectionData.mapDataDict.Keys)
                {
                    if (e.NameWithoutLocale.IsEquivalentTo("Maps/" + name))
                    {
                        e.Edit(delegate (IAssetData idata)
                        {
                            Monitor.Log("Editing map " + e.Name);
                            var mapData = idata.AsMap();
                            MapData data = mapCollectionData.mapDataDict[name];
                            int count = 0;
                            foreach (var kvp in data.tileDataDict)
                            {
                                foreach (Layer layer in mapData.Data.Layers)
                                {
                                    if (layer.Id == "Paths")
                                        continue;
                                    try
                                    {
                                        layer.Tiles[(int)kvp.Key.X, (int)kvp.Key.Y] = null;
                                    }
                                    catch
                                    {

                                    }
                                }
                                foreach (var kvp2 in kvp.Value.tileDict)
                                {
                                    try
                                    {
                                        List<StaticTile> tiles = new List<StaticTile>();
                                        for (int i = 0; i < kvp2.Value.tiles.Count; i++)
                                        {
                                            TileInfo tile = kvp2.Value.tiles[i];
                                            tiles.Add(new StaticTile(mapData.Data.GetLayer(kvp2.Key), mapData.Data.GetTileSheet(tile.tileSheet), tile.blendMode, tile.tileIndex));
                                            foreach (var prop in kvp2.Value.tiles[i].properties)
                                            {
                                                tiles[i].Properties[prop.Key] = prop.Value;
                                            }
                                        }

                                        if (kvp2.Value.tiles.Count == 1)
                                        {
                                            mapData.Data.GetLayer(kvp2.Key).Tiles[(int)kvp.Key.X, (int)kvp.Key.Y] = tiles[0];
                                        }
                                        else
                                        {
                                            mapData.Data.GetLayer(kvp2.Key).Tiles[(int)kvp.Key.X, (int)kvp.Key.Y] = new AnimatedTile(mapData.Data.GetLayer(kvp2.Key), tiles.ToArray(), kvp2.Value.frameInterval);
                                        }
                                        count++;
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                            Monitor.Log($"Added {count} custom tiles to map {name}");
                        });
                        cleanMaps.Add(name);
                    }
                }
            }
        }
        private static bool pressSwitchToolButton_Prefix()
        {
            if (!Config.EnableMod || !Context.IsPlayerFree || Game1.input.GetMouseState().ScrollWheelValue == Game1.oldMouseState.ScrollWheelValue || !modActive.Value || copiedTileLoc.Value.X < 0)
                return true;

            ModActions.SwitchTile(Game1.input.GetMouseState().ScrollWheelValue - Game1.oldMouseState.ScrollWheelValue > 0);

            return false;
        }
    }
}
