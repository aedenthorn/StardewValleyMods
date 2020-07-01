using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
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
    public class ModEntry : Mod, IAssetEditor, IAssetLoader
    {
        
        public static ModConfig config;
        public static IMonitor SMonitor;
        public static IJsonAssetsApi JsonAssets;
        public static Texture2D OxygenBarTexture;
        public static int scubaMaskID = -1;
        public static int scubaFinsID = -1;
        public static int scubaTankID = -1;
        public static List<int> scubaGear = new List<int>();
        public static List<SButton> dirButtons = new List<SButton>();
        public static bool myButtonDown = false;
        public static int oxygen = 0;
        public static int lastUpdateMs = 0;
        public static bool willSwim = false;
        public static bool isUnderwater = false;
        public static NPC oldMariner;
        public static bool marinerQuestionsWrongToday = false;
        public static Random myRand;
        public static Dictionary<string, DiveMap> diveMaps = new Dictionary<string, DiveMap>();
        public static Dictionary<string,bool> changeLocations = new Dictionary<string, bool> {
            {"UnderwaterMountain", false },
            {"Mountain", false },
            {"Town", false },
            {"Forest", false },
            {"UnderwaterBeach", false },
            {"Beach", false },
        };
        public static List<Vector2> bubbles = new List<Vector2>();
        private string[] diveLocations = new string[] {
            "Beach",
            "Forest",
            "Mountain",
            "UnderwaterBeach",
            "UnderwaterMountain",
            "ScubaCave",
            "ScubaAbigailCave",
            "ScubaCrystalCave",
        };


        public override void Entry(IModHelper helper)
        {
            config = Helper.ReadConfig<ModConfig>();
            if (!config.EnableMod)
                return;

            SMonitor = Monitor;

            myRand = new Random();
            
            SwimPatches.Initialize(Monitor, helper, config);
            SwimDialog.Initialize(Monitor, helper, config);
            SwimMaps.Initialize(Monitor, helper, config);
            SwimHelperEvents.Initialize(Monitor, helper, config);
            SwimUtils.Initialize(Monitor, helper, config);

            foreach (InputButton ib in Game1.options.moveUpButton)
            {
                dirButtons.Add(ib.ToSButton());
            }
            foreach(InputButton ib in Game1.options.moveDownButton)
            {
                dirButtons.Add(ib.ToSButton());
            }
            foreach(InputButton ib in Game1.options.moveRightButton)
            {
                dirButtons.Add(ib.ToSButton());
            }
            foreach(InputButton ib in Game1.options.moveLeftButton)
            {
                dirButtons.Add(ib.ToSButton());
            }

            helper.Events.GameLoop.UpdateTicked += SwimHelperEvents.GameLoop_UpdateTicked;
            helper.Events.Input.ButtonPressed += SwimHelperEvents.Input_ButtonPressed;
            helper.Events.Input.ButtonReleased += SwimHelperEvents.Input_ButtonReleased;
            helper.Events.GameLoop.DayStarted += SwimHelperEvents.GameLoop_DayStarted;
            helper.Events.GameLoop.GameLaunched += SwimHelperEvents.GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += SwimHelperEvents.GameLoop_SaveLoaded;
            helper.Events.Display.RenderedHud += SwimHelperEvents.Display_RenderedHud;
            helper.Events.Display.RenderedWorld += SwimHelperEvents.Display_RenderedWorld;
            helper.Events.Player.InventoryChanged += SwimHelperEvents.Player_InventoryChanged;
            helper.Events.Player.Warped += SwimHelperEvents.Player_Warped;

            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

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
               postfix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.Farmer_updateCommon_Postfix))
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
               transpiler: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.Wand_DoFunction_Transpiler))
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

        }
        public override object GetApi()
        {
            return new SwimModApi(Monitor, this);
        }


        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (!config.EnableMod)
                return false;

            if (asset.AssetName.StartsWith("Fishies"))
            {
                return true;
            }

            string name = asset.AssetName.Replace("/", "\\");

            if (name.Equals("Maps\\CrystalCave") || name.Equals("Maps\\CrystalCaveDark"))
            {
                return true;
            }


            return false;
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            Monitor.Log($"loading asset for {asset.AssetName}");


            string name = asset.AssetName.Replace("/", "\\");

            if (name.Equals("Maps\\CrystalCave") || name.Equals("Maps\\CrystalCaveDark"))
            {
                return (T)(object)Helper.Content.Load<Map>($"assets/tmx-pack/assets/{name.Substring(5)}.tbin");
            }

             
            if (asset.AssetName.StartsWith("Fishies"))
            {
                return (T)(object)Helper.Content.Load<Texture2D>($"assets/{asset.AssetName}.png");
            }
            throw new InvalidDataException(); 
        }

                /// <summary>Get whether this instance can edit the given asset.</summary>
                /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (!config.EnableMod)
                return false;

            foreach(string key in changeLocations.Keys)
            {
                if (asset.AssetNameEquals($"Maps/{key}"))
                    return false;

            }

            return false;
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            Monitor.Log("Editing asset: " + asset.AssetName);

            string mapName = asset.AssetName.Replace("Maps/", "").Replace("Maps\\", "");

            if (false && changeLocations.ContainsKey(mapName))
            {
                IAssetDataForMap map = asset.AsMap();
                for (int x = 0; x < map.Data.Layers[0].LayerWidth; x++)
                {
                    for (int y = 0; y < map.Data.Layers[0].LayerHeight; y++)
                    {
                        if (SwimUtils.doesTileHaveProperty(map.Data, x, y, "Water", "Back") != null)
                        {
                            Tile tile = map.Data.GetLayer("Back").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                            if (tile != null && (((mapName == "Beach" || mapName == "UnderwaterBeach") && x > 58 && x < 61 && y > 11 && y < 15) || mapName != "Beach"))
                            {
                                if (tile.TileIndexProperties.ContainsKey("Passable"))
                                {
                                    tile.TileIndexProperties.Remove("Passable");
                                }
                            }
                            tile = map.Data.GetLayer("Front").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                            if (tile != null)
                            {
                                if (tile.TileIndexProperties.ContainsKey("Passable"))
                                {
                                    //tile.TileIndexProperties.Remove("Passable");
                                }
                            }
                            if (map.Data.GetLayer("AlwaysFront") != null)
                            {
                                tile = map.Data.GetLayer("AlwaysFront").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                                if (tile != null)
                                {
                                    if (tile.TileIndexProperties.ContainsKey("Passable"))
                                    {
                                        //tile.TileIndexProperties.Remove("Passable");
                                    }
                                }
                            }
                            tile = map.Data.GetLayer("Buildings").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                            if (tile != null)
                            {
                                if (
                                    ((mapName == "Beach" || mapName == "UnderwaterBeach") && x > 58 && x < 61 && y > 11 && y < 15) ||
                                    (mapName != "Beach" && mapName != "UnderwaterBeach"
                                        && ((tile.TileIndex > 1292 && tile.TileIndex < 1297) || (tile.TileIndex > 1317 && tile.TileIndex < 1322)
                                            || (tile.TileIndex % 25 > 17 && tile.TileIndex / 25 < 53 && tile.TileIndex / 25 > 48)
                                            || (tile.TileIndex % 25 > 1 && tile.TileIndex % 25 < 7 && tile.TileIndex / 25 < 53 && tile.TileIndex / 25 > 48)
                                            || (tile.TileIndex % 25 > 11 && tile.TileIndex / 25 < 51 && tile.TileIndex / 25 > 48)
                                            || (tile.TileIndex % 25 > 10 && tile.TileIndex % 25 < 14 && tile.TileIndex / 25 < 49 && tile.TileIndex / 25 > 46)
                                            || tile.TileIndex == 734 || tile.TileIndex == 759
                                            || tile.TileIndex == 628 || tile.TileIndex == 629
                                            || (mapName == "Forest" && x == 119 && ((y > 42 && y < 48) || (y > 104 && y < 119)))
                                    
                                        )
                                    )
                                )
                                {
                                    if (tile.TileIndexProperties.ContainsKey("Passable"))
                                    {
                                        tile.TileIndexProperties["Passable"] = "T";
                                    }
                                    else
                                    {
                                        tile.TileIndexProperties.Add("Passable", "T");
                                    }

                                }
                                else if(mapName == "Beach" && tile.TileIndex == 76)
                                {
                                    if(x > 58 && x < 61 && y > 11 && y < 15)
                                    {
                                        Game1.getLocationFromName(mapName).removeTile(x, y, "Buildings");
                                    }
                                    if (tile.TileIndexProperties.ContainsKey("Passable"))
                                    {
                                        tile.TileIndexProperties.Remove("Passable");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
