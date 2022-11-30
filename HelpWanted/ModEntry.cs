using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using xTile;
using xTile.Dimensions;
using xTile.Tiles;
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
            var api = Helper.ModRegistry.GetApi<IHelpWantedAPI>("aedenthorn.HelpWanted");
            if (api != null)
            {
                var d = new MyQuestData()
                {
                    pinTextureSource = new Rectangle(0,0,64,64),
                    padTextureSource = new Rectangle(0,0,64,64),
                    pinTexture = pinTexture,
                    padTexture = padTexture,
                    pinColor = Color.Black,
                    padColor = Color.Gray,
                    icon = Game1.getCharacterFromName("Krobus").Portrait,
                    iconSource = new Rectangle(0, 0, 64, 64),
                    quest = new ItemDeliveryQuest()
                };
                (d.quest as ItemDeliveryQuest).target.Value = "Krobus";
                (d.quest as ItemDeliveryQuest).item.Value = 305;
                (d.quest as ItemDeliveryQuest).targetMessage = "I can haz void egg? Tanks lots.";
                d.quest.currentObjective = "Gib Krobus void egg.";
                d.quest.questDescription = "Pls gib.";

                api.AddQuestToday(d);
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
            for (int i = 0; i < Config.MaxQuests; i++)
            {
                if (modQuestList.Any())
                {
                    questList.Add(modQuestList[0]);
                    modQuestList.RemoveAt(0);
                    continue;
                }
                var days = Game1.stats.DaysPlayed;
                Game1.stats.DaysPlayed = (uint)random.Next();
                AccessTools.FieldRefAccess<Quest, Random>(Game1.questOfTheDay, "random") = random;
                gettingQuestDetails = true;
                Game1.questOfTheDay.reloadDescription();
                Game1.questOfTheDay.reloadObjective();
                gettingQuestDetails = false;
                NPC npc = null;
                if (Game1.questOfTheDay is ItemDeliveryQuest)
                {
                    npc = Game1.getCharacterFromName((Game1.questOfTheDay as ItemDeliveryQuest).target.Value);
                    if ((Config.MustLikeItem || Config.MustLoveItem) && Game1.NPCGiftTastes.TryGetValue((Game1.questOfTheDay as ItemDeliveryQuest).target.Value, out string data))
                    {
                        var split = data.Split('/');
                    }
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
                Texture2D icon = npc.Portrait;
                Rectangle iconRect = new Rectangle(0, 0, 64, 64);
                questList.Add(new QuestData() { padTexture = padTexture, pinTexture = pinTexture, padTextureSource = new Rectangle(0, 0, 64, 64), pinTextureSource = new Rectangle(0, 0, 64, 64), icon = icon, iconSource = iconRect, quest = Game1.questOfTheDay, pinColor = GetRandomColor(), padColor = GetRandomColor() });
                RefreshQuestOfTheDay(random);
                Game1.stats.DaysPlayed = days;
            }
            Helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
        }

        private Color GetRandomColor()
        {
            return new Color((byte)random.Next(155, 256), (byte)random.Next(155, 256), (byte)random.Next(155, 256));
        }

        private void RefreshQuestOfTheDay(Random r)
        {
            var mine = (MineShaft.lowestLevelReached > 0 && Game1.stats.DaysPlayed > 5U);
            float totalWeight = Config.ResourceCollectionWeight + (mine ? Config.SlayMonstersWeight : 0) + Config.FishingWeight + Config.ItemDeliveryWeight;
            double d = r.NextDouble();
            float currentWeight = Config.ResourceCollectionWeight;
            if (d < currentWeight / totalWeight)
            {
                Game1.questOfTheDay = new ResourceCollectionQuest();
                return;
            }
            if (mine)
            {
                currentWeight += Config.SlayMonstersWeight;
                if (d < currentWeight / totalWeight)
                {
                    Game1.questOfTheDay = new SlayMonsterQuest();
                    return;
                }
            }
            currentWeight += Config.FishingWeight;
            if (d < currentWeight / totalWeight)
            {
                Game1.questOfTheDay = new FishingQuest();
                return;
            }
            Game1.questOfTheDay = new ItemDeliveryQuest();
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

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Icon Scale",
                getValue: () => Config.IconScale,
                setValue: value => Config.IconScale = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Max Quests",
                getValue: () => Config.MaxQuests,
                setValue: value => Config.MaxQuests = value
            );
        }
    }
}