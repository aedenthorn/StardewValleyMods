using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace ConversationBubbles
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static readonly string dictPath = "aedenthorn.ConversationBubbles/dictionary";
        public static Dictionary<string, ConversationData> npcConversationDataDict = new Dictionary<string, ConversationData>();
        public static List<ConversationInstance> currentConversations = new List<ConversationInstance>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
            helper.Events.Player.Warped += Player_Warped;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, ConversationData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            npcConversationDataDict = Helper.GameContent.Load<Dictionary<string, ConversationData>>(dictPath);
            Monitor.Log($"Loaded {npcConversationDataDict.Count} conversation datas");
            currentConversations.Clear();
        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            currentConversations.Clear();
        }

        private void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            if (!Config.EnableMod)
                return;

            foreach(var kvp in npcConversationDataDict)
            {
                if (e.NewTime >= kvp.Value.minTime && (kvp.Value.maxTime < 0 || e.NewTime < kvp.Value.maxTime) && (kvp.Value.locations is null || kvp.Value.locations.Contains(Game1.currentLocation.Name)) && Game1.random.NextDouble() < kvp.Value.chance)
                    MakeConversation(kvp.Key, kvp.Value);
            }
        }

        private void MakeConversation(string key, ConversationData data)
        {
            List<List<object>> candidates = new List<List<object>>();
            foreach(var p in data.participants)
            {
                List<object> thisCandidates = new List<object>();
                foreach (var t in p.participantTypes)
                {
                    switch (t)
                    {
                        case "NPC":
                            thisCandidates.AddRange(Game1.currentLocation.characters.ToList().Where(c => c.isVillager()));
                            break;
                        case "Monster":
                            thisCandidates.AddRange(Game1.currentLocation.characters.ToList().Where(c => c is Monster));
                            break;
                        case "FarmAnimal":
                            if (Game1.currentLocation is AnimalHouse)
                                thisCandidates.AddRange((Game1.currentLocation as AnimalHouse).animals.Values.ToList());
                            else if (Game1.currentLocation is Farm)
                                thisCandidates.AddRange((Game1.currentLocation as Farm).animals.Values.ToList());
                            else if (Game1.currentLocation is Forest)
                                thisCandidates.AddRange((Game1.currentLocation as Forest).marniesLivestock.ToList());
                            else continue;
                            break;
                        case "Pet":
                            thisCandidates.AddRange(Game1.currentLocation.characters.ToList().Where(c => c is Pet));
                            break;
                        case "Cat":
                            thisCandidates.AddRange(Game1.currentLocation.characters.ToList().Where(c => c is Cat));
                            break;
                        case "Dog":
                            thisCandidates.AddRange(Game1.currentLocation.characters.ToList().Where(c => c is Dog));
                            break;
                        case "Junimo":
                            thisCandidates.AddRange(Game1.currentLocation.characters.ToList().Where(c => c is Junimo));
                            break;
                        case "JunimoHarvester":
                            thisCandidates.AddRange(Game1.currentLocation.characters.ToList().Where(c => c is JunimoHarvester));
                            break;
                        case "Tree":
                            thisCandidates.AddRange(Game1.currentLocation.terrainFeatures.Values.Where(f => f is Tree));
                            break;
                        case "FruitTree":
                            thisCandidates.AddRange(Game1.currentLocation.terrainFeatures.Values.Where(f => f is FruitTree));
                            break;
                        default:
                            thisCandidates.AddRange(Game1.currentLocation.characters.ToList().Where(c => c.GetType()?.Name == t));
                            break;
                    }
                }
                if (p.participantNames != null)
                {
                    thisCandidates = thisCandidates.Where(c => AccessTools.Property(c.GetType(), "Name") is not null && p.participantNames.Contains(AccessTools.Property(c.GetType(), "Name").GetValue(c))).ToList();
                }
                if (thisCandidates.Count == 0)
                    return;
                ShuffleList(thisCandidates);
                candidates.Add(thisCandidates);
            }
            var instance = new ConversationInstance() { data = data };
            for (int i = 0; i < candidates.Count; i++)
            {
                for (int j = 0; j < candidates[i].Count; j++)
                {
                    if(i == 0)
                    {
                        foreach (var cc in currentConversations)
                        {
                            if (cc.participants.Contains(candidates[i][j]))
                                goto next2;
                        }
                        instance.participants.Add(candidates[i][j]);
                        goto next3;
                    }
                    for (int k = 0; k < i; k++)
                    {
                        if (Vector2.Distance(GetTile(instance.participants[k]), GetTile(candidates[i][j])) > data.tileDistance)
                        {
                            continue;
                        }
                        foreach(var cc in currentConversations)
                        {
                            if (cc.participants.Contains(candidates[i][j]))
                                goto next1;
                        }
                        instance.participants.Add(candidates[i][j]);
                        goto next3;
                    next1:
                        continue;
                    }
                next2:
                    continue;
                }
                return;
            next3:
                continue;
            }

            foreach(var d in data.dialogues)
            {
                instance.dialogues.Add(new DialogueInstance() { 
                    data = d, 
                    textAboveHeadPreTimer = Game1.random.Next(d.waitTimeMin, Math.Max(d.waitTimeMax, d.waitTimeMin)), 
                    textAboveHeadTimer = d.sayTime 
                });
            }
            Monitor.Log($"Starting conversation {key}");
            currentConversations.Add(instance);
        }

        private Vector2 GetTile(object thing)
        {
            if (thing.GetType().IsAssignableFrom(typeof(Character)))
                return (thing as Character).getTileLocation();
            if (thing.GetType().IsAssignableFrom(typeof(TerrainFeature)))
                return (thing as TerrainFeature).currentTileLocation;
            if (thing.GetType().IsAssignableFrom(typeof(Object)))
                return (thing as Object).TileLocation;
            return Vector2.Zero;
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

        }

    }
}