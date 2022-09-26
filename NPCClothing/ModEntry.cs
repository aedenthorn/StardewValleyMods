using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

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
            else if (e.NameWithoutLocale.StartsWith("Characters/"))
            {
                e.Edit(CheckClothes, StardewModdingAPI.Events.AssetEditPriority.Late);
            }
            else if (e.NameWithoutLocale.StartsWith("Portraits/"))
            {
                e.Edit(CheckClothes, StardewModdingAPI.Events.AssetEditPriority.Late);
            }
        }

        private void CheckClothes(IAssetData obj)
        {
            if (!Config.ModEnabled)
                return;
            var split = obj.NameWithoutLocale.Name.Split('/');
            if (split.Length != 2)
                return;
            foreach (var kvp in clothingDict)
            {
                if (ClothesFit(split[1], kvp.Value, split[0] == "Characters"))
                {
                    Monitor.Log($"Applying clothes {kvp.Key} to {obj.NameWithoutLocale.Name}");
                    Color[] colors = ApplyClothes(split[1], obj.AsImage().Data, kvp.Value, split[0] == "Characters" ? kvp.Value.spriteTexturePath : kvp.Value.portraitTexturePath);
                    obj.AsImage().Data.SetData(colors);
                }
            }
        }
        
        private bool ClothesFit(string name, ClothingData data, bool sprite)
        {
            var npc = Game1.getCharacterFromName(name);
            if (npc is null || !npc.isVillager())
                return false;

            if (sprite && string.IsNullOrEmpty(data.spriteTexturePath))
                return false;
            if (!sprite && string.IsNullOrEmpty(data.portraitTexturePath))
                return false;
            if (data.nameRestrictions is not null && data.nameRestrictions.Count > 0 &&  !data.nameRestrictions.Contains(name))
                return false;
            if (data.genderRestrictions is not null && data.genderRestrictions.Count > 0 && !data.genderRestrictions.Contains(genderList[npc.Gender]))
                return false;
            if (data.ageRestrictions is not null && data.ageRestrictions.Count > 0 && !data.ageRestrictions.Contains(ageList[npc.Age]))
                return false;
            if (data.giftName is not null && data.giftName.Length > 0 && (!npc.modData.TryGetValue(giftKey, out string gifts) || !gifts.Split(',').Contains(data.giftName)))
                return false;
            if (data.percentChance <= Game1.random.Next(100))
                return false;
            return true;
        }
        private Color[] ApplyClothes(string name, Texture2D texture, ClothingData data, string texturePath)
        {
            Texture2D clothesTexture = Helper.GameContent.Load<Texture2D>(texturePath);
            Color[] clothesColors = new Color[clothesTexture.Width * clothesTexture.Height];
            Color[] charColors = new Color[texture.Width * texture.Height];
            clothesTexture.GetData(clothesColors);
            texture.GetData(charColors);
            skinDict.TryGetValue(name, out List<Color> skins);
            for (int i = 0; i < clothesColors.Length; i++)
            {
                if (clothesColors[i] != Color.Transparent)
                {
                    if(skins is not null && data.skinColors is not null)
                    {
                        if(data.skinColors.Contains(clothesColors[i]))
                        {
                            int idx = data.skinColors.IndexOf(clothesColors[i]);
                            if(skins.Count > idx)
                                charColors[i] = skins[idx];
                            continue;
                        }
                    }
                    if(clothesColors[i].A < 255)
                    {
                        if (clothesColors[i].A < 15) 
                            charColors[i] = Color.Transparent;
                        else
                            charColors[i] = Color.Lerp(charColors[i], clothesColors[i], clothesColors[i].A / 255f);
                    }
                    else 
                        charColors[i] = clothesColors[i];
                }
            }
            return charColors;
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
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
        }

    }
}