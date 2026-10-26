using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FoodBuffMultiplier
{
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

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
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit(delegate (IAssetData data)
                {
                    var dict = data.AsDictionary<string, ObjectData>();
                    foreach (var key in dict.Data.Keys.ToList())
                    {
                        if (dict.Data[key].Edibility > 0 && !Config.ExcludedFoods.Contains(dict.Data[key].Name))
                        {
                            if (dict.Data[key].Buffs != null)
                            {
                                foreach (var b in dict.Data[key].Buffs)
                                {
                                    if (!Config.ExcludedBuffs.Contains(b.BuffId))
                                    {
                                        b.Duration = (int)Math.Round(b.Duration * Config.DurationMult);
                                        if(b.CustomAttributes != null)
                                        {
                                            b.CustomAttributes.FarmingLevel = (int)Math.Round(b.CustomAttributes.FarmingLevel * Config.SkillMult);
                                            b.CustomAttributes.MiningLevel = (int)Math.Round(b.CustomAttributes.MiningLevel * Config.SkillMult);
                                            b.CustomAttributes.CombatLevel = (int)Math.Round(b.CustomAttributes.CombatLevel * Config.SkillMult);
                                            b.CustomAttributes.ForagingLevel = (int)Math.Round(b.CustomAttributes.ForagingLevel * Config.SkillMult);
                                            b.CustomAttributes.FishingLevel = (int)Math.Round(b.CustomAttributes.FishingLevel * Config.SkillMult);
                                            b.CustomAttributes.LuckLevel = (int)Math.Round(b.CustomAttributes.LuckLevel * Config.SkillMult);
                                            b.CustomAttributes.MaxStamina = (int)Math.Round(b.CustomAttributes.MaxStamina * Config.MaxStaminaMult);
                                            b.CustomAttributes.MagneticRadius = (int)Math.Round(b.CustomAttributes.MagneticRadius * Config.MagneticRadiusMult);
                                            b.CustomAttributes.Speed = (int)Math.Round(b.CustomAttributes.Speed * Config.SpeedMult);
                                            b.CustomAttributes.Defense = (int)Math.Round(b.CustomAttributes.Defense * Config.DefenseMult);
                                            b.CustomAttributes.Attack = (int)Math.Round(b.CustomAttributes.Attack * Config.AttackMult);
                                            b.CustomAttributes.Immunity = (int)Math.Round(b.CustomAttributes.Immunity * Config.ImmunityMult);
                                            b.CustomAttributes.KnockbackMultiplier = (int)Math.Round(b.CustomAttributes.KnockbackMultiplier * Config.KnockbackMultiplierMult);
                                            b.CustomAttributes.WeaponSpeedMultiplier = (int)Math.Round(b.CustomAttributes.WeaponSpeedMultiplier * Config.WeaponSpeedMultiplierMult);
                                            b.CustomAttributes.CriticalChanceMultiplier = (int)Math.Round(b.CustomAttributes.CriticalChanceMultiplier * Config.CriticalChanceMultiplierMult);
                                            b.CustomAttributes.CriticalPowerMultiplier = (int)Math.Round(b.CustomAttributes.CriticalPowerMultiplier * Config.CriticalPowerMultiplierMult);
                                            b.CustomAttributes.WeaponPrecisionMultiplier = (int)Math.Round(b.CustomAttributes.WeaponPrecisionMultiplier * Config.WeaponPrecisionMultiplierMult);
                                        }
                                    }
                                }
                            }
                        }

                    }
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