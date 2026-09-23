using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FarmerPortraits
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public const string reactionsPath = "aedenthorn.FarmerPortraits/reactions";
        public static Dictionary<string, Dictionary<string, int>> ReactionsDict  => SHelper.GameContent.Load<Dictionary<string, Dictionary<string, int>>>(reactionsPath);
        private static PerScreen<Dictionary<string, Texture2D>> portraitTextures = new(() => new Dictionary<string, Texture2D>());

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            helper.Events.Display.MenuChanged += Display_MenuChanged;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            helper.ConsoleCommands.Add("fppme", "Set the portrait for this farmer.", SetThisPortrait);
            helper.ConsoleCommands.Add("fppg", "Set the portrait for all farmers.", SetGlobalPortrait);
            helper.ConsoleCommands.Add("fpbme", "Set the background for this farmer.", SetThisBackground);
            helper.ConsoleCommands.Add("fpbg", "Set the background for all farmers.", SetGlobalBackground);
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(reactionsPath))
            {
                e.LoadFrom(() => new Dictionary<string, Dictionary<string, int>>(), AssetLoadPriority.Exclusive);
            }
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (Config.Debug && e.Button == SButton.OemCloseBrackets)
            {
                Dialogue d = new Dialogue(Game1.getCharacterFromName("Clint"), "Data\\ExtraDialogue:Clint_NoInventorySpace", "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur? Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla pariatur?");
                d.CurrentEmotion = "$0";
                Game1.DrawDialogue(d);
                return;

            }
            if (Config.Debug && e.Button == SButton.OemOpenBrackets)
            {
                Response[] responses = new Response[]
                {
                    new Response("Yes", Game1.content.LoadString("Strings\\Characters:Leo_Memory_Answer_Yes")),
                    new Response("No", Game1.content.LoadString("Strings\\Characters:Leo_Memory_Answer_No"))
                };
                string question = "Yes or no?";
                if (question == null)
                {
                    question = "";
                }
                Game1.currentLocation.createQuestionDialogue(question, responses, null, Game1.getCharacterFromName("Lewis"));
                return;
            }
        }

        private void Display_MenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!Config.EnableMod || Game1.activeClickableMenu is null)
                return;
            ReloadTextures();
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            //SMonitor.Log(string.Join(",", Game1.objectData.Where(kvp => kvp.Value.Type == "Arch").Select(kvp => kvp.Key)));
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
            for(int i = 1; i < str.Length; i++)
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