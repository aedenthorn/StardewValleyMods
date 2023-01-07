using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace HelpWanted
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static bool skipBillboardConst;
        public static bool gettingQuestDetails;
        public static string dictPath = "aedenthorn.HelpWanted/dictionary";
        public static string pinTexturePath = "aedenthorn.HelpWanted/pin";
        public static string padTexturePath = "aedenthorn.HelpWanted/pad";
        public static ModEntry context;
        public static List<IQuestData> questList = new();
        public static List<IQuestData> modQuestList = new();
        public static Random random;
        public static Texture2D pinTexture;
        public static Texture2D padTexture;
        public static Dictionary<string, JsonQuestData> modQuestDict = new Dictionary<string, JsonQuestData>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;

            random = new Random();

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, JsonQuestData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        public override object GetApi()
        {
            return new HelpWantedAPI();
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            if (!Config.ModEnabled || Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason))
                return;
            try
            {
                pinTexture = Game1.content.Load<Texture2D>(pinTexturePath);
            }
            catch
            {
                pinTexture = Helper.ModContent.Load<Texture2D>("assets/pin.png");
            }
            try
            {
                padTexture = Game1.content.Load<Texture2D>(padTexturePath);
            }
            catch
            {
                padTexture = Helper.ModContent.Load<Texture2D>("assets/pad.png");
            }
            var dict = Helper.GameContent.Load<Dictionary<string, JsonQuestData>>(dictPath);
            foreach(var kvp in dict)
            {
                var d = kvp.Value;
                try
                {
                    var q = new QuestData()
                    {
                        pinTextureSource = d.pinTextureSource,
                        padTextureSource = d.padTextureSource,
                        pinTexture = string.IsNullOrEmpty(d.pinTexturePath) ? pinTexture : Helper.GameContent.Load<Texture2D>(d.pinTexturePath),
                        padTexture = string.IsNullOrEmpty(d.padTexturePath) ? padTexture : Helper.GameContent.Load<Texture2D>(d.padTexturePath),
                        pinColor = d.pinColor is null ? GetRandomColor() : d.pinColor.Value,
                        padColor = d.padColor is null ? GetRandomColor() : d.padColor.Value,
                        icon = string.IsNullOrEmpty(d.iconPath) ? Game1.getCharacterFromName(d.quest.target).Portrait : Helper.GameContent.Load<Texture2D>(d.iconPath),
                        iconSource = d.iconSource,
                        iconColor = d.iconColor is null ? new Color(Config.PortraitTintR, Config.PortraitTintG, Config.PortraitTintB, Config.PortraitTintA) : d.iconColor.Value,
                        iconScale = d.iconScale,
                        iconOffset = d.iconOffset is null ? new Point(Config.PortraitOffsetX, Config.PortraitOffsetY) : d.iconOffset.Value,
                        quest = MakeQuest(d.quest)
                    };
                    modQuestList.Add(q);
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Error loading custom quest {kvp.Key} :\n\n {ex}", LogLevel.Warn);
                }

            }
            Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            questList.Clear();
            List<string> npcs = new List<string>();
            if (Game1.questOfTheDay is null)
            {
                RefreshQuestOfTheDay(random);
            }
            Rectangle iconRect = new Rectangle(0, 0, 64, 64);
            Point iconOffset = new Point(Config.PortraitOffsetX, Config.PortraitOffsetY);
            int tries = 0;
            for (int i = 0; i < Config.MaxQuests; i++)
            {
                if (modQuestList.Any())
                {
                    questList.Add(modQuestList[0]);
                    modQuestList.RemoveAt(0);
                    continue;
                }
                try
                {
                    AccessTools.FieldRefAccess<Quest, Random>(Game1.questOfTheDay, "random") = random;
                    gettingQuestDetails = true;
                    Game1.questOfTheDay.reloadDescription();
                    Game1.questOfTheDay.reloadObjective();
                    gettingQuestDetails = false;
                    NPC npc = null;
                    if (Game1.questOfTheDay is ItemDeliveryQuest)
                    {
                        npc = Game1.getCharacterFromName((Game1.questOfTheDay as ItemDeliveryQuest).target.Value);
                    }
                    else if (Game1.questOfTheDay is ResourceCollectionQuest)
                    {
                        npc = Game1.getCharacterFromName((Game1.questOfTheDay as ResourceCollectionQuest).target.Value);
                    }
                    else if (Game1.questOfTheDay is SlayMonsterQuest)
                    {
                        npc = Game1.getCharacterFromName((Game1.questOfTheDay as SlayMonsterQuest).target.Value);
                    }
                    else if (Game1.questOfTheDay is FishingQuest)
                    {
                        npc = Game1.getCharacterFromName((Game1.questOfTheDay as FishingQuest).target.Value);
                    }
                    if (npc is not null)
                    {
                        if ((Config.OneQuestPerVillager && npcs.Contains(npc.Name)) ||
                            (Config.AvoidMaxHearts && !Game1.IsMultiplayer && Game1.player.tryGetFriendshipLevelForNPC(npc.Name) >= Utility.GetMaximumHeartsForCharacter(npc) * 250)
                        )
                        {
                            tries++;
                            if(tries > 100)
                            {
                                tries = 0;
                            }
                            else
                            {
                                i--;
                            }
                            RefreshQuestOfTheDay(random);
                            continue;
                        }
                        tries = 0;
                        npcs.Add(npc.Name);
                        Texture2D icon = npc.Portrait;
                        questList.Add(new QuestData() { padTexture = padTexture, pinTexture = pinTexture, padTextureSource = new Rectangle(0, 0, 64, 64), pinTextureSource = new Rectangle(0, 0, 64, 64), icon = icon, iconSource = iconRect, quest = Game1.questOfTheDay, pinColor = GetRandomColor(), padColor = GetRandomColor(), iconColor = new Color(Config.PortraitTintR, Config.PortraitTintG, Config.PortraitTintB, Config.PortraitTintA), iconOffset = iconOffset, iconScale = Config.PortraitScale });
                    }
                }
                catch(Exception ex) 
                {
                    Monitor.Log($"Error loading quest:\n\n {ex}", LogLevel.Warn);
                }
                RefreshQuestOfTheDay(random);
            }
            modQuestList.Clear();
            Helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
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
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Must Like Item",
                getValue: () => Config.MustLikeItem,
                setValue: value => Config.MustLikeItem = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Must Love Item",
                getValue: () => Config.MustLoveItem,
                setValue: value => Config.MustLoveItem = value
            );
            
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Allow Artisan Goods",
                getValue: () => Config.AllowArtisanGoods,
                setValue: value => Config.AllowArtisanGoods = value
            );
            
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Ignore Game's Item Choice",
                getValue: () => Config.IgnoreVanillaItemSelection,
                setValue: value => Config.IgnoreVanillaItemSelection = value
            );
            
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "One Quest / Villager",
                getValue: () => Config.OneQuestPerVillager,
                setValue: value => Config.OneQuestPerVillager = value
            );
            
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Avoid Max Heart Villagers",
                getValue: () => Config.AvoidMaxHearts,
                setValue: value => Config.AvoidMaxHearts = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Max Item Price",
                getValue: () => Config.MaxPrice,
                setValue: value => Config.MaxPrice = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Days To Complete",
                getValue: () => Config.QuestDays,
                setValue: value => Config.QuestDays = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Max Quests",
                getValue: () => Config.MaxQuests,
                setValue: value => Config.MaxQuests = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Note Scale",
                getValue: () => Config.NoteScale + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.NoteScale = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Portrait Scale",
                getValue: () => Config.PortraitScale + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.PortraitScale = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "X Overlap Boundary",
                getValue: () => Config.XOverlapBoundary + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.XOverlapBoundary = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Y Overlap Boundary",
                getValue: () => Config.YOverlapBoundary+ "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.YOverlapBoundary = f; } }
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Portrait Offset X",
                getValue: () => Config.PortraitOffsetX,
                setValue: value => Config.PortraitOffsetX = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Portrait Offset Y",
                getValue: () => Config.PortraitOffsetY,
                setValue: value => Config.PortraitOffsetY = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Random Color Min",
                getValue: () => Config.RandomColorMin,
                setValue: value => Config.RandomColorMin = value,
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Random Color Max",
                getValue: () => Config.RandomColorMax,
                setValue: value => Config.RandomColorMax = value,
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Portrait Tint R",
                getValue: () => Config.PortraitTintR,
                setValue: value => Config.PortraitTintR = value,
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Portrait Tint G",
                getValue: () => Config.PortraitTintG,
                setValue: value => Config.PortraitTintG = value,
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Portrait Tint B",
                getValue: () => Config.PortraitTintB,
                setValue: value => Config.PortraitTintB = value,
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Portrait Tint A",
                getValue: () => Config.PortraitTintA,
                setValue: value => Config.PortraitTintA = value,
                min: 0,
                max: 255
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Resource Collect",
                getValue: () => Config.ResourceCollectionWeight+"",
                setValue: delegate(string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.ResourceCollectionWeight = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Fishing",
                getValue: () => Config.FishingWeight+"",
                setValue: delegate(string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.FishingWeight = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Slay Monster",
                getValue: () => Config.SlayMonstersWeight+"",
                setValue: delegate(string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.SlayMonstersWeight = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Item Delivery",
                getValue: () => Config.ItemDeliveryWeight+"",
                setValue: delegate(string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.ItemDeliveryWeight = f; } }
            );
        }
    }
}