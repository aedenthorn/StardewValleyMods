using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectTriggers
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetLoader
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        public static readonly string dictPath = "aedenthorn.ObjectTriggers/dictionary";
        private static Dictionary<string, ObjectTriggerData> objectTriggerDataDict = new Dictionary<string, ObjectTriggerData>();
        private static Dictionary<long, List<ObjectTriggerInstance>> farmerTrippingDict = new Dictionary<long, List<ObjectTriggerInstance>>();
        private static IParticleEffectAPI particleEffectAPI;

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
            helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            objectTriggerDataDict = Helper.Content.Load<Dictionary<string, ObjectTriggerData>>(dictPath, ContentSource.GameContent);
        }

        private void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            foreach(var kvp in objectTriggerDataDict)
            {
                switch (kvp.Value.tripperType)
                {
                    case "farmer":
                        foreach(var farmer in Game1.getAllFarmers())
                        {
                            CheckTrigger(farmer, kvp.Key, kvp.Value);
                        }
                        break;

                }
            }
            var keyArray = farmerTrippingDict.Keys.ToArray();
            for (int i = 0; i < keyArray.Length; i++)
            {
                var key = keyArray[i];
                Farmer farmer = Game1.getFarmer(key);
                if (farmer == null)
                    continue;
                for (int j = 0; j < farmerTrippingDict[key].Count; i++)
                {
                    var oti = farmerTrippingDict[key][j];
                    var otd = objectTriggerDataDict[oti.TriggerKey];
                    if (otd.interval <= 0)
                        continue;
                    oti.elapsed++;
                    if (oti.elapsed > otd.interval) 
                    {
                        farmerTrippingDict[key][j].elapsed = 0;
                        switch (otd.triggerEffectType)
                        {
                            case "damage":
                                farmer.takeDamage((int)objectTriggerDataDict[oti.TriggerKey].effectAmount, true, null);
                                break;
                            case "heal":
                                farmer.health = Math.Min(farmer.maxHealth, farmer.health + (int)objectTriggerDataDict[oti.TriggerKey].effectAmount);
                                break;
                            case "stamina":
                                farmer.Stamina = Math.Max(0, Math.Min(farmer.MaxStamina, farmer.Stamina + (int)objectTriggerDataDict[oti.TriggerKey].effectAmount));
                                break;
                        }
                    }
                }
            }
        }


        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {

            particleEffectAPI = Helper.ModRegistry.GetApi<IParticleEffectAPI>("aedenthorn.ParticleEffects");

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
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (!Config.EnableMod)
                return false;

            return asset.AssetNameEquals(dictPath);
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            Monitor.Log("Loading dictionary");

            return (T)(object)new Dictionary<string, ObjectTriggerData>();
        }
    }
}