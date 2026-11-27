using HarmonyLib;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace TextReplace
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public const string dictPath = "aedenthorn.TextReplace/dict";
        public static HashSet<string> assetNames = new();
        public static bool launched;
        public static List<string> tooLate = new();
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
            //helper.ConsoleCommands.Add("textreplace", "Create a content.json file to use with Text Replace.\n\nUsage: textreplace <modID> <find> <replace>\n- modId: a unique name for your mod's key in the dictionary.\n- find: the string to search for.\n- replace: the replacement string.", CreateReplacement);
            //File.WriteAllText(Path.Combine(helper.DirectoryPath, "content.json"), JsonConvert.SerializeObject(instanceDict));
        }

        private void CreateReplacement(string command, string[] args)
        {
            if (args.Length != 3) 
            {
                SMonitor.Log("Syntax error.\n\nUsage: textreplace <modID> <find> <replace>");
                return;
            }

            var dict = new ReplaceDict();
            foreach(var n in assetNames)
            {
                if (!Helper.GameContent.DoesAssetExist<Dictionary<string, string>>(Helper.GameContent.ParseAssetName(n)))
                    continue;
                var d = Helper.GameContent.Load<Dictionary<string, string>>(n).Where(kvp => kvp.Value.Contains(args[1]));
                if (d.Any())
                { 
                    dict.Replacements[n] = new();
                    foreach (var kvp in d)
                    {
                        dict.Replacements[n][kvp.Key] = new Dictionary<string, string>()
                        {
                            { args[1], args[2] }
                        };
                    }
                }
            }
            var data = new ContentPatcherData()
            {
                Format = SHelper.ModRegistry.Get("PathosChild.ContentPatcher")?.Manifest.Version.ToString() ?? "2.9.0",
            };
            data.Changes.Add(new ChangeData()
            {
                Entries = new Dictionary<string, ReplaceDict>()
                {
                    { args[0], dict }
                }
            });
            Directory.CreateDirectory(Path.Combine(Helper.DirectoryPath, "output"));
            File.WriteAllText(Path.Combine(Helper.DirectoryPath, "output", "content.json"), JsonConvert.SerializeObject(data, Formatting.Indented));
            SMonitor.Log($"Wrote {dict.Replacements.Count} replacements to file output/content.json");
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, ReplaceDict>(), AssetLoadPriority.Exclusive);
            }
            else if (e.DataType == typeof(Dictionary<string, string>))
            {
                //assetNames.Add(e.NameWithoutLocale.ToString());
                if (!launched)
                {
                    tooLate.Add(e.NameWithoutLocale.ToString());
                    return;
                }
                var instanceDict = SHelper.GameContent.Load<Dictionary<string, ReplaceDict>>(dictPath);
                foreach (var dict in instanceDict.Values)
                {
                    foreach (var kvp in dict.Replacements)
                    {
                        if (e.NameWithoutLocale.IsEquivalentTo(kvp.Key))
                        {
                            e.Edit((IAssetData data) =>
                            {
                                var dict = data.AsDictionary<string, string>().Data;
                                foreach (var kvp2 in kvp.Value)
                                {
                                    if (!dict.TryGetValue(kvp2.Key, out var str))
                                    {
                                        if(Config.Debug)
                                            Monitor.Log($"Missing key {kvp2.Key} in {kvp.Key}", LogLevel.Info);
                                    }
                                    else
                                    {
                                        foreach (var kvp3 in kvp2.Value)
                                        {
                                            var newStr = str.Replace(kvp3.Key, kvp3.Value);
                                            if(newStr == str)
                                            {
                                                if(Config.Debug)
                                                    Monitor.Log($"String {kvp3.Key} not found in entry {kvp2.Key} in file {kvp.Key}", LogLevel.Info);
                                            }
                                            else
                                            {
                                                str = newStr;
                                            }
                                        }
                                        dict[kvp2.Key] = str;
                                    }
                                }
                            }, AssetEditPriority.Late + 1000);
                        }
                    }
                }
            }
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            launched = true;
            Helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
            if (tooLate.Any())
            {
                Helper.GameContent.InvalidateCache(dictPath);
                var instanceDict = SHelper.GameContent.Load<Dictionary<string, ReplaceDict>>(dictPath);
                foreach (var str in tooLate)
                {
                    foreach (var dict in instanceDict.Values)
                    {
                        foreach (var kvp in dict.Replacements)
                        {
                            if (str == kvp.Key)
                            {
                                Helper.GameContent.InvalidateCache(str);
                            }
                        }
                    }
                }
            }
        }
        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;

            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("ModEnabled"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );
                var props = typeof(ModConfig).GetProperties().ToArray();
                var configMenuExt = Helper.ModRegistry.GetApi<IGMCMOptionsAPI>("jltaylor-us.GMCMOptions");

                foreach (var p in props)
                {
                    if (p.PropertyType == typeof(bool))
                    {
                        configMenu.AddBoolOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (bool)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
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
            string newStr = "";
            foreach (var c in str)
            {
                if (c >= 'A' && c <= 'Z' && newStr.Length > 0)
                {
                    newStr += " ";
                }
                newStr += c;
            }
            return newStr;
        }
    }
}