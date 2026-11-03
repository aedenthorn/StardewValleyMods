using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.Characters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Bork
{
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public const string dataPath = "aedenthorn.Bork/data";
        public const string synonymPath = "aedenthorn.Bork/synonyms";
        public const string responsePath = "aedenthorn.Bork/responses";
        public const string portraitPath = "aedenthorn.Bork/portrait";
        public const string spritePath = "aedenthorn.Bork/sprite";

        public static Dictionary<string, QueryData> DataDict
        {
            get
            {
                //SHelper.GameContent.InvalidateCache(dataPath);
                return SHelper.GameContent.Load<Dictionary<string, QueryData>>(dataPath); ;
            }
        }
        public static Dictionary<string, List<string>> SynonymDict
        {
            get
            {
                return SHelper.GameContent.Load<Dictionary<string, List<string>>>(synonymPath); ;
            }
        }
        public static Dictionary<string, Func<Dictionary<string, string>, List<string>>> ResponseDict
        {
            get
            {
                return SHelper.GameContent.Load<Dictionary<string, Func<Dictionary<string, string>, List<string>>>>(responsePath); ;
            }
        }
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }
        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(dataPath))
            {
                e.LoadFrom(() => {
                    var dict = SHelper.ModContent.DoesAssetExist<Dictionary<string, QueryData>>("assets/data.json") ? SHelper.ModContent.Load<Dictionary<string, QueryData>>("assets/data.json") : new Dictionary<string, QueryData>();
                    return dict;
                }, AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(synonymPath))
            {
                e.LoadFrom(() => {
                    var dict = SHelper.ModContent.DoesAssetExist<Dictionary<string, List<string>>>("assets/synonyms.json") ? SHelper.ModContent.Load<Dictionary<string, List<string>>>("assets/synonyms.json") : new Dictionary<string, List<string>>();
                    return dict;
                }, AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(responsePath))
            {
                e.LoadFrom(() => {
                    var dict = new Dictionary<string, Func<Dictionary<string, string>, List<string>>>()
                    {
                        { "ItemCountInLocation", ItemCountInLocation },
                        { "WhereNPC", WhereNPC },
                        { "WeatherSeasonDay", WeatherSeasonDay },
                        { "WeatherTomorrow", WeatherTomorrow },
                        { "NPCBirthday", NPCBirthday },
                        { "Joke", Joke },
                        { "Quote", Quote  },
                        { "AboutYou", AboutYou }

                    };
                    return dict;
                }, AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Portraits/Bork"))
            {
                e.LoadFromModFile<Texture2D>("assets/portrait.png", AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Characters/Bork"))
            {
                e.LoadFromModFile<Texture2D>("assets/sprite.png", AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Characters"))
            {
                e.Edit((asset) => {
                    var dict = asset.AsDictionary<string, CharacterData>().Data;
                    dict["Bork"] = new CharacterData()
                    {
                        DisplayName = SHelper.Translation.Get("bork"),
                        BirthSeason = Season.Winter,
                        BirthDay = 28,
                        HomeRegion = "Other",
                        CanSocialize = "false",
                        CanBeRomanced = false,
                        CanReceiveGifts = false,
                        CanCommentOnPurchasedShopItems = false,
                        CanGreetNearbyCharacters = false,
                        CanVisitIsland = "false",
                        Calendar = CalendarBehavior.HiddenUntilMet,
                        SocialTab = SocialTabBehavior.UnknownUntilMet,
                        SpawnIfMissing = false,
                        Breather = false,
                    };

                });
            }
        }


        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );
                var props = typeof(ModConfig).GetProperties().ToArray();
                var configMenuExt = Helper.ModRegistry.GetApi<IGMCMOptionsAPI>("jltaylor-us.GMCMOptions");

                foreach (var p in props)
                {
                    if (p.Name == "Debug")
                        continue;
                    if (p.PropertyType == typeof(bool))
                    {
                        configMenu.AddBoolOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (bool)p.GetValue(Config),
                            setValue: value => { p.SetValue(Config, value); }
                        );
                    }
                    else if (p.PropertyType == typeof(int))
                    {
                        configMenu.AddNumberOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (int)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(float))
                    {
                        configMenu.AddNumberOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (float)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(double))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => p.GetValue(Config).ToString(),
                            setValue: value => { if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) { p.SetValue(Config, d); } }
                        );
                    }
                    else if (p.PropertyType == typeof(string))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (string)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(KeybindList))
                    {
                        configMenu.AddKeybindList(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (KeybindList)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(SButton))
                    {
                        configMenu.AddKeybind(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (SButton)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(Color) && configMenuExt is not null)
                    {
                        configMenuExt.AddColorOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (Color)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                }
            }
        }

        public static string AddSpaces(string str)
        {
            if (str?.Length < 2)
                return str;
            string newStr = str[0].ToString();
            for (int i = 1; i < str.Length; i++)
            {
                char c = str[i];
                if (i < str.Length - 1)
                {
                    char c1 = str[i + 1];
                    char cm = str[i - 1];
                    if (c >= 'A' && c <= 'Z' && ((c1 >= 'a' && c1 <= 'z') || (cm >= 'a' && cm <= 'z')))
                    {
                        newStr += " ";
                    }
                }
                newStr += c;
            }
            return newStr;
        }
    }
}