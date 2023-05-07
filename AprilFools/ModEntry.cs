using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xTile;

namespace AprilFools
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static string[] ravenText;
        public static Texture2D blackTexture;
        public static Texture2D beeTexture;
        public static bool asciifying;
        public static bool pixelating;
        public static bool slimeFarmer;
        public static BigSlime slime;
        public static bool beeing;
        public static bool backwardsFarmer;
        public static bool gianting;
        public static List<BeeData> beeDataList = new List<BeeData>();
        public static int ravenTicks;
        public static SpriteFont font;
        public static SpriteBatch screenBatch;
        public static Texture2D screenTexture;

        public static SpeakingAnimalData speakingAnimals;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
            helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
            helper.Events.Player.Warped += Player_Warped;
            helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            helper.Events.Display.Rendered += Display_Rendered;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            ravenText = File.ReadAllLines(Path.Combine(Helper.DirectoryPath, "assets", "raven.txt"));
            blackTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            blackTexture.SetData(new Color[] { Color.Black });

            font = helper.ModContent.Load<SpriteFont>("assets/Fira_Code.xnb");
            beeTexture = helper.ModContent.Load<Texture2D>("assets/bee.png");

            var screenWidth = Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth;
            var screenHeight = Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight;
            screenTexture = new Texture2D(Game1.graphics.GraphicsDevice, screenWidth, screenHeight);
            screenBatch = new SpriteBatch(Game1.graphics.GraphicsDevice);

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Maps/Town") && IsModEnabled() && Config.BuildingsEnabled && !Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater"))
            {
                e.Edit(delegate(IAssetData data)
                {
                    SMonitor.Log($"Patching Maps/Town");
                    Map pierre = Helper.ModContent.Load<Map>("assets/pierrebuilding.tmx");
                    Map joja = Helper.ModContent.Load<Map>("assets/jojabuilding.tmx");
                    data.AsMap().PatchMap(pierre, null, new Rectangle(90, 41, 12, 13), PatchMapMode.Replace);
                    data.AsMap().PatchMap(joja, null, new Rectangle(38, 47, 12, 11), PatchMapMode.Replace);
                });
            }
        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            speakingAnimals = null;
        }

        private void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            if(Config.EnableMod && Config.EnableAnimalTalk && speakingAnimals is null && (Game1.currentLocation is Farm || Game1.currentLocation is AnimalHouse || Game1.currentLocation is Forest))
            {
                FarmAnimal[] animals;
                if (Game1.currentLocation is AnimalHouse)
                    animals = (Game1.currentLocation as AnimalHouse).animals.Values.ToArray();
                else if (Game1.currentLocation is Farm)
                    animals = (Game1.currentLocation as Farm).animals.Values.ToArray();
                else if (Game1.currentLocation is Forest)
                    animals = (Game1.currentLocation as Forest).marniesLivestock.ToArray();
                else return;
                foreach (var a in animals)
                {
                    foreach (var b in animals)
                    {
                        if (a.myID.Value == b.myID.Value)
                            continue;
                        if (Vector2.Distance(a.getTileLocation(), b.getTileLocation()) <= 10 && Game1.random.NextDouble() < 0.3)
                        {
                            speakingAnimals = new SpeakingAnimalData(a, b);
                            return;
                        }
                    }
                }
            }
        }


        private void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            if (!IsModEnabled())
                return;
            if (slimeFarmer)
            {
                if(!Config.SlimeEnabled || Game1.random.NextDouble() < 0.1)
                {
                    slimeFarmer = false;
                }
                
            }
            else if(Config.SlimeEnabled)
            {
                slimeFarmer = Game1.random.NextDouble() < 0.01;
                if (slimeFarmer)
                {
                    if (slime == null)
                        slime = new BigSlime(Game1.player.Position, 121);
                }
            }
            if (backwardsFarmer)
            {
                if(!Config.BackwardsEnabled ||Game1.random.NextDouble() < 0.1)
                {
                    backwardsFarmer = false;
                }
                
            }
            else if (Config.BackwardsEnabled)
            {
                backwardsFarmer = Game1.random.NextDouble() < 0.008;
            }
            if (gianting)
            {
                if(Config.GiantEnabled || Game1.random.NextDouble() < 0.1)
                {
                    gianting = false;
                }
                
            }
            else if (Config.GiantEnabled)
            {
                gianting = Game1.random.NextDouble() < 0.01;
            }
            if(asciifying)
            {
                if(!Config.AsciiEnabled || Game1.random.NextDouble() < 0.1)
                {
                    asciifying = false;
                }

            }
            else if(pixelating)
            {
                if(!Config.PixelateEnabled || Game1.random.NextDouble() < 0.1)
                {
                    pixelating = false;
                }

            }
            else
            {
                if (Config.PixelateEnabled)
                    pixelating = Game1.random.NextDouble() < 0.01;
                if (!pixelating && Config.AsciiEnabled)
                    asciifying = Game1.random.NextDouble() < 0.008;
                else
                    asciifying = false;
            }
            if (!Config.BeesEnabled || beeDataList.Count > 30)
                beeDataList.Clear();
            else if (Game1.random.NextDouble() < (beeDataList.Count + 1) / 30f)
            {
                beeDataList.Add(new BeeData()
                {
                    pos = new Vector2(Game1.random.Next(Game1.viewport.Width), Game1.random.Next(Game1.viewport.Height))
                });
            }
        }

        private void Display_Rendered(object sender, StardewModdingAPI.Events.RenderedEventArgs e)
        {
            if (!IsModEnabled())
                return;

            if (Config.BeesEnabled && beeDataList.Count > 0)
            {
                for (int i = beeDataList.Count - 1; i >= 0; i--)
                {
                    var beeDirection = beeDataList[i].dir;
                    var beeAngle = beeDataList[i].angle;
                    var beePos = beeDataList[i].pos;

                    int size = 64;
                    if (beeDirection != Vector2.Zero)
                    {
                        int period = 36;
                        if (Game1.random.NextDouble() < 0.01)
                            beeDirection = Vector2.Zero;
                        beeDataList[i].ticks++;
                        var which = beeDataList[i].ticks % period;
                        beeDataList[i].currentSprite = which < period / 4 ? 0 : ((which < period  * 3 / 4 && which >= period / 2) ? 2 : 1);
                    }
                    else
                    {
                        beeDataList[i].ticks = 0;
                        if (Game1.random.NextDouble() < 0.01)
                        {
                            beeAngle = Game1.random.NextDouble() * 2 * Math.PI;
                            beeDirection = new Vector2((float)Math.Cos(beeAngle), (float)Math.Sin(beeAngle));
                            beeDirection.Normalize();
                        }
                    }
                    beePos += beeDirection;
                    if (beePos.X < -size || beePos.Y < -size || beePos.X > Game1.viewport.Width + size / 2 || beePos.Y > Game1.viewport.Height + size / 2)
                    {
                        beeDataList.RemoveAt(i);
                        continue;
                    }
                    e.SpriteBatch.Draw(beeTexture, new Rectangle(Utility.Vector2ToPoint(beePos), new Point(size, size)), new Rectangle(beeDataList[i].currentSprite * 512, 0, 512, 512), Color.White, (float)(beeAngle + Math.PI / 2), new Vector2(256, 256), SpriteEffects.None, 1);
                    beeDataList[i].dir = beeDirection;
                    beeDataList[i].angle = beeAngle;
                    beeDataList[i].pos = beePos;
                }

            }
        }


        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!IsModEnabled())
                return;

            if (Config.AsciiEnabled && asciifying)
            {
                int scale = 16;
                var lines = ConvertToAscii(ScaleScreen(scale, out int w, out int h), Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth / scale);
                if (lines.Count > 0)
                {
                    e.SpriteBatch.Draw(blackTexture, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.White);
                    int height = Game1.viewport.Height / lines.Count;
                    for (int i = 0; i < lines.Count; i++)
                    {
                        e.SpriteBatch.DrawString(font, lines[i], new Vector2(0, height * i), Color.White, 0, Vector2.Zero, 0.8f, SpriteEffects.None, 1);
                    }
                }
            }
            else if (Config.PixelateEnabled && pixelating)
            {
                Color[] data = ScaleScreen(16, out int width, out int height);
                Texture2D pixelScreen = new Texture2D(Game1.graphics.GraphicsDevice, width, height);
                pixelScreen.SetData(data);
                e.SpriteBatch.Draw(pixelScreen, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.White);
            }
            if (Config.RavenEnabled && (Game1.timeOfDay >= 2100 || Game1.timeOfDay <= 200))
            {
                for(int i = 0; i < ravenText.Length; i++)
                {
                    var y = Game1.viewport.Height - ravenTicks / 2 + i * 48;
                    if (y < -48)
                        continue;
                    if (y > Game1.viewport.Height)
                        break;
                    e.SpriteBatch.DrawString(Game1.dialogueFont, ravenText[i],new Vector2(0, y), Color.DarkGray * 0.75f);
                    e.SpriteBatch.DrawString(Game1.dialogueFont, ravenText[i],new Vector2(2, y + 2), Color.Black * 0.75f);
                }
                ravenTicks++;
            }
            else
            {
                ravenTicks = 0;
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

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_OnlyAprilFirst_Name"),
                getValue: () => Config.RestrictToAprilFirst,
                setValue: value => Config.RestrictToAprilFirst = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_EnableBees_Name"),
                getValue: () => Config.BeesEnabled,
                setValue: value => Config.BeesEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_EnableBackwards_Name"),
                getValue: () => Config.BackwardsEnabled,
                setValue: value => Config.BackwardsEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_EnableAscii_Name"),
                getValue: () => Config.AsciiEnabled,
                setValue: value => Config.AsciiEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_EnablePixelate_Name"),
                getValue: () => Config.PixelateEnabled,
                setValue: value => Config.PixelateEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_EnableTreeScreams_Name"),
                getValue: () => Config.TreeScreamEnabled,
                setValue: value => Config.TreeScreamEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_EnableInventoryAvoid_Name"),
                getValue: () => Config.InventoryEnabled,
                setValue: value => Config.InventoryEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_EnableSlime_Name"),
                getValue: () => Config.SlimeEnabled,
                setValue: value => Config.SlimeEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_EnableRaven_Name"),
                getValue: () => Config.RavenEnabled,
                setValue: value => Config.RavenEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_EnableGiants_Name"),
                getValue: () => Config.GiantEnabled,
                setValue: value => Config.GiantEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_EnableBuildingSwitch_Name"),
                getValue: () => Config.BuildingsEnabled,
                setValue: value => Config.BuildingsEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_EnableAnimalGreeting_Name"),
                getValue: () => Config.EnableAnimalTalk,
                setValue: value => Config.EnableAnimalTalk = value
            );
        }


    }
}