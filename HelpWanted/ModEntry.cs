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
        public static string pinTexturePath = "aedenthorn.HelpWanted/pin";
        public static string padTexturePath = "aedenthorn.HelpWanted/pad";
        public static ModEntry context;
        public static List<IQuestData> questList = new();
        public static List<IQuestData> modQuestList = new();
        public static Random random;
        public static Texture2D pinTexture;
        public static Texture2D padTexture;

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


            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }
        public override object GetApi()
        {
            return new HelpWantedAPI();
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            if (!Config.ModEnabled)
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
            Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
            questList.Clear();
            if (Game1.questOfTheDay is null)
            {
                RefreshQuestOfTheDay(random);
            }
            Rectangle iconRect = new Rectangle(0, 0, 64, 64);
            Point iconOffset = new Point(Config.PortraitOffsetX, Config.PortraitOffsetY);
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
                        Texture2D icon = npc.Portrait;
                        questList.Add(new QuestData() { padTexture = padTexture, pinTexture = pinTexture, padTextureSource = new Rectangle(0, 0, 64, 64), pinTextureSource = new Rectangle(0, 0, 64, 64), icon = icon, iconSource = iconRect, quest = Game1.questOfTheDay, pinColor = GetRandomColor(), padColor = GetRandomColor(), iconColor = new Color(Config.PortraitTintR, Config.PortraitTintG, Config.PortraitTintB, Config.PortraitTintA), iconOffset = iconOffset, iconScale = Config.PortraitScale });
                        RefreshQuestOfTheDay(random);
                    }
                }
                catch { }
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