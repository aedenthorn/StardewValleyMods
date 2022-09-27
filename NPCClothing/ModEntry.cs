using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;

namespace NPCClothing
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static string dictPath = "aedenthorn.NPCClothing/dictionary";
        public static string skinPath = "aedenthorn.NPCClothing/skin";
        public static string giftKey = "aedenthorn.NPCClothing/gift";
        
        public static ClothingData forceWear;

        public static int[] giftIndexes = new int[] {
            -42424200,
            -42424202,
            -42424204,
            -42424206,
            -42424208
        };
        
        public static Dictionary<string, ClothingData> clothingDict = new Dictionary<string, ClothingData>();
        public static Dictionary<string, List<Color>> skinDict = new Dictionary<string, List<Color>>();
        public static Dictionary<string, NPC> npcDict = new Dictionary<string, NPC>();
        
        public static List<string> genderList = new List<string>()
        {
            "male", "female", "undefined"
        };
        public static List<string> ageList = new List<string>()
        {
            "adult", "teen", "child"
        };


        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.ModEnabled)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, ClothingData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(skinPath))
            {
                e.LoadFrom(() => new Dictionary<string, List<Color>>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.StartsWith("Characters/") && e.NameWithoutLocale.Name.Split('/').Length == 2)
            {
                e.Edit(CheckClothes, StardewModdingAPI.Events.AssetEditPriority.Late);
            }
            else if (e.NameWithoutLocale.StartsWith("Portraits/"))
            {
                e.Edit(CheckClothes, StardewModdingAPI.Events.AssetEditPriority.Late);
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            clothingDict = Game1.content.Load<Dictionary<string, ClothingData>>(dictPath);
            Monitor.Log($"Loaded {clothingDict.Count} clothes items");
            skinDict = new Dictionary<string, List<Color>>()
            {
                {"Emily", new List<Color>()
                    {
                        new Color(122, 0, 74),
                        new Color(242, 102, 81),
                        new Color(122, 0, 74)
                    } 
                }
            };
            Helper.GameContent.InvalidateCache(asset => asset.DataType == typeof(Texture2D) && ((asset.Name.StartsWith("Characters/") && asset.Name.Name.Split('/').Length == 2) || asset.Name.StartsWith("Portraits/")));
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            //MakeHatData();

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
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Wear when gifted",
                getValue: () => Config.ForceWearOnGift,
                setValue: value => Config.ForceWearOnGift = value
            );
        }
    }
}