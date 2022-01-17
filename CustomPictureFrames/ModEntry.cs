using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CustomPictureFrames
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetLoader
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public int currentFrame;
        public static List<FrameData> frameList = new List<FrameData>();
        public static Dictionary<string, List<Texture2D>> pictureDict = new Dictionary<string, List<Texture2D>>();
        public bool framing = false;
        private Color oldAmbientLight;
        private bool takingPicture;
        public static readonly string frameworkPath = "custom_picture_frame_dictionary";

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Display.RenderedWorld += Display_RenderedWorld;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.draw), new Type[] {typeof(SpriteBatch)}),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(GameLocation_draw_Postfix))
            );
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (takingPicture)
            {
                TakePicture();
                Game1.ambientLight = oldAmbientLight;
                takingPicture = false;
                Helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
            }
        }

        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {

            if (Config.EnableMod && framing && frameList.Count > 0)
            {
                var pos = Game1.getMousePosition() - new Point(frameList[currentFrame].texture.Width * 4, frameList[currentFrame].texture.Width * 4);
                if (pos.X < 0)
                    pos.X = 0;
                if (pos.Y < 0)
                    pos.Y = 0;

                e.SpriteBatch.Draw(frameList[currentFrame].texture, Utility.PointToVector2(pos), null, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, 1);
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
                name: () => "Mod Enabled?",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Start Framing Key",
                getValue: () => Config.StartFramingKey,
                setValue: value => Config.StartFramingKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Switch Frame Key",
                getValue: () => Config.SwitchFrameKey,
                setValue: value => Config.SwitchFrameKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Take Picture Key",
                getValue: () => Config.TakePictureKey,
                setValue: value => Config.TakePictureKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Switch Picture Key",
                getValue: () => Config.SwitchPictureKey,
                setValue: value => Config.SwitchPictureKey = value
            );

        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            frameList.Clear();
            foreach (var kvp in Helper.Content.Load<Dictionary<string, string>>(frameworkPath, ContentSource.GameContent))
            {
                var tex = Helper.Content.Load<Texture2D>(kvp.Value, ContentSource.GameContent);
                tex.Name = Path.GetFileName(kvp.Key);
                frameList.Add(new FrameData() { texture = tex, name = kvp.Key });
            }

            framing = false;
            pictureDict.Clear();
            if (!Directory.Exists(Path.Combine(Helper.DirectoryPath, "pictures")))
                Directory.CreateDirectory(Path.Combine(Helper.DirectoryPath, "pictures"));
            if (!Directory.Exists(Path.Combine(Helper.DirectoryPath, "pictures", Constants.SaveFolderName)))
                Directory.CreateDirectory(Path.Combine(Helper.DirectoryPath, "pictures", Constants.SaveFolderName));
            foreach(var f in Directory.GetDirectories(Path.Combine(Helper.DirectoryPath, "pictures", Constants.SaveFolderName)))
            {
                var folderName = Path.GetFileName(f);
                {
                    List<Texture2D> textures = new List<Texture2D>();
                    foreach (var file in Directory.GetFiles(f, "*.png"))
                    {
                        string name = Path.GetFileNameWithoutExtension(file);
                        var tex = Texture2D.FromFile(Game1.graphics.GraphicsDevice, file);
                        textures.Add(tex);
                        Monitor.Log($"added picture {name}");
                    }
                    if(textures.Count > 0)
                        pictureDict.Add(folderName, textures);
                }
            }
        }
        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsWorldReady || frameList.Count == 0)
                return;
            if(!framing && e.Button == Config.SwitchPictureKey)
            {
                foreach (var f in Game1.currentLocation.furniture)
                {
                    if (f.boundingBox.Value.Contains(Game1.viewport.X + Game1.getOldMouseX(), Game1.viewport.Y + Game1.getOldMouseY()))
                    {
                        string key;
                        if (pictureDict.ContainsKey(f.Name))
                            key = f.Name;
                        else if (f.Name.Contains("/") && pictureDict.ContainsKey(f.Name.Split('/')[1]))
                            key = f.Name.Split('/')[1];
                        else
                            continue;
                        int index = 0;
                        if (f.modData.ContainsKey("aedenthorn.CustomPictureFrames/index"))
                            index = int.Parse(f.modData["aedenthorn.CustomPictureFrames/index"])+1;
                        if (index >= pictureDict[key].Count)
                            index = -1;
                        f.modData["aedenthorn.CustomPictureFrames/index"] = index.ToString();
                        Monitor.Log($"Set picture index for {f.Name} to {index}");
                        Helper.Input.Suppress(e.Button);
                        return;
                    }
                }
            }
            if(framing && e.Button == SButton.Escape)
            {
                framing = false;
                return;
            }
            if(e.Button == Config.StartFramingKey)
            {
                framing = !framing;
                return;
            }
            if(framing && e.Button == Config.SwitchFrameKey)
            {
                currentFrame++;
                currentFrame %= frameList.Count;
                Monitor.Log($"switching to frame {frameList[currentFrame].name}");
                Helper.Input.Suppress(Config.SwitchFrameKey);
                return;
            }
            if(framing && e.Button == Config.TakePictureKey)
            {
                Helper.Input.Suppress(e.Button);
                TakePicture();
                //oldAmbientLight = Game1.ambientLight;
                //takingPicture = true;
                //Game1.ambientLight = Color.White;
                //Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            }
        }

        private void TakePicture()
        {
            var picture_name = $"{frameList[currentFrame].name}";

            Monitor.Log($"Saving framed picture {picture_name}");

            var frameWidth = frameList[currentFrame].texture.Width;
            var frameHeight = frameList[currentFrame].texture.Height;

            Color[] frameData = new Color[frameWidth * frameHeight];
            frameList[currentFrame].texture.GetData(frameData);

            // get inner pixels

            bool[] innerPixels = new bool[frameData.Length];
            for (int y = 0; y < frameHeight; y++)
            {
                var transparent = true;
                List<List<int>> transparentPixels = new List<List<int>>();
                transparentPixels.Add(new List<int>());
                for (int x = 0; x < frameWidth; x++)
                {
                    if (frameData[y * frameWidth + x] == Color.Transparent)
                    {
                        if (!transparent)
                        {

                            transparent = true;
                            transparentPixels.Add(new List<int>() { y * frameWidth + x });
                        }
                        else
                        {
                            transparentPixels[transparentPixels.Count - 1].Add(y * frameWidth + x);
                        }
                    }
                    else
                        transparent = false;
                }
                if (!transparent)
                    transparentPixels.Add(new List<int>());
                //Monitor.Log($"{transparentPixels.Count} transparent groups for y = {y}");
                if (transparentPixels.Count > 2)
                {
                    for(int i = 1; i < transparentPixels.Count - 1; i++)
                    {
                        foreach (var p in transparentPixels[i])
                            innerPixels[p] = true;
                    }
                }
            }



            var screenWidth = Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth;
            var screenHeight = Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight;
            var pos = Game1.getMousePosition() - new Point(frameWidth * 4, frameHeight * 4);
            if (pos.X < 0)
                pos.X = 0;
            if (pos.Y < 0)
                pos.Y = 0;

            Color[] screenData = new Color[screenWidth * screenHeight];
            Game1.graphics.GraphicsDevice.GetBackBufferData(screenData);


            Color[] pictureData = new Color[frameWidth * frameHeight * 16];
            //Monitor.Log($"pos {pos}, mousepos {mousePos}, screen {screenWidth},{screenHeight}, framedata {frameData.Length}, pictureData{pictureData.Length}");

            // compose picture
            int innerCount = 0;
            for (int y = pos.Y; y < pos.Y + frameHeight * 4; y++)
            {
                for (int x = pos.X; x < pos.X + frameWidth * 4; x++)
                {
                    int screenI = y * screenWidth + x;
                    int i = (y - pos.Y) * frameWidth * 4 + (x - pos.X);
                    int px = (x - pos.X) / 4;
                    int py = (y - pos.Y) / 4;
                    int frameI = py * frameWidth + px;
                    if (frameI < innerPixels.Length && innerPixels[frameI])
                    {
                        innerCount++;
                        pictureData[i] = screenData[screenI];
                        pictureData[i].A = 255;
                    }
                }
            }

            Monitor.Log($"wrote {innerCount} inner pixels");
            Texture2D tex = new Texture2D(Game1.graphics.GraphicsDevice, frameWidth * 4, frameHeight * 4);
            tex.SetData(pictureData);

            if (!Directory.Exists(Path.Combine(Helper.DirectoryPath, "pictures")))
                Directory.CreateDirectory(Path.Combine(Helper.DirectoryPath, "pictures"));
            if (!Directory.Exists(Path.Combine(Helper.DirectoryPath, "pictures", Constants.SaveFolderName)))
                Directory.CreateDirectory(Path.Combine(Helper.DirectoryPath, "pictures", Constants.SaveFolderName));
            if (!Directory.Exists(Path.Combine(Helper.DirectoryPath, "pictures", Constants.SaveFolderName, picture_name)))
                Directory.CreateDirectory(Path.Combine(Helper.DirectoryPath, "pictures", Constants.SaveFolderName, picture_name));

            int no = 1;

            var file = Path.Combine(Helper.DirectoryPath, "pictures", Constants.SaveFolderName, picture_name, $"{no}.png");

            while (File.Exists(file))
            {
                file = Path.Combine(Helper.DirectoryPath, "pictures", Constants.SaveFolderName, picture_name, $"{++no}.png");
            }

            Stream stream = File.Create(file);
            tex.SaveAsPng(stream, frameWidth * 4, frameHeight * 4);
            stream.Close();
            if (!pictureDict.ContainsKey(picture_name))
                pictureDict[picture_name] = new List<Texture2D>();
            pictureDict[picture_name].Add(tex);
            framing = false;
            if (Config.Message.Length > 0)
            {
                Game1.addHUDMessage(new HUDMessage(string.Format(Config.Message, picture_name)));
            }
        }


        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (!Config.EnableMod)
                return false;

            return asset.AssetNameEquals(frameworkPath);
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            Monitor.Log("Loading frame list");

            return (T)(object)new Dictionary<string, string>();
        }
    }

}