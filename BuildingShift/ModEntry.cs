using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Object = StardewValley.Object;

namespace BuildingShift
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public const string shiftKey = "aedenthorn.BuildingShift/shift";
        public const string lockedKey = "aedenthorn.BuildingShift/locked";


        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();


            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            foreach (var t in typeof(Game1).Assembly.GetTypes())
            {
                if(t == typeof(Building) || t.IsSubclassOf(typeof(Building)))
                {
                    foreach (var m in new MethodInfo[]{ t.GetMethod("draw"), t.GetMethod("drawShadow"), t.GetMethod("drawBackground"), t.GetMethod("DrawEntranceTiles") })
                    {
                        if (m != null && m.DeclaringType == t)
                        {
                            SMonitor.Log($"Transpiling {t.Name}.{m.Name}");

                            harmony.Patch(
                                m,
                                transpiler: new HarmonyMethod(typeof(ModEntry).GetMethod("Draw_Transpiler"))
                            );
                        }
                    }
                }
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsPlayerFree || Game1.currentLocation?.buildings  == null)
                return;
            if (e.Button == Config.ShiftDown)
            {
                Building b = GetHoveredBuilding();
                if (b != null)
                {
                    ShiftBuilding(b, 0, 1);
                }
            }
            else if (e.Button == Config.ShiftUp)
            {
                Building b = GetHoveredBuilding();
                if (b != null)
                {
                    ShiftBuilding(b, 0, -1);
                }
            }
            else if (e.Button == Config.ShiftRight)
            {
                Building b = GetHoveredBuilding();
                if (b != null)
                {
                    ShiftBuilding(b, 1, 0);
                }
            }
            else if (e.Button == Config.ShiftLeft)
            {
                Building b = GetHoveredBuilding();
                if (b != null)
                {
                    ShiftBuilding(b, -1, 0);
                }
            }
            else if (e.Button == Config.ResetKey)
            {
                Building b = GetHoveredBuilding();
                if (b != null)
                {
                    SMonitor.Log($"{b.buildingType.Value} shift reset");
                    b.modData[shiftKey] = "0,0";
                }
            }
            else if (e.Button == Config.LockKey)
            {
                Building b = GetHoveredBuilding();
                if (b != null)
                {
                    SMonitor.Log($"{b.buildingType.Value} shift lock toggle");
                    bool locked = false;
                    if (b.modData.ContainsKey(lockedKey))
                    {
                        b.modData.Remove(lockedKey);
                    }
                    else
                    {
                        locked = true;
                        b.modData[lockedKey] = "true";
                    }
                    Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get(locked ? "locked" : "unlocked"), 2));
                }
            }
        }


        private Building GetHoveredBuilding()
        {
            var x = Game1.viewport.X + Game1.getOldMouseX();
            var y = Game1.viewport.Y + Game1.getOldMouseY();
            using (List<Building>.Enumerator enumerator = Game1.currentLocation.buildings.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var building = enumerator.Current;
                    if(TryGetShift(building, out var amount))
                    {
                        x -= (int)amount.X;
                        y -= (int)amount.Y;
                    }
                    if(building.occupiesTile(new Vector2(x / 64, y / 64)))
                        return building;
                }
            }
            return null;
        }


        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => { Helper.WriteConfig(Config); }
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
