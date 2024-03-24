using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;
using xTile.Layers;
using xTile.Tiles;

namespace BedTweaks
{
    public class ModEntry : Mod
    {
        public static IModHelper SHelper;
        private static int PIXELS_PER_TILE = 16;
        public static ModConfig Config;
        private Dictionary<string, Texture2D> managedTextures;
        public override void Entry(IModHelper helper)
        {
            ModEntry.SHelper = helper;
            Config = helper.ReadConfig<ModConfig>();
            managedTextures = new Dictionary<string, Texture2D>();
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
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

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            /*configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Redraw Middle Pillows",
                getValue: () => Config.RedrawMiddlePillows,
                setValue: value => Config.RedrawMiddlePillows = value
            );*/
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Bed Width",
                tooltip: () => "Width of the bed, in tiles. Will only update on game restart. Collission may (will) be incorrect; pick up and replace the bed, and colission will be fixed on all future loads.",
                getValue: () => Config.BedWidth,
                setValue: value => Config.BedWidth = value,
                min: 3
            );
            /*configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Sheet Opacity %",
                getValue: () => (int)(Config.SheetTransparency * 100),
                setValue: value => Config.SheetTransparency = value / 100f,
                min: 1,
                max: 100
            );*/
        }

        public override object GetApi()
        {
            return new BedTweaksAPI();
        }


        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {

            if (e.NameWithoutLocale.IsEquivalentTo("Data\\Furniture"))
            {
                Texture2D furnitureTextureSheet = SHelper.GameContent.Load<Texture2D>("TileSheets\\furniture");
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, string>().Data;
                    List<string[]> bedIds = new List<string[]>();

                    foreach(var kvp in data)
                    {
                        // If the data for this has already been modified, don't do it again. Idk why this would be the case, but this method takes a long time and it would suck if it ran while the game was running.
                        if (this.managedTextures.ContainsKey("BedTweaks/" + kvp.Key))
                        {
                            continue;
                        }

                        string[] values = kvp.Value.Split('/');

                        if (values[1] == "bed double")
                        {
                            string id = "(F)" + kvp.Key;

                            int bedWidthTiles = Config.BedWidth;
                            //Field 2 is the size in tiles of the texture
                            values[2] = bedWidthTiles + " " + values[2].Split(' ')[1]; //Replaces width but keeps height

                            //Field 3 is the size in tiles of the physical bed (for collision)
                            values[3] = bedWidthTiles + " " + values[3].Split(' ')[1]; //Replaces width but keeps height

                            this.Monitor.Log("Generating texture for " + values[0], LogLevel.Trace);
                            Rectangle spriteLocation = ItemRegistry.GetData(id).GetSourceRect();
                            Color[,] bedSprite; 

                            if(values.Length < 10 || values[9] == "") // If no texture sheet is specified, gets texture from the default sheet
                            {
                                bedSprite = getImageSubsection(furnitureTextureSheet, spriteLocation.Left, spriteLocation.Top, spriteLocation.Width * 2, spriteLocation.Height);
                            }
                            else // Gets the sprite from the specified sprite sheet.
                            {
                                bedSprite = getImageSubsection(SHelper.GameContent.Load<Texture2D>(values[9]), spriteLocation.Left, spriteLocation.Top, spriteLocation.Width * 2, spriteLocation.Height);
                            }
                            Texture2D newSprite = generateBedSprite(bedSprite, Config.BedWidth, kvp.Key);

                            managedTextures["BedTweaks/" + kvp.Key] = newSprite;

                            bedIds.Add(new string[]{kvp.Key, string.Join("/", generateNewBedValues(kvp.Key, values))});
                        }
                    }

                    foreach(string[] kvp in bedIds)
                    {
                        data[kvp[0]] = kvp[1];
                    }
                }, AssetEditPriority.Late);
            }else if(managedTextures.ContainsKey(e.NameWithoutLocale.BaseName))
            {
                e.LoadFrom(() => { return managedTextures[e.NameWithoutLocale.BaseName]; }, AssetLoadPriority.Medium);
            }else if (e.NameWithoutLocale.BaseName.StartsWith("BedTweaks"))
            {
                this.Monitor.Log("Requested asset \"" + e.NameWithoutLocale.BaseName + "\" that is not registered!", LogLevel.Debug);
            }
        }

        private static Color[,] getImageSubsection(Texture2D textureData, int startX, int startY, int width, int height) 
        {
            Color[,] output = new Color[height, width];
            Color[][] subsection = new Color[height][];
            for(int i = 0; i < height; i++)
            {
                subsection[i] = new Color[width];
                textureData.GetData(0, 0, new Rectangle(startX, startY + i, width, 1), subsection[i], 0, width);
            }
            output = new Color[height, width];
            for(int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    output[i, j] = subsection[i][j];
                }
            }
            return output;
        }

        private static void copyFrom<T>(T[,] source, T[,] target, int sourceStartY, int sourceStartX, int targetStartY, int targetStartX, int height, int width)
        {
            for(int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    target[targetStartY + i, targetStartX + j] = source[sourceStartY + i, sourceStartX + j];
                }
            }
        }

        private static Texture2D generateBedSprite(Color[,] baseSprite, int newTileWidth, string id)
        {
            Color[,] finalSprite = new Color[baseSprite.GetLength(0), newTileWidth * PIXELS_PER_TILE * 2];

            // Copy the left vertical tile
            copyFrom<Color>(baseSprite, finalSprite, 0, 0, 0, 0, baseSprite.GetLength(0), PIXELS_PER_TILE);
            copyFrom<Color>(baseSprite, finalSprite, 0, baseSprite.GetLength(1)/2, 0, PIXELS_PER_TILE*newTileWidth, baseSprite.GetLength(0), PIXELS_PER_TILE);

            // Copy from right vertical tile
            copyFrom<Color>(baseSprite, finalSprite, 0, baseSprite.GetLength(1)/2 - PIXELS_PER_TILE, 0, (newTileWidth-1) * PIXELS_PER_TILE, baseSprite.GetLength(0), PIXELS_PER_TILE);
            copyFrom<Color>(baseSprite, finalSprite, 0, baseSprite.GetLength(1) - PIXELS_PER_TILE, 0, (newTileWidth*2 - 1) * PIXELS_PER_TILE, baseSprite.GetLength(0), PIXELS_PER_TILE);

            // Duplicate the second vertical tile from the source into the middle tiles of the new
            for(int i = 0; i < newTileWidth-2; i++)
            {
                copyFrom<Color>(baseSprite, finalSprite, 0, PIXELS_PER_TILE, 0, (1+i)*PIXELS_PER_TILE, baseSprite.GetLength(0), PIXELS_PER_TILE);
                copyFrom<Color>(baseSprite, finalSprite, 0, baseSprite.GetLength(1)/2+PIXELS_PER_TILE, 0, PIXELS_PER_TILE * (newTileWidth+1+i), baseSprite.GetLength(0), PIXELS_PER_TILE);
            }


            ModEntry.doPillows(baseSprite, finalSprite, newTileWidth, id);

            Color[] finalSpriteOneLine = new Color[finalSprite.Length];
            for(int i = 0; i <  finalSprite.GetLength(0); i++)
            {
                int rowStart = i*finalSprite.GetLength(1);
                for(int j = 0; j < finalSprite.GetLength(1); j++)
                {
                    finalSpriteOneLine[rowStart + j] = finalSprite[i, j];
                }
            }

            Texture2D output = new Texture2D(Game1.graphics.GraphicsDevice, finalSprite.GetLength(1), finalSprite.GetLength(0));
            output.SetData<Color>(0, new Rectangle(0, 0, finalSprite.GetLength(1), finalSprite.GetLength(0)), finalSpriteOneLine, 0, finalSpriteOneLine.Length);
            return output;
        }

        private static void doPillows(Color[,] baseSprite, Color[,] finalSprite, int newTileWidth, string itemId)
        {
            PillowData pillowData = PillowManager.getPillowData(itemId);
            if(!pillowData.shouldRedraw)
            {
                return;
            }

            //Redraws the pillows on the borders of the middle tiles.
            //Like, half is in one tile and half in the other

            for (int i = 2; i < newTileWidth-1; i++) // i is the tile to draw the pillow at the end of
            {
                int pixelOffset = (3*(i-2))/(newTileWidth-3);// For some reason the default left pillow is 6 pixels from the end, while the default right is five. This helps smooth out the differences. Value 0-2.
                copyFrom<Color>(baseSprite, finalSprite, pillowData.startY, pillowData.startX, pillowData.startY, PIXELS_PER_TILE*i - PIXELS_PER_TILE/2 + pixelOffset, pillowData.height, pillowData.width);
            }
        }

        private static string[] generateNewBedValues(string id, string[] oldValues)
        {
            string[] output;
            if(oldValues.Length < 10)
            {
                output = new string[10];
                for(int i = 0; i < oldValues.Length; i++)
                {
                    output[i] = oldValues[i];
                }
            }
            else
            {
                output = oldValues;
            }
            output[8] = "0";//Sprite index (does not matter because we're handling it manually)
            output[9] = "BedTweaks" + "\\" + id;//Custom texture
            return output;
        }
    }
}