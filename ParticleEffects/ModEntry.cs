using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace ParticleEffects
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static readonly string dictPath = "Mods/aedenthorn.ParticleEffects/dict";
        public static Dictionary<string, ParticleEffectData> effectDict = new Dictionary<string, ParticleEffectData>();
        
        public static Dictionary<long, EntityParticleData> farmerEffectDict = new Dictionary<long, EntityParticleData>();
        public static Dictionary<string, EntityParticleData> npcEffectDict = new Dictionary<string, EntityParticleData>();
        public static Dictionary<string, EntityParticleData> locationEffectDict = new Dictionary<string, EntityParticleData>();
        public static EntityParticleData screenEffectDict = new EntityParticleData();
        public static Dictionary<string, EntityParticleData> objectEffectDict = new Dictionary<string, EntityParticleData>();
        
        public static Dictionary<Point, List<ParticleEffectData>> screenDict = new Dictionary<Point, List<ParticleEffectData>>();
        public static Dictionary<string, List<string>> NPCDict = new Dictionary<string, List<string>>();
        public static Dictionary<long, List<string>> farmerDict = new Dictionary<long, List<string>>();
        public static Dictionary<string, Dictionary<Point, List<ParticleEffectData>>> locationDict = new Dictionary<string, Dictionary<Point, List<ParticleEffectData>>>();

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
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
            helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            helper.Events.Display.RenderedHud += Display_RenderedHud;

            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.draw), new System.Type[] { typeof(SpriteBatch) }),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(Farmer_draw_postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.draw), new System.Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(Object_draw_postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.draw), new System.Type[] { typeof(SpriteBatch), typeof(float) }),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(NPC_draw_postfix))
            );
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if(Config.EnableMod && e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, ParticleEffectData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        private void Display_RenderedHud(object sender, StardewModdingAPI.Events.RenderedHudEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            foreach (var kvp in screenDict)
            {
                foreach (var effect in kvp.Value)
                {
                    ShowScreenParticleEffect(e.SpriteBatch, effect);
                }
            }
            foreach (var key in effectDict.Keys)
            {
                var ped = effectDict[key];
                switch (ped.type.ToLower())
                {
                    case "screen":
                        ShowScreenParticleEffect(e.SpriteBatch, ped);
                        break;
                }
            }
        }

        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (locationDict.TryGetValue(Game1.currentLocation.Name, out Dictionary<Point, List<ParticleEffectData>> dict))
            {
                foreach (var kvp in dict)
                {
                    foreach (var effect in kvp.Value)
                    {
                        ShowLocationParticleEffect(e.SpriteBatch, Game1.currentLocation, effect);
                    }
                }
            }
            foreach (var key in effectDict.Keys)
            {
                var ped = effectDict[key];
                switch (ped.type.ToLower())
                {
                    case "location":
                        if (Game1.currentLocation.Name == ped.name)
                            ShowLocationParticleEffect(e.SpriteBatch, Game1.currentLocation, ped);
                        break;
                }
            }
        }

        public override object GetApi()
        {
            return new ParticleEffectsAPI();
        }

        private void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            LoadEffects();
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            LoadEffects();
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            LoadEffects();
        }


        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {
                // register mod
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Mod Enabled",
                    getValue: () => Config.EnableMod,
                    setValue: value => Config.EnableMod = value
                );
            }

        }

    }
}