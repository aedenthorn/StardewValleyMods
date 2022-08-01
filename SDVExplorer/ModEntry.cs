using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using SDVExplorer.UI;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SDVExplorer
{
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static ExplorerMenu explorerMenu;

        public static List<object> currentHeirarchy = new List<object>();

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.ModEnabled)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (Config.MenuKeys.JustPressed())
            {
                Monitor.Log($"Opening menu");
                Game1.activeClickableMenu = new ExplorerMenu(Game1.uiViewport.Width / 2 - (1200 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (900 + IClickableMenu.borderWidth * 2) / 2, 1200 + IClickableMenu.borderWidth * 2, 900 + IClickableMenu.borderWidth * 2);
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {

            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled?",
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
        }

        public static List<FieldElement> LoadFields(object obj, List<object> hier)
        {
            currentHeirarchy = new List<object>(hier);
            Dictionary<string, FieldElement> fields = new Dictionary<string, FieldElement>();
            var type = obj.GetType();
            while (true)
            {
                foreach (var field in AccessTools.GetDeclaredFields(type))
                {
                    if (field.Name == null)
                        continue;
                    AddField(type, field, field.FieldType, field.Name, hier, fields);
                }
                foreach (var field in AccessTools.GetDeclaredProperties(type))
                {
                    if (field.Name == null)
                        continue;
                    AddField(type, field, field.PropertyType, field.Name, hier, fields);
                }
                if (type.BaseType == null || type.BaseType == typeof(object))
                    break;
                type = type.BaseType;
            }

            var list = fields.Values.ToList();
            list.Sort(delegate(FieldElement a, FieldElement b) { return a.label.CompareTo(b.label); });
            return list;
        }

        private static void AddField(Type type, object field, Type fieldType, string name, List<object> hier, Dictionary<string, FieldElement> fields)
        {
            var h = new List<object>(hier);
            h.Add(field);
            if (fields.ContainsKey(name))
                return;
            if (fieldType == typeof(bool))
            {
                fields[name] = new FieldCheckbox(name, AccessTools.Field(type, name), h);
            }
            else
            {
                fields[name] = new FieldElement(name, AccessTools.Field(type, name), h);
            }
        }
    }
}