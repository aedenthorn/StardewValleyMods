using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace LoadMenuCache
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(LoadGameMenu.SaveFileSlot), "drawSlotFarmer")]
        public class LoadGameMenu_SaveFileSlot_FindSaveGames_Patch
        {
            public static bool Prefix(LoadGameMenu.SaveFileSlot __instance, SpriteBatch b, int i, LoadGameMenu ___menu)
            { 
                if(!Config.ModEnabled || !textureDict.TryGetValue(__instance.Farmer.slotName, out Texture2D tex))  
                    return true;
                var offset = new Vector2(92f, 20f);
                var pos = new Vector2(___menu.slotButtons[i].bounds.X + offset.X, ___menu.slotButtons[i].bounds.Y + offset.Y);
                b.Draw(tex, pos, null, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.8f);
                return false;
            }
        }
        [HarmonyPatch(typeof(LoadGameMenu), "startListPopulation")]
        public class LoadGameMenu_startListPopulation_Patch
        {
            public static void Prefix()
            {
                SMonitor.Log("startListPopulation");
            }
        }
        [HarmonyPatch(typeof(LoadGameMenu), "FindSaveGames")]
        public class LoadGameMenu_FindSaveGames_Patch
        {
            public static bool Prefix(ref List<Farmer> __result, string filter)
            {
                if (!Config.ModEnabled)
                    return true;
                SMonitor.Log("FindSaveGames");
                List<Farmer> results = new List<Farmer>();
                string pathToDirectory = Path.Combine(new string[]
                {
                Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley"), "Saves")
                });
                if (Directory.Exists(pathToDirectory))
                {
                    foreach (string s in Directory.EnumerateDirectories(pathToDirectory).ToList())
                    {
                        string saveName = Path.GetFileName(s);
                        string pathToFile = Path.Combine(pathToDirectory, saveName, "SaveGameInfo");
                        if (File.Exists(Path.Combine(pathToDirectory, saveName, "LoadMenuCache", "data.json")) && File.Exists(Path.Combine(pathToDirectory, saveName, "LoadMenuCache", "portrait.png")))
                        {
                            Stopwatch stopwatch = Stopwatch.StartNew();
                            FarmerData data = JsonConvert.DeserializeObject<FarmerData>(File.ReadAllText(Path.Combine(pathToDirectory, saveName, "LoadMenuCache", "data.json")));
                            Farmer f = new Farmer() 
                            {
                                slotName = saveName,
                                Name = data.Name,
                                gameVersion = data.gameVersion,
                                dayOfMonthForSaveGame = data.dayOfMonthForSaveGame,
                                seasonForSaveGame = data.seasonForSaveGame,
                                yearForSaveGame = data.yearForSaveGame,
                                dateStringForSaveGame = data.dateStringForSaveGame,
                                millisecondsPlayed = data.millisecondsPlayed
                            };
                            f.team.SetIndividualMoney(f, data.Money);
                            f.farmName.Value = data.farmName;
                            textureDict[saveName] = Texture2D.FromFile(Game1.graphics.GraphicsDevice, Path.Combine(pathToDirectory, saveName, "LoadMenuCache", "portrait.png"));
                            results.Add(f);
                            SMonitor.Log($"Loaded {saveName} in {stopwatch.ElapsedMilliseconds}ms");
                        }
                        else if (File.Exists(Path.Combine(pathToDirectory, saveName, saveName)))
                        {
                            Farmer f = null;
                            try
                            {
                                using (FileStream stream = File.OpenRead(pathToFile))
                                {
                                    f = (Farmer)SaveGame.farmerSerializer.Deserialize(stream);
                                    SaveGame.loadDataToFarmer(f);
                                    f.slotName = saveName;
                                    results.Add(f);
                                }

                                var folder = Path.Combine(pathToDirectory, saveName, "LoadMenuCache");
                                Directory.CreateDirectory(folder);

                                FarmerData data = new FarmerData()
                                {
                                    slotName = f.slotName,
                                    Name = f.Name,
                                    gameVersion = f.gameVersion,
                                    dayOfMonthForSaveGame = f.dayOfMonthForSaveGame,
                                    seasonForSaveGame = f.seasonForSaveGame,
                                    yearForSaveGame = f.yearForSaveGame,
                                    dateStringForSaveGame = f.dateStringForSaveGame,
                                    farmName = f.farmName.Value,
                                    Money = f.Money,
                                    millisecondsPlayed = f.millisecondsPlayed
                                };
                                File.WriteAllText(Path.Combine(folder, "data.json"), JsonConvert.SerializeObject(data, Formatting.Indented));

                                var ort = Game1.graphics.GraphicsDevice.GetRenderTargets();
                                var rt = new RenderTarget2D(Game1.graphics.GraphicsDevice, 64, 128);
                                Game1.graphics.GraphicsDevice.SetRenderTarget(rt);
                                Game1.graphics.GraphicsDevice.Clear(Color.Transparent);
                                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, null);
                                f.FarmerRenderer.draw(Game1.spriteBatch, new FarmerSprite.AnimationFrame(0, 0, false, false, null, false), 0, new Rectangle(0, 0, 16, 32), Vector2.Zero, Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, f);
                                Game1.spriteBatch.End();
                                Stream stream2 = File.Create(Path.Combine(folder, "portrait.png"));
                                rt.SaveAsPng(stream2, 64, 128);
                                Game1.graphics.GraphicsDevice.SetRenderTargets(ort);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Exception occured trying to access file '{0}'", pathToFile);
                                Console.WriteLine(ex.GetBaseException().ToString());
                                if (f != null)
                                {
                                    f.unload();
                                }
                            }
                        }
                    }
                }
                results.Sort();
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    for (int i = 0; i < results.Count; i++)
                    {
                        Farmer farmer = results[i];
                        string name = farmer.Name;
                        if (name != null && name.IndexOfIgnoreCase(filter) == -1)
                        {
                            string value = farmer.farmName.Value;
                            if (value != null && value.IndexOfIgnoreCase(filter) == -1)
                            {
                                results[i] = null;
                            }
                        }
                    }
                }
                __result = results;
                return false;
            }
        }
    }
}