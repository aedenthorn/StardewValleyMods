using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace MoveablePetBowl
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

        public static IMonitor PMonitor;
        public static IModHelper PHelper;
        public static ModConfig Config;
        private static IJsonAssetsApi mJsonAssets;
        private static Texture2D tilesTexture;
        private static Texture2D waterTexture;
        private static bool playerWatered = false;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.EnableMod)
                return;

            PMonitor = Monitor;
            PHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;

            Helper.Events.Content.AssetRequested += Content_AssetRequested;

            tilesTexture = Helper.ModContent.Load<Texture2D>("assets/tiles.png");
            waterTexture = Helper.ModContent.Load<Texture2D>("assets/water.png");

            var harmony = new Harmony(ModManifest.UniqueID);
            
            ConstructorInfo ci = typeof(Farm).GetConstructor(new Type[] { typeof(string), typeof(string) });
            harmony.Patch(
               original: ci,
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(Farm_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Farm), "_UpdateWaterBowl"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(_UpdateWaterBowl_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Pet), nameof(Pet.setAtFarmPosition)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(setAtFarmPosition_Prefix))
            );

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


            var hm = new HarmonyMethod(typeof(ModEntry), nameof(performRemoveAction_Prefix));
            hm.priority = Priority.First;

            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.performRemoveAction)),
               prefix: hm
            );


            hm = new HarmonyMethod(typeof(ModEntry), nameof(performToolAction_Postfix));
            hm.priority = Priority.First;

            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.performToolAction)),
               postfix: hm
            );


            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float), typeof(float) }),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_draw_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_draw_Postfix2))
            );


        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo("Maps/Farm") || e.NameWithoutLocale.IsEquivalentTo("Maps/Farm_Combat") || e.NameWithoutLocale.IsEquivalentTo("Maps/Farm_Fishing") || e.NameWithoutLocale.IsEquivalentTo("Maps/Farm_Foraging") || e.NameWithoutLocale.IsEquivalentTo("Maps/Farm_FourCorners") || e.NameWithoutLocale.IsEquivalentTo("Maps/Farm_Island") || e.NameWithoutLocale.IsEquivalentTo("Maps/Farm_Mining"))
            {
                Monitor.Log("Editing map " + e.Name.Name);

                try
                {
                    e.Edit(delegate (IAssetData data)
                    {
                        var mapData = data.AsMap();
                        for (int x = 0; x < mapData.Data.Layers[0].LayerWidth; x++)
                        {
                            for (int y = 0; y < mapData.Data.Layers[0].LayerHeight; y++)
                            {
                                //Monitor.Log($"{x},{y},{map.GetLayer("Buildings").Tiles[x, y]?.TileIndex},{map.GetLayer("Front").Tiles[x, y]?.TileIndex}",LogLevel.Warn);

                                if (mapData.Data.GetLayer("Buildings").Tiles[x, y]?.TileIndex == 1938)
                                {
                                    Monitor.Log("Removing existing pet bowl.");
                                    mapData.Data.GetLayer("Buildings").Tiles[x, y] = null;
                                    mapData.Data.GetLayer("Back").Tiles[x, y].TileIndex = 1938;
                                    try
                                    {
                                        mapData.Data.GetLayer("Back").Tiles[x - 1, y].TileIndexProperties.Remove("NoFurniture");
                                        mapData.Data.GetLayer("Back").Tiles[x - 1, y].Properties.Remove("NoFurniture");
                                        mapData.Data.GetLayer("Back").Tiles[x - 1, y].Properties.Remove("Placeable");
                                    }
                                    catch
                                    {
                                    }
                                    try
                                    {
                                        mapData.Data.GetLayer("Back").Tiles[x - 1, y + 1].TileIndexProperties.Remove("NoFurniture");
                                        mapData.Data.GetLayer("Back").Tiles[x - 1, y + 1].Properties.Remove("NoFurniture");
                                        mapData.Data.GetLayer("Back").Tiles[x - 1, y + 1].Properties.Remove("Placeable");
                                    }
                                    catch
                                    {
                                    }
                                    try
                                    {
                                        mapData.Data.GetLayer("Back").Tiles[x, y + 1].TileIndexProperties.Remove("NoFurniture");
                                        mapData.Data.GetLayer("Back").Tiles[x, y + 1].Properties.Remove("NoFurniture");
                                        mapData.Data.GetLayer("Back").Tiles[x, y + 1].Properties.Remove("Placeable");
                                    }
                                    catch
                                    {
                                    }
                                    try
                                    {
                                        mapData.Data.GetLayer("Back").Tiles[x, y + 2].Properties.Add("Buildable", "T");
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }

                    });
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Exception removing existing pet bowl.\n{ex}", LogLevel.Error);
                }
                return;


            }
            if (e.NameWithoutLocale.Name.EndsWith("outdoorsTileSheet") && Config.FixTilesheet)
            {
                e.Edit(delegate(IAssetData data)
                {
                    var image = data.AsImage();
                    int y = 0;
                    if (e.NameWithoutLocale.Name.EndsWith("summer_outdoorsTileSheet"))
                    {
                        y = 1;
                    }
                    else if (e.NameWithoutLocale.Name.EndsWith("fall_outdoorsTileSheet"))
                    {
                        y = 2;

                    }
                    else if (e.NameWithoutLocale.Name.EndsWith("winter_outdoorsTileSheet"))
                    {
                        y = 3;

                    }
                    Rectangle rect = new Rectangle(0, y * 16, 32, 16);

                    image.PatchImage(tilesTexture, rect, new Rectangle(208, 1232, 32, 16));
                });

                return;
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            
            mJsonAssets = base.Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            if (mJsonAssets == null)
            {
                Monitor.Log("Can't load Json Assets API for Moveable Pet Bowl", LogLevel.Warn);
            }
            else
            {
                mJsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, "json-assets"));
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {

            Farm farm = Game1.getFarm();
            PMonitor.Log($"Vanilla pet bowl location {farm.petBowlPosition}");
            ResetPetBowlLocation(farm, Vector2.Zero);
        }
        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            Farm farm = Game1.getFarm();
            if (Game1.isRaining)
            {
                foreach (KeyValuePair<Vector2, Object> kvp in farm.objects.Pairs)
                {
                    if (kvp.Value.bigCraftable.Value && kvp.Value.Name.EndsWith("Pet Bowl"))
                    {
                        kvp.Value.modData["aedenthorn.PetBowl/Watered"] = "true";
                    }
                }
            }
        }
        private static void ResetPetBowlLocation(Farm farm, Vector2 exception)
        {
            List<Vector2> potentialBowls = new List<Vector2>();
            foreach (KeyValuePair<Vector2, Object> kvp in farm.objects.Pairs)
            {
                if (kvp.Value.Name.EndsWith("Pet Bowl") && kvp.Key != exception)
                {
                    potentialBowls.Add(kvp.Key);
                }
            }

            if (potentialBowls.Any())
            {
                Vector2 bowlLoc = potentialBowls[Game1.random.Next(potentialBowls.Count)];
                farm.petBowlPosition.Value = Utility.Vector2ToPoint(bowlLoc);
                PMonitor.Log($"Set pet bowl location to {bowlLoc}");
                return;
            }

            PMonitor.Log("No pet bowl on farm, setting default", LogLevel.Debug);
            Layer back_layer = farm.map.GetLayer("Back");
            for (int x = 0; x < back_layer.LayerWidth; x++)
            {
                for (int y = 0; y < back_layer.LayerHeight; y++)
                {
                    if (back_layer.Tiles[x, y] != null && back_layer.Tiles[x, y].TileIndex == 1938)
                    {
                        farm.petBowlPosition.Set(x, y);
                        PMonitor.Log($"Set pet bowl position to {x}, {y}", LogLevel.Debug);
                        return;
                    }
                }
            }

        }

        private static void Farm_Postfix(Farm __instance)
        {
            ResetPetBowlLocation(__instance, Vector2.Zero);
        }

        private static bool _UpdateWaterBowl_Prefix(Farm __instance)
        {
            if (!Config.EnableMod)
                return true;
            Vector2 closestBowl = Vector2.Zero;
            float closestDistance = 4;
            foreach (KeyValuePair<Vector2, Object> kvp in __instance.objects.Pairs)
            {
                if(kvp.Value.bigCraftable.Value && kvp.Value.Name.EndsWith("Pet Bowl"))
                {
                    if (__instance.petBowlWatered.Value && !playerWatered)
                    {
                        kvp.Value.modData["aedenthorn.PetBowl/Watered"] = "true";
                    }
                    else if(kvp.Value.modData.ContainsKey("aedenthorn.PetBowl/Watered") && kvp.Value.modData["aedenthorn.PetBowl/Watered"] == "true")
                    {
                        foreach (Character c in __instance.characters)
                        {
                            if (c is Pet)
                            {
                                float distance = Vector2.Distance(c.getTileLocation(), kvp.Key);
                                if (distance < closestDistance)
                                {
                                    closestBowl = kvp.Key;
                                    closestDistance = distance;
                                }
                            }
                        }
                    }
                }
            }
            if (closestDistance < 4)
            {
                __instance.objects[closestBowl].modData["aedenthorn.PetBowl/Watered"] = "false";
            }
            playerWatered = false;
            return false;
        }

        private static void setAtFarmPosition_Prefix()
        {
            if (Game1.IsMasterGame)
            {
                Farm farm = Game1.getFarm();
                ResetPetBowlLocation(farm, Vector2.Zero);
            }
        }
        private static void loadForNewGame_Postfix()
        {
            Farm farm = Game1.getFarm();

            if (mJsonAssets != null)
            {
                int id = mJsonAssets.GetBigCraftableId("Wooden Pet Bowl");
                farm.Objects.Add(Utility.PointToVector2(farm.petBowlPosition.Value), new Object(Utility.PointToVector2(farm.petBowlPosition.Value), id));
                PMonitor.Log("Added wooden pet bowl to farm");
            }

        }

        private static void placementAction_Postfix(Object __instance, bool __result, int x, int y, Farmer who)
        {
            if (!__result || !__instance.Name.EndsWith("Pet Bowl") || who == null || !(who.currentLocation is Farm))
                return;

            (who.currentLocation as Farm).petBowlPosition.Value = new Point(x / 64, y / 64);
            PMonitor.Log($"Set pet bowl location to {(who.currentLocation as Farm).petBowlPosition}");
        }

        private static bool checkForAction_Prefix(Object __instance, ref bool __result, Farmer who, bool justCheckingForActivity)
        {

            if (__instance.Name.EndsWith("Pet Bowl") && who.currentLocation is Farm && !justCheckingForActivity)
            {
                PMonitor.Log("Clicked on pet bowl");
                who.currentLocation.playSound("slosh");
                return true;
            }
            return true;
        }

        private static void performRemoveAction_Prefix(Object __instance, Vector2 tileLocation, GameLocation environment)
        {
            if (__instance.Name.EndsWith("Pet Bowl") && environment is Farm)
            {
                PMonitor.Log("Removed pet bowl");
                ResetPetBowlLocation(environment as Farm, tileLocation);
            }
        }

        private static void performToolAction_Postfix(Object __instance, Tool t, GameLocation location)
        {
            if (__instance.Name.EndsWith("Pet Bowl") && location is Farm && t is WateringCan && (t as WateringCan).WaterLeft > 0)
            {
                PMonitor.Log("Watered pet bowl");
                __instance.modData["aedenthorn.PetBowl/Watered"] = "true";
                playerWatered = true;
                (location as Farm).petBowlWatered.Set(true);
            }
        }
        public static void Object_draw_Postfix(Object __instance, SpriteBatch spriteBatch, float layerDepth, int xNonTile, int yNonTile, float alpha)
        {
            if (__instance == null || !__instance.Name?.EndsWith("Pet Bowl") == true || !__instance.modData.ContainsKey("aedenthorn.PetBowl/Watered") || __instance.modData["aedenthorn.PetBowl/Watered"] != "true")
                return;
            Vector2 scaleFactor = __instance.getScale();
            scaleFactor *= 4f;
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)xNonTile, (float)yNonTile));
            Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
            spriteBatch.Draw(waterTexture, destination, new Rectangle(0,0,16,32), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, layerDepth + 0.00001f);

        }

        public static void Object_draw_Postfix2(Object __instance, SpriteBatch spriteBatch, int x, int y)
        {
            if (__instance == null || !__instance.Name?.EndsWith("Pet Bowl") == true || !__instance.modData.ContainsKey("aedenthorn.PetBowl/Watered") || __instance.modData["aedenthorn.PetBowl/Watered"] != "true")
                return;

            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64)));

            float draw_layer = Math.Max(0f, ((y + 1) * 64 - 24 + 1) / 10000f) + (x + 1) * 1E-05f;

            spriteBatch.Draw(waterTexture, position, new Rectangle(0, 0, waterTexture.Width, waterTexture.Height), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, draw_layer);

        }

    }
}