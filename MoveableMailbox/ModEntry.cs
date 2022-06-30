using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using xTile;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace MoveableMailbox
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod, IAssetEditor
    {

        public static IMonitor PMonitor;
        public static IModHelper PHelper;
        public static ModConfig config;
        private static IJsonAssetsApi mJsonAssets;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            config = Helper.ReadConfig<ModConfig>();

            if (!config.EnableMod)
                return;

            PMonitor = Monitor;
            PHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), nameof(Game1.loadForNewGame)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(loadForNewGame_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.checkForAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(checkForAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.placementAction)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(placementAction_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.performRemoveAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(performRemoveAction_Prefix))
            );

        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            
            mJsonAssets = base.Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            if (mJsonAssets == null)
            {
                Monitor.Log("Can't load Json Assets API for Moveable Mailbox", LogLevel.Warn);
            }
            else
            {
                mJsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, "json-assets"));
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            if (config.CustomMailbox)
            {
                try
                {
                    Texture2D tex = new Texture2D(Game1.graphics.GraphicsDevice, 16, 32);
                    Color[] data = new Color[tex.Width * tex.Height];
                    tex.GetData(data);

                    Texture2D source = Helper.Content.Load<Texture2D>($"Maps/{Game1.currentSeason.ToLower()}_outdoorsTileSheet", ContentSource.GameContent);
                    Color[] srcData = new Color[source.Width * source.Height];
                    source.GetData(srcData);

                    int width = 400;
                    int startx = 80;
                    int starty = 1232;
                    int start = starty * width + startx;
                    for (int i = 0; i < data.Length; i++)
                    {
                        int srcIdx = start + (i / 16 * width + i % 16); 
                        data[i] = srcData[srcIdx];
                    }
                    tex.SetData(data);
                    Stream stream = File.Create(Path.Combine(Helper.DirectoryPath, "json-assets", "BigCraftables", "Mailbox", "big-craftable.png"));
                    tex.SaveAsPng(stream, tex.Width, tex.Height);
                    stream.Close();
                    Monitor.Log($"Wrote custom mailbox texture from Maps/{ Game1.currentSeason.ToLower()}_outdoorsTileSheet to {Path.Combine(Helper.DirectoryPath, "json-assets", "BigCraftables", "Mailbox", "big-craftable.png")}.", LogLevel.Debug);
                    Helper.Content.InvalidateCache("Tilesheets/Craftables");
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Error writing mailbox texture.\n{ex}", LogLevel.Warn);
                }
            }

            Farm farm = Game1.getFarm();
            foreach (KeyValuePair<Vector2, Object> kvp in farm.objects.Pairs)
            {
                if (kvp.Value.Name.EndsWith("Mailbox"))
                {
                    (farm as Farm).mapMainMailboxPosition = Utility.Vector2ToPoint(kvp.Key);
                    PMonitor.Log($"Set mailbox location to {kvp.Key}");
                    return;
                }
            }
            farm.mapMainMailboxPosition = Point.Zero;
            PMonitor.Log("Set mailbox location to 0,0");
        }

        private static void loadForNewGame_Postfix()
        {
            Farm farm = Game1.getFarm();

            if (mJsonAssets != null)
            {
                int id = mJsonAssets.GetBigCraftableId("Mailbox");
                farm.Objects.Add(Utility.PointToVector2(farm.GetMainMailboxPosition()), new Object(Utility.PointToVector2(farm.GetMainMailboxPosition()), id));
                PMonitor.Log($"Added mailbox to farm, id {id}");
            }

        }

        private static void placementAction_Postfix(Object __instance, bool __result, int x, int y, Farmer who)
        {
            if (!__result || !__instance.Name.EndsWith("Mailbox") || who == null)
                return;

            (who.currentLocation as Farm).mapMainMailboxPosition = new Point(x / 64, y / 64);
            PMonitor.Log($"Set mailbox location to {(who.currentLocation as Farm).mapMainMailboxPosition}");
        }

        private static bool checkForAction_Prefix(Object __instance, ref bool __result, Farmer who, bool justCheckingForActivity)
        {

            if (__instance.Name.EndsWith("Mailbox") && !justCheckingForActivity)
            {
                PMonitor.Log("Clicked on mailbox");
                Point mailbox_position = Game1.player.getMailboxPosition();
                if (__instance.tileLocation.X != mailbox_position.X || __instance.tileLocation.Y != mailbox_position.Y)
                {
                    PMonitor.Log("Not our mailbox", LogLevel.Debug);
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Farm_OtherPlayerMailbox"));
                    __result = true;
                    return false;
                }
                who.currentLocation.mailbox();
                __result = true;
                return false;
            }
            return true;
        }

        private static void performRemoveAction_Prefix(Object __instance, Vector2 tileLocation, GameLocation environment)
        {
            if (__instance.Name.EndsWith("Mailbox") && environment is Farm)
            {
                PMonitor.Log("Removed mailbox");
                foreach (KeyValuePair<Vector2, Object> kvp in environment.objects.Pairs)
                {
                    if (kvp.Value.Name.EndsWith("Mailbox") && kvp.Key != tileLocation)
                    {
                        (environment as Farm).mapMainMailboxPosition = Utility.Vector2ToPoint(kvp.Key);
                        PMonitor.Log($"Set mailbox location to {kvp.Key}");
                        return;
                    }
                }
                (environment as Farm).mapMainMailboxPosition = Point.Zero;
                PMonitor.Log("Set mailbox location to 0,0");
            }
        }


        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (!config.EnableMod)
                return false;

            if (mJsonAssets != null && asset.AssetNameEquals("Tilesheets/Craftables"))
            {
                return true;
            }
            if (asset.AssetNameEquals("Maps/Farm") || asset.AssetNameEquals("Maps/Farm_Combat") || asset.AssetNameEquals("Maps/Farm_Fishing") || asset.AssetNameEquals("Maps/Farm_Foraging") || asset.AssetNameEquals("Maps/Farm_FourCorners") || asset.AssetNameEquals("Maps/Farm_Island") || asset.AssetNameEquals("Maps/Farm_Mining"))
            {
                return true;
            }

            return false;
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            Monitor.Log("Editing asset" + asset.AssetName);

            if (asset.AssetName.StartsWith("Maps"))
            {
                try
                {
                    var mapData = asset.AsMap();
                    for (int x = 0; x < mapData.Data.Layers[0].LayerWidth; x++)
                    {
                        for (int y = 0; y < mapData.Data.Layers[0].LayerHeight; y++)
                        {
                            //Monitor.Log($"{x},{y},{map.GetLayer("Buildings").Tiles[x, y]?.TileIndex},{map.GetLayer("Front").Tiles[x, y]?.TileIndex}",LogLevel.Warn);

                            if (mapData.Data.GetLayer("Buildings").Tiles[x, y]?.TileIndex == 1955)
                            {
                                Monitor.Log("Removing existing mailbox stand.");
                                mapData.Data.GetLayer("Buildings").Tiles[x, y] = null;
                            }
                            if (mapData.Data.GetLayer("Front").Tiles[x, y]?.TileIndex == 1930)
                            {
                                Monitor.Log("Removing existing mailbox top.");
                                mapData.Data.GetLayer("Front").Tiles[x, y] = null;
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Monitor.Log($"Exception removing existing mailbox.\n{ex}", LogLevel.Error);
                }
                return;
            }
            if (asset.AssetNameEquals("Tilesheets/Craftables"))
            {
                int id = mJsonAssets.GetBigCraftableId("Mailbox");
                Monitor.Log($"mailbox id {id}");
                if (id > 0)
                {
                    int x = id % 8 * 16;
                    int y = id / 8 * 32;
                    asset.AsImage().PatchImage(Helper.Content.Load<Texture2D>($"json-assets/BigCraftables/Mailbox/big-craftable.png"), targetArea: new Rectangle(x, y, 16, 32));
                    Monitor.Log("patched craftables.");
                }
            }
        }
    }
}