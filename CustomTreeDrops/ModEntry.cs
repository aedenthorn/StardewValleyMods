using HarmonyLib;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Globalization;
using Object = StardewValley.Object;

namespace CustomTreeDrops
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        private static IDynamicGameAssetsApi apiDGA;
        private static IJsonAssetsApi apiJA;
        private Harmony harmony;

        private static Dictionary<int, DropData> dropDict = new Dictionary<int, DropData>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;


            harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Tree), "performTreeFall"),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Tree_performTreeFall_Transpiler))
            );

        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            apiDGA = Helper.ModRegistry.GetApi<IDynamicGameAssetsApi>("spacechase0.DynamicGameAssets");
            apiJA = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            

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
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Min Seeds",
                getValue: () => Config.MinSeeds,
                setValue: value => Config.MinSeeds = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Max Seeds",
                getValue: () => Config.MaxSeeds,
                setValue: value => Config.MaxSeeds = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Min Mixed Seeds",
                getValue: () => Config.MinMixedSeeds,
                setValue: value => Config.MinMixedSeeds = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Max Seeds",
                getValue: () => Config.SapToDrop,
                setValue: value => Config.SapToDrop = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Ancient Seed % Chance",
                getValue: () => "" + Config.AncientSeedChance,
                setValue: delegate (string value) { try { Config.AncientSeedChance = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Mixed Seed % Chance",
                getValue: () => "" + Config.MixedSeedChance,
                setValue: delegate (string value) { try { Config.MixedSeedChance = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
            );
        }

        public static void DropItems(Tree tree, int index, int x, int y, int number, GameLocation location)
        {
            if (!dropDict.ContainsKey(tree.treeType.Value))
            {
                int whatToDrop = (tree.treeType.Value == 7 && x % 7f == 0f) ? 422 : ((tree.treeType.Value == 7) ? 420 : ((tree.treeType.Value == 8) ? 709 : 92));
                if (Game1.IsMultiplayer)
                {
                    Game1.createMultipleObjectDebris(whatToDrop, x, y, 1, AccessTools.FieldRefAccess<Tree, NetLong>(tree, "lastPlayerToHit").Value, location);
                }
                else
                {
                    Game1.createMultipleObjectDebris(whatToDrop, x, y, 1, location);
                }
            }
            else
            {

            }
        }
        
        public static int GetSapToDropTwice()
        {
            return Config.SapToDrop * 2;
        }
        public static int GetSapToDrop()
        {
            return Config.SapToDrop;
        }


        private static Object GetObjectFromID(string id, int amount, int quality)
        {
            SMonitor.Log($"Trying to get object {id}, DGA {apiDGA != null}, JA {apiJA != null}");

            Object obj = null;
            try
            {

                if (int.TryParse(id, out int index))
                {
                    SMonitor.Log($"Spawning object with index {id}");
                    return new Object(index, amount, false, -1, quality);
                }
                else
                {
                    var dict = SHelper.Content.Load<Dictionary<int, string>>("Data/ObjectInformation", ContentSource.GameContent);
                    foreach (var kvp in dict)
                    {
                        if (kvp.Value.StartsWith(id + "/"))
                            return new Object(kvp.Key, amount, false, -1, quality);
                    }
                }
                if (apiDGA != null && id.Contains("/"))
                {
                    object o = apiDGA.SpawnDGAItem(id);
                    if (o is Object)
                    {
                        SMonitor.Log($"Spawning DGA object {id}");
                        (o as Object).Stack = amount;
                        (o as Object).Quality = quality;
                        return (o as Object);
                    }
                }
                if (apiJA != null)
                {
                    int idx = apiJA.GetObjectId(id);
                    if (idx != -1)
                    {
                        SMonitor.Log($"Spawning JA object {id}");
                        return new Object(idx, amount, false, -1, quality);

                    }
                }
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Exception: {ex}", LogLevel.Error);
            }
            SMonitor.Log($"Couldn't find item with id {id}");
            return obj;
        }

    }
}