using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DoorKnock
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public const string answerPointKey = "aedenthorn.DoorKnock/answer";
        public const string returnPointKey = "aedenthorn.DoorKnock/return";
        public const string animationKey = "aedenthorn.DoorKnock/animation";
        public const string openKey = "aedenthorn.DoorFurniture/open";
        public static Dictionary<string, int> delayDict = new Dictionary<string, int>();
        public static Dictionary<string, int> returnDict = new Dictionary<string, int>();
        public static PerScreen<Vector2?> farmerController = new PerScreen<Vector2?>();

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
            helper.Events.Multiplayer.ModMessageReceived += Multiplayer_ModMessageReceived;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Multiplayer_ModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if(!Config.ModEnabled || !Game1.IsMasterGame)
                return;
            if (e.FromModID == ModManifest.UniqueID)
            {
                MyMessage message = e.ReadAs<MyMessage>();
                switch (e.Type)
                {
                    case "Return":
                        returnDict[message.name] = message.delay;
                        return;
                    case "Delay":
                        delayDict[message.name] = message.delay;
                        return;
                    default:
                        return;
                }
            }
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            if(!Config.ModEnabled || !Game1.IsMasterGame)
                return;
            returnDict.Clear();
            delayDict.Clear();
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree || !Game1.IsMasterGame)
                return;
            foreach(var key in delayDict.Keys.ToArray())
            {
                delayDict[key]--;
                if (delayDict[key] < 0)
                {
                    delayDict.Remove(key);
                    var npc = Game1.getCharacterFromName(key, true);
                    if(npc != null)
                        DoneDelaying(npc);
                }
            }
            foreach(var key in returnDict.Keys.ToArray())
            {
                returnDict[key]--;
                if (returnDict[key] < 0)
                {
                    returnDict.Remove(key);
                    var npc = Game1.getCharacterFromName(key, true);
                    if(npc != null)
                        DoneWaiting(npc);
                }
            }
        }

        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree || !Config.KnockButton.JustPressed())
                return;
            if (Config.Debug)
            {
                //farmerController.Value = Game1.player.Position;
                //Game1.playSound("axchop");
            }
            var doorTile = Game1.player.GetGrabTile();
            Furniture f = Game1.currentLocation.GetFurnitureAt(doorTile);
            if (f?.modData.TryGetValue(openKey, out var open) == true && open == "closed")
            {
                PlayKnockSound(doorTile);
                return;
            }
            var action = Game1.currentLocation.GetTilePropertySplitBySpaces("Action", "Buildings", (int)doorTile.X, (int)doorTile.Y);
            if (action.Length > 1 && action[0] == "Door")
            {
                KnockInteriorDoor(doorTile, action);
                return;
            }
            var door = doorTile.ToPoint();
            if (Game1.currentLocation.doors.TryGetValue(door, out var target))
            {
                KnockExteriorDoor(doorTile, action);
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