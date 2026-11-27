using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GMCMHelper
{
    public partial class ModEntry
    {
        public static bool AddMod(IManifest ModManifest, object Config, ITranslationHelper Translation, IModHelper Helper, IList<string> ExcludeList)
        {
            var configMenu = SHelper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return false;
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new(),
                save: () => Helper.WriteConfig(Config)
            );

            var props = Config.GetType().GetProperties().ToArray();
            var configMenuExt = Helper.ModRegistry.GetApi<IGMCMOptionsAPI>("jltaylor-us.GMCMOptions");

            foreach (var p in props)
            {
                if (ExcludeList.Contains(p.Name))
                    continue;
                if (p.PropertyType == typeof(bool))
                {
                    configMenu.AddBoolOption(
                        mod: ModManifest,
                        name: () => { var t = Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                        tooltip: () => { var t = Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                        getValue: () => (bool)p.GetValue(Config),
                        setValue: value => p.SetValue(Config, value)
                    );
                }
                else if (p.PropertyType == typeof(int))
                {
                    configMenu.AddNumberOption(
                        mod: ModManifest,
                        name: () => { var t = Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                        tooltip: () => { var t = Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                        getValue: () => (int)p.GetValue(Config),
                        setValue: value => p.SetValue(Config, value)
                    );
                }
                else if (p.PropertyType == typeof(float))
                {
                    configMenu.AddNumberOption(
                        mod: ModManifest,
                        name: () => { var t = Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                        tooltip: () => { var t = Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                        getValue: () => (float)p.GetValue(Config),
                        setValue: value => p.SetValue(Config, value)
                    );
                }
                else if (p.PropertyType == typeof(double))
                {
                    configMenu.AddTextOption(
                        mod: ModManifest,
                        name: () => { var t = Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                        tooltip: () => { var t = Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                        getValue: () => p.GetValue(Config).ToString(),
                        setValue: value => { if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) { p.SetValue(Config, d); } }
                    );
                }
                else if (p.PropertyType == typeof(string))
                {
                    configMenu.AddTextOption(
                        mod: ModManifest,
                        name: () => { var t = Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                        tooltip: () => { var t = Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                        getValue: () => (string)p.GetValue(Config),
                        setValue: value => p.SetValue(Config, value)
                    );
                }
                else if (p.PropertyType == typeof(KeybindList))
                {
                    configMenu.AddKeybindList(
                        mod: ModManifest,
                        name: () => { var t = Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                        tooltip: () => { var t = Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                        getValue: () => (KeybindList)p.GetValue(Config),
                        setValue: value => p.SetValue(Config, value)
                    );
                }
                else if (p.PropertyType == typeof(SButton))
                {
                    configMenu.AddKeybind(
                        mod: ModManifest,
                        name: () => { var t = Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                        tooltip: () => { var t = Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                        getValue: () => (SButton)p.GetValue(Config),
                        setValue: value => p.SetValue(Config, value)
                    );
                }
                else if (p.PropertyType == typeof(Color) && configMenuExt is not null)
                {
                    configMenuExt.AddColorOption(
                        mod: ModManifest,
                        name: () => { var t = Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                        tooltip: () => { var t = Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                        getValue: () => (Color)p.GetValue(Config),
                        setValue: value => p.SetValue(Config, value)
                    );
                }
            }
            return true;
        }

        public static string AddSpaces(string str)
        {
            string newStr = "";
            foreach(var c in str)
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