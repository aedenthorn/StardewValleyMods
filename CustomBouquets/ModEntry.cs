using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;

namespace CustomBouquets
{
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public static string dictPath = "aedenthorn.CustomBouquets/dict";
        public static string flowerPath1 = "aedenthorn.CustomBouquets/flower1";
        public static string flowerPath2 = "aedenthorn.CustomBouquets/flower2";
        public static string flowerPath3 = "aedenthorn.CustomBouquets/flower3";
        public static string bouquetPath = "aedenthorn.CustomBouquets/bouquet";
        public static string recipeKey = "aedenthorn.CustomBouquets_Bouquet";
        public static string prismaticKey = "aedenthorn.PrismaticFlowers/prismatic";
        public static Texture2D flower1;
        public static Texture2D flower2;
        public static Texture2D flower3;
        public static Texture2D bouquet;
        public static Dictionary<string, string> dataDict;
        public static Dictionary<string, Texture2D> textureDict;

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;


            Helper.Events.Content.AssetRequested += Content_AssetRequested;
            Helper.Events.Content.AssetReady += Content_AssetReady;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Display.MenuChanged += Display_MenuChanged;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Display_MenuChanged(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            showingBouquets = false;
        }

        private void Content_AssetReady(object sender, StardewModdingAPI.Events.AssetReadyEventArgs e)
        {
            if (!Config.EnableMod)
                return;
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, string>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(flowerPath1))
            {
                e.LoadFromModFile<Texture2D>("assets/flower1.png", StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(flowerPath2))
            {
                e.LoadFromModFile<Texture2D>("assets/flower2.png", StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(flowerPath3))
            {
                e.LoadFromModFile<Texture2D>("assets/flower3.png", StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(bouquetPath))
            {
                e.LoadFromModFile<Texture2D>("assets/bouquet.png", StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
            {
                e.Edit(delegate (IAssetData data)
                {
                    var dict = data.AsDictionary<string, string>();
                    dict.Data[recipeKey] = $"771 1/Home/458/false/default/";
                });
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
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
                    name: () => SHelper.Translation.Get("Config.EnableMod"),
                    getValue: () => Config.EnableMod,
                    setValue: value => Config.EnableMod = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("Config.AllowMultSame"),
                    tooltip: () => SHelper.Translation.Get("Config.AllowMultSame.Desc"),
                    getValue: () => Config.AllowMultSame,
                    setValue: value => Config.AllowMultSame = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("Config.ForceColorFlowers"),
                    tooltip: () => SHelper.Translation.Get("Config.ForceColorFlowers.Desc"),
                    getValue: () => Config.ForceColorFlowers,
                    setValue: value => Config.ForceColorFlowers = value
                );

            }
        }
    }
}
