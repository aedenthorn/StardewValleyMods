using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile;

namespace SewerSlimes
{
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static string[] BaseSlimes = new string[]
        {
                "Green Slime",
                "Frost Jelly",
                "Red Sludge",
                "Purple Sludge"
        };

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;
            Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;


            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_DayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
        {
            var sewer = Game1.getLocationFromName("Sewer");
            if (sewer is null)
                return;
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            var sewer = Game1.getLocationFromName("Sewer");
            if (!Config.ModEnabled || sewer is null)
                return;
            int existing = 0;
            if(sewer.characters.Count > 0)
            {
                for (int i = sewer.characters.Count - 1; i >= 0; i--)
                {
                    if (sewer.characters[i] is Monster && (sewer.characters[i] is BigSlime || sewer.characters[i] is GreenSlime))
                    {
                        existing++;
                    }
                }
            }
            if(Config.MaxSlimesPerDay < Config.MinSlimesPerDay)
            {
                Config.MaxSlimesPerDay = Config.MinSlimesPerDay;
                Helper.WriteConfig(Config);
            }
            int amount = Math.Min(Config.MaxTotalSlimes - existing, Game1.random.Next(Config.MinSlimesPerDay, Config.MaxSlimesPerDay + 1));
            if (amount <= 0)
                return;
            List<Vector2> used = new();
            for (int i = 0; i < amount; i++)
            {
                Rectangle rect = Config.SpawnAreas[Game1.random.Next(Config.SpawnAreas.Count)];
                Vector2 pos = new Vector2(rect.X + Game1.random.Next(rect.Width), rect.Y + Game1.random.Next(rect.Height)) * 64;
                if (used.Contains(pos))
                    continue;
                Monster m;
                if (Game1.random.NextDouble() < Config.BigChancePercent / 100f)
                {
                    string name = GetSlimeName(Config.BigSlimeWeights);
                    switch (name)
                    {
                        case "Blue":
                            m = new BigSlime(pos, 40);
                            break;
                        case "Red":
                            m = new BigSlime(pos, 80);
                            break;
                        case "Purple":
                            m = new BigSlime(pos, 121);
                            break;
                        default:
                            m = new BigSlime(pos, 0);
                            break;
                    }
                }
                else
                {
                    m = MakeSlime(pos, Config.SlimeWeights);
                }
                var n = m.Name;
                sewer.characters.Add(m);
            }
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if(Config.ModEnabled && e.DataType == typeof(Map) && e.NameWithoutLocale.IsEquivalentTo("Maps/Sewer"))
            {
                e.Edit(delegate (IAssetData assetData)
                {
                    var map = assetData.Data as Map;
                    foreach (var r in Config.ForbidAreas)
                    {
                        for (int x = r.X; x < r.X + r.Width; x++)
                        {
                            for (int y = r.Y; y < r.Y + r.Height; y++)
                            {
                                try
                                {
                                    map.GetLayer("Back").Tiles[x, y].Properties["NPCBarrier"] = "t";
                                }
                                catch {
                                    var fx = 1;
                                }
                            }
                        }
                    }
                });
            }
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
                    name: () => SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );
                
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_MinSlimesPerDay_Name"),
                    getValue: () => Config.MinSlimesPerDay,
                    setValue: value => Config.MinSlimesPerDay = value
                );
                
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_MaxSlimesPerDay_Name"),
                    getValue: () => Config.MaxSlimesPerDay,
                    setValue: value => Config.MaxSlimesPerDay = value
                );
                
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_MaxSlimes_Name"),
                    getValue: () => Config.MaxTotalSlimes,
                    setValue: value => Config.MaxTotalSlimes = value
                );
                
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_SpecialChancePercent_Name"),
                    getValue: () => Config.SpecialChancePercent,
                    setValue: value => Config.SpecialChancePercent = value,
                    min: 0,
                    max: 100
                );

                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_BigChancePercent_Name"),
                    getValue: () => Config.BigChancePercent,
                    setValue: value => Config.BigChancePercent = value,
                    min: 0,
                    max: 100
                );

                configMenu.AddSectionTitle(
                    mod: ModManifest,
                    text: () => SHelper.Translation.Get("GMCM_Option_SlimeWeights_Text")
                );
                foreach(var w in Config.SlimeWeights.Keys.ToArray())
                {
                    configMenu.AddNumberOption(
                        mod: ModManifest,
                        name: () => SHelper.Translation.Get($"GMCM_Option_{w}_Name"),
                        getValue: () => Config.SlimeWeights[w],
                        setValue: value => Config.SlimeWeights[w] = value
                    );
                }
                configMenu.AddSectionTitle(
                    mod: ModManifest,
                    text: () => SHelper.Translation.Get("GMCM_Option_BigSlimeWeights_Text")
                );
                foreach(var w in Config.BigSlimeWeights.Keys.ToArray())
                {
                    configMenu.AddNumberOption(
                        mod: ModManifest,
                        name: () => SHelper.Translation.Get($"GMCM_Option_Big_{w}_Name"),
                        getValue: () => Config.BigSlimeWeights[w],
                        setValue: value => Config.BigSlimeWeights[w] = value
                    );
                }
            }
        }
    }
}