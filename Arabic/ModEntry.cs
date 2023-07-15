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
        public static SpriteFont dialogueFont;
        public static SpriteFont smallFont;
        public static SpriteFont tinyFont;
        //private Dictionary<string, Mapping> arabicMap;

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

            /*
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
            */
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if(e.Button == SButton.Down)
            {
                ReloadFonts();
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ReloadFonts();

        }

    }
}