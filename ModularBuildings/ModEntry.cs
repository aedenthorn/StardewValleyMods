using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;

namespace ModularBuildings
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetLoader
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        private static bool isBuilding;
        private static Rectangle buildingRect;
        private string dictPath;
        private Harmony harmony;
        private Dictionary<string, ModularBuildingPart> buildingPartDict;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.EnableMod)
                return;

            context = this;
            dictPath = $"{ModManifest.UniqueID}/dict";

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

            harmony = new Harmony(ModManifest.UniqueID);
            /*
            harmony.Patch(
               original: AccessTools.Constructor(typeof(CraftingPage), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(List<Chest>) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.CraftingPage_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(CraftingPage), nameof(CraftingPage.receiveLeftClick)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.CraftingPage_receiveLeftClick_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Constructor(typeof(InventoryMenu), new Type[] { typeof(int), typeof(int), typeof(bool), typeof(IList<Item>), typeof(InventoryMenu.highlightThisItem), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.InventoryMenu_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), nameof(Game1.drawDialogueBox), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(string), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(int) }),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Game1_drawDialogueBox_Postfix))
            );
            */

        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            buildingPartDict = SHelper.Content.Load<Dictionary<string, ModularBuildingPart>>(dictPath, ContentSource.GameContent);
            foreach(var key in buildingPartDict.Keys)
            {
                buildingPartDict[key].texture = Helper.Content.Load<Texture2D>(buildingPartDict[key].texturePath, ContentSource.GameContent);
            }
            Monitor.Log($"Loaded {buildingPartDict.Count} building parts.");
        }

        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if(Config.EnableMod && isBuilding)
            {
                if (Helper.Input.IsDown(Config.BuildKey) || Helper.Input.IsSuppressed(Config.BuildKey))
                {
                    buildingRect.Size = Utility.Vector2ToPoint(Game1.lastCursorTile - buildingRect.Location.ToVector2() + new Vector2(1,1));
                    for(int x = buildingRect.X; x < buildingRect.X + buildingRect.Width; x++)
                    {
                        for(int y = buildingRect.Y; y < buildingRect.Y + buildingRect.Height; y++)
                        {
                            e.SpriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2(x * 64, y * 64)), new Rectangle?(new Rectangle(194, 388, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                        }
                    }
                    Helper.Input.Suppress(Config.BuildKey);
                }
                else
                {
                    BuildBuilding();
                    isBuilding = false;
                    buildingRect = new Rectangle();
                }
            }
            else
            {
                isBuilding = false;
                buildingRect = new Rectangle();
            }
            if (Game1.currentLocation.modData.TryGetValue($"{ModManifest.UniqueID}/index", out string indexString) && indexString.Length > 0)
            {
                string[] parts = indexString.Split('|');
                foreach(var part in parts)
                {
                    DrawBuilding(e.SpriteBatch, new ModularBuilding(Game1.currentLocation.modData[$"{ModManifest.UniqueID}/{part}"]));
                }
            }
        }

        private void DrawBuilding(SpriteBatch sb, ModularBuilding b)
        {
            var wallName = b.wall;
            if (wallName == null || wallName == "")
            {
                foreach(var kvp in buildingPartDict)
                {
                    if (kvp.Value.partType == "wall")
                    {
                        wallName = kvp.Key;
                        break;
                    }
                }
            }
            var roofName = b.roof;
            if (roofName == null || roofName == "")
            {
                foreach(var kvp in buildingPartDict)
                {
                    if (kvp.Value.partType == "roof")
                    {
                        roofName = kvp.Key;
                        break;
                    }
                }
            }
            var gableName = b.gable;
            if (gableName == null || gableName == "")
            {
                foreach(var kvp in buildingPartDict)
                {
                    if (kvp.Value.partType == "gable")
                    {
                        gableName = kvp.Key;
                        break;
                    }
                }
            }

            buildingPartDict.TryGetValue(wallName, out ModularBuildingPart wall);
            buildingPartDict.TryGetValue(roofName, out ModularBuildingPart roof);
            buildingPartDict.TryGetValue(gableName, out ModularBuildingPart gable);
            if (wall == null || roof == null || gable == null)
                return;

            // draw wall

            int textureHeight = wall.texture.Height / wall.tileSize;
            int textureWidth = wall.texture.Width / wall.tileSize;
            int wallHeight = textureHeight;
            int xStart = b.rect.X;
            int yStart = b.rect.Y + b.rect.Height - wallHeight;
            for(int x = xStart; x < xStart + b.rect.Width; x++)
            {
                for(int y = yStart; y < yStart + textureHeight; y++)
                {
                    int index = x - xStart;
                    int wallX;
                    if (index == 0)
                        wallX = 0;
                    else if(index == b.rect.Width - 1)
                        wallX = textureWidth - 1;
                    else
                        wallX = 1 + GetIndex(index - 1, b.rect.Width - 2, wall.repeatX, textureWidth - 2);

                    sb.Draw(wall.texture, new Rectangle(Utility.Vector2ToPoint(Game1.GlobalToLocal(new Vector2(x, y) * 64)), new Point(64, 64)), new Rectangle(wallX * wall.tileSize, (y - yStart) * wall.tileSize, wall.tileSize, wall.tileSize), Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.1f);
                }
            }

            // get roof stuff first

            int roofWidth = b.rect.Width;
            float offsetX = 0;
            float offsetY = 0;
            if (b.rect.Width % 2 == 1) // fix odd width
            {
                roofWidth++;
                offsetX = 0.5f;
                offsetY = 0.5f;
            }
            int roofHeight = Math.Max(roofWidth, textureWidth) / 2 - 1 + Math.Max(b.rect.Height, textureHeight);

            // draw gable

            textureHeight = gable.texture.Height / gable.tileSize;
            textureWidth = gable.texture.Width / gable.tileSize;
            int gableWidth = roofWidth;
            int gableHeight = roofHeight - b.rect.Height;
            xStart = b.rect.X;
            yStart = b.rect.Y + b.rect.Height - wallHeight - gableHeight;
            for (int x = xStart; x < xStart + gableWidth; x++)
            {
                for (int y = yStart; y < yStart + gableHeight; y++)
                {
                    int indexX = x - xStart;
                    int indexY = y - yStart;
                    if (indexX < gableWidth / 2 && indexY < gableWidth / 2 - indexX - 1)
                        continue;
                    else if (indexX >= gableWidth / 2 && indexY < indexX - gableWidth / 2)
                        continue;
                    int gableX = GetIndex(x - xStart, gableWidth, gable.repeatX, textureWidth);
                    int gableY = GetIndex(y - yStart, gableWidth, gable.repeatY, textureWidth);
                    sb.Draw(gable.texture, new Rectangle(Utility.Vector2ToPoint(Game1.GlobalToLocal(new Vector2(x - offsetX, y) * 64)), new Point(64, 64)), new Rectangle(gableX * gable.tileSize, gableY * gable.tileSize, gable.tileSize, gable.tileSize), Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.1f);
                }
            }

            // draw roof

            textureHeight = roof.texture.Height / roof.tileSize;
            textureWidth = roof.texture.Width / roof.tileSize;


            xStart = b.rect.X;
            yStart = b.rect.Y + b.rect.Height - wallHeight - roofHeight + 1;
            for(int x = xStart; x < xStart + roofWidth; x++)
            {
                for(int y = yStart; y < yStart + roofHeight; y++)
                {
                    int roofX;
                    int roofY;
                    int indexX = x - xStart;
                    int indexY = y - yStart;
                    if(roof.angle == 45)
                    {
                        int border;
                        if (indexX < roofWidth / 2)
                        {
                            border = roofWidth / 2 - indexX - 1;
                        }
                        else
                        {
                            border = indexX - roofWidth / 2;
                        }
                        if (indexY < border)
                            continue;
                        else if (indexY > border + b.rect.Height - 1)
                            continue;
                        if (indexY == border)
                            roofY = 0;
                        else if (indexY == border + b.rect.Height - 1)
                            roofY = textureHeight - 1;
                        else
                        {
                            roofY = 1 + GetIndex(indexY - border - 1, roofHeight - 2, roof.repeatY, textureHeight - 2);
                        }

                        if (indexX == 0)
                        {
                            roofX = 0;
                        }
                        else if (indexX == roofWidth - 1)
                        {
                            roofX = textureWidth - 1;
                        }
                        else if (indexX == roofWidth / 2 - 1)
                        {
                            roofX = textureWidth / 2 - 1;
                        }
                        else if(indexX == roofWidth / 2)
                        {
                            roofX = textureWidth / 2;
                        }
                        else
                        {
                            if (indexX < roofWidth / 2)
                            {
                                roofX = 1 + GetIndex(indexX - 1, roofWidth / 2 - 2, roof.repeatX, textureWidth / 2 - 2);
                            }
                            else
                            {
                                roofX = textureWidth / 2 + 1 + GetIndex(indexX - roofWidth / 2 - 1, roofWidth / 2 - 2, roof.repeatX, textureWidth / 2 - 2);
                            }
                        }
                        var zx = border;
                    }
                    else
                    {
                        return;
                    }
                    sb.Draw(roof.texture, new Rectangle(Utility.Vector2ToPoint(Game1.GlobalToLocal(new Vector2(x - offsetX, y + offsetY) * 64)), new Point(64, 64)), new Rectangle(roofX * roof.tileSize, roofY * roof.tileSize, roof.tileSize, roof.tileSize), Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.2f);
                }
            }

        }

        private int GetIndex(int index, int size, string type, int limiter)
        {
            switch (type)
            {
                case "linear":
                    return index % limiter;
                case "symmetrical":
                    if (index < size / 2)
                        return index % limiter;
                    else
                        return (size - index - 1) % limiter;
            }
            return index % limiter;
        }

        private void BuildBuilding()
        {
            Game1.playSound("yoba");
            var building = new ModularBuilding(buildingRect);
            Game1.currentLocation.modData[$"{ModManifest.UniqueID}/{buildingRect.X},{buildingRect.Y}"] = building.GetBuildingString();
            if (Game1.currentLocation.modData.TryGetValue($"{ModManifest.UniqueID}/index", out string indexString) && indexString.Length > 0)
            {
                indexString += $"|{buildingRect.X},{buildingRect.Y}";
            }
            else
            {
                indexString = $"{buildingRect.X},{buildingRect.Y}";
            }

            Game1.currentLocation.modData[$"{ModManifest.UniqueID}/index"] = indexString;
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if(e.Button == Config.BuildKey && Helper.Input.IsDown(Config.ModKey) && Context.CanPlayerMove)
            {
                isBuilding = true;
                buildingRect = new Rectangle(Utility.Vector2ToPoint(Game1.lastCursorTile), new Point(1, 1));
                Helper.Input.Suppress(Config.BuildKey);
            }
        }


        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (!Config.EnableMod)
                return false;

            return asset.AssetNameEquals(dictPath);
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            Monitor.Log("Loading dictionary");

            return (T)(object)new Dictionary<string, ModularBuildingPart>();
        }
    }
}