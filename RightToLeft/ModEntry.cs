using BmFont;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData;
using StardewValley.Menus;
using System.Collections.Generic;
using System.IO;

namespace RightToLeft
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        //public static string dictPath = "aedenthorn.RightToLeft/dictionary";
        private static Dictionary<string, LanguageInfo> languageDict = new();
        private LetterViewerMenu letterView;

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
            Helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;
            foreach (var f in Directory.GetDirectories(Helper.DirectoryPath))
            {
                try
                {
                    LanguageInfo li = JsonConvert.DeserializeObject<LanguageInfo>(File.ReadAllText(Path.Combine(f, "content.json")));
                    li.path = f;
                    languageDict[li.code] = li;
                }
                catch { }
            }
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

        private void Display_RenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            letterView = null;
            if (letterView != null)
            {
                letterView.draw(e.SpriteBatch);
            }
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/AdditionalLanguages"))
            {
                e.Edit(delegate (IAssetData data) { 
                    foreach(var kvp in languageDict) 
                    {
                        data.GetData<List<ModLanguage>>().Add(kvp.Value.metaData);
                    }
                });
            }
            foreach (var kvp in languageDict)
            {
                if (e.Name.IsEquivalentTo($"aedenthorn.RightToLeft/{kvp.Value.code}/Button"))
                {
                    e.LoadFromModFile<Texture2D>(Path.Combine(kvp.Value.code, "button.png"), AssetLoadPriority.Exclusive);
                    return;
                }
                if(e.Name.IsEquivalentTo("Fonts/SpriteFont1_international") && LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod && LocalizedContentManager.CurrentModLanguage.LanguageCode == kvp.Value.code)
                {
                    var x = e.Name;
                    e.LoadFromModFile<XmlSource>($"{kvp.Value.code}/dialogueFont.fnt", AssetLoadPriority.Exclusive);
                    return;
                }
                else if (e.Name.IsEquivalentTo($"Fonts/{kvp.Value.name}_0"))
                {
                    e.LoadFromModFile<Texture2D>(Path.Combine(kvp.Value.code, "dialogueFont_0.png"), AssetLoadPriority.Exclusive);
                    return;
                }
                else if (e.Name.IsEquivalentTo($"Fonts/SpriteFont1.{kvp.Value.code}"))
                {
                    ReloadFonts();
                    e.LoadFrom(() => kvp.Value.dialogueFont, AssetLoadPriority.Exclusive);
                    return;
                }
                else if (e.Name.IsEquivalentTo($"Fonts/SmallFont.{kvp.Value.code}"))
                {
                    ReloadFonts();
                    e.LoadFrom(() => kvp.Value.smallFont, AssetLoadPriority.Exclusive);
                    return;
                }
                else if (e.Name.IsEquivalentTo($"Fonts/tinyFont.{kvp.Value.code}"))
                {
                    ReloadFonts();
                    e.LoadFrom(() => kvp.Value.tinyFont, AssetLoadPriority.Exclusive);
                    return;
                }
            }
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if(e.Button == SButton.Down)
            {
                ReloadFonts();
                foreach(var key in languageDict.Keys)
                {
                    Helper.GameContent.InvalidateCache($"Fonts/SpriteFont1.{key}");
                    Helper.GameContent.InvalidateCache($"Fonts/SmallFont.{key}");
                    Helper.GameContent.InvalidateCache($"Fonts/tinyFont.{key}");
                }
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            

            ReloadFonts();
        }

    }
}