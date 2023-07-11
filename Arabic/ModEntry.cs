using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Arabic
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static Texture2D arabicTexture;
        public static SpriteFont arabicFont;
        private Dictionary<string, Mapping> arabicMap;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            //Helper.Events.Content.AssetRequested += Content_AssetRequested;

            arabicMap = new Dictionary<string, Mapping>();
            XmlReader xmlReader = XmlReader.Create(Path.Combine(Helper.DirectoryPath, "assets", "Arabic.fnt"));
            while (xmlReader.Read())
            {
                if (xmlReader.Name.Equals("char") && (xmlReader.NodeType == XmlNodeType.Element))
                {
                    arabicMap[xmlReader.GetAttribute("id")] = new Mapping()
                    {
                        x = int.Parse(xmlReader.GetAttribute("x")),
                        y = int.Parse(xmlReader.GetAttribute("y")),
                        width = int.Parse(xmlReader.GetAttribute("width")),
                        height = int.Parse(xmlReader.GetAttribute("height")),
                        xo = int.Parse(xmlReader.GetAttribute("xoffset")),
                        yo = int.Parse(xmlReader.GetAttribute("yoffset")),
                        xa = int.Parse(xmlReader.GetAttribute("xadvance")),
                    };
                }
            }
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if(e.Button == SButton.Down)
            {
                ReloadFont();
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ReloadFont();

        }

        private void ReloadFont()
        {
            Monitor.Log("Reloading font");
            arabicTexture = Helper.ModContent.Load<Texture2D>("assets/Arabic_0.png");
            arabicMap = new Dictionary<string, Mapping>();
            XmlReader xmlReader = XmlReader.Create(Path.Combine(Helper.DirectoryPath, "assets", "Arabic.fnt"));
            Monitor.Log("reading xml font");
            while (xmlReader.Read())
            {
                //keep reading until we see your element
                if (xmlReader.Name.Equals("char") && (xmlReader.NodeType == XmlNodeType.Element))
                {
                    arabicMap[xmlReader.GetAttribute("id")] = new Mapping()
                    {
                        x = int.Parse(xmlReader.GetAttribute("x")),
                        y = int.Parse(xmlReader.GetAttribute("y")),
                        width = int.Parse(xmlReader.GetAttribute("width")),
                        height = int.Parse(xmlReader.GetAttribute("height")),
                        xo = int.Parse(xmlReader.GetAttribute("xoffset")),
                        yo = int.Parse(xmlReader.GetAttribute("yoffset")),
                        xa = int.Parse(xmlReader.GetAttribute("xadvance")),
                    };
                }
            }
            var fontObject = new SpriteFontMapping()
            {
                DefaultCharacter = '؟',
                LineSpacing = 24,
                Spacing = -1
            };
            var glyphs = new List<Rectangle>();
            var cropping = new List<Rectangle>();
            var charMap = new List<char>();
            var kerning = new List<Vector3>();
            foreach (var m in arabicMap)
            {
                var ch = (char)int.Parse(Convert.ToString(int.Parse(m.Key), 16), NumberStyles.HexNumber);
                var glyph = new Rectangle(m.Value.x, m.Value.y, m.Value.width, m.Value.height);
                var crop = new Rectangle(m.Value.xo, m.Value.yo, m.Value.width, m.Value.height);
                var kern = new Vector3(-m.Value.xa + m.Value.width, m.Value.width, 0);
                fontObject.Characters.Add(ch);
                fontObject.Glyphs.Add(ch, new SpriteFont.Glyph()
                {
                    BoundsInTexture = glyph,
                    Cropping = crop,
                    Character = ch,
                    LeftSideBearing = kern.X,
                    RightSideBearing = kern.Z,
                    Width = kern.Y,
                    WidthIncludingBearings = kern.X + kern.Y + kern.Z
                });
                glyphs.Add(glyph);
                cropping.Add(crop);
                charMap.Add(ch);
                kerning.Add(kern);
            }
            //File.WriteAllText(Path.Combine(Helper.DirectoryPath, "assets", "SpriteFont1.ar.json"), JsonConvert.SerializeObject(fontObject, Newtonsoft.Json.Formatting.Indented));
            arabicFont = new SpriteFont(arabicTexture, glyphs, cropping, charMap, 32, -1, kerning, '؟');
            //var spriteFont = SHelper.GameContent.Load<SpriteFont>("Fonts/SpriteFont1.ar-AR");
        }
    }
}