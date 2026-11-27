using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VoicedDialogue
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        
        public static Harmony harmony;
        public static SoundEffect voiceSound;
        public static string dictPath= "aedenthorn.VoicedDialogue/dict";
        public static PerScreen<string> currentDialogue = new();
        private static SoundEffect soundEffect;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.EnableMod)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Constructor(typeof(DialogueBox), new Type[] { typeof(Dialogue) }),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.DialogueBox_Ctor_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(DialogueBox), nameof(DialogueBox.receiveLeftClick)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.DialogueBox_receiveLeftClick_Postfix))
            );
            /*
            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.showTextAboveHead)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.NPC_showTextAboveHead_Prefix))
            );
            */
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.EnableMod) { return; }
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, VoiceData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {

            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu != null)
            {

                // register mod
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Mod Enabled",
                    getValue: () => Config.EnableMod,
                    setValue: value => Config.EnableMod = value
                );
            }
        }

        public static void DialogueBox_Ctor_Postfix(Dialogue dialogue)
        {
            if (!Config.EnableMod)
                return;

            PlayDialogue(dialogue);
        }
        public static void DialogueBox_receiveLeftClick_Postfix(DialogueBox __instance)
        {
            if(!__instance.transitioning && __instance.characterDialogue != null)
                PlayDialogue(__instance.characterDialogue);
        }
        public static void NPC_showTextAboveHead_Prefix(NPC __instance, string Text)
        {
            if (!Config.EnableMod)
                return;
            PlayDialogue(__instance.Name, Text, null);
        }

        public static void PlayDialogue(Dialogue dialogue)
        {
            if (!Config.EnableMod || dialogue.speaker == null || dialogue.dialogues[dialogue.currentDialogueIndex] == currentDialogue.Value)
                return;
            PlayDialogue(dialogue.speaker.Name, dialogue.dialogues[dialogue.currentDialogueIndex], dialogue.TranslationKey + "_" + dialogue.currentDialogueIndex);
        }
        public static void PlayDialogue(string name, string dialogue, string key) {

            if (!Config.EnableMod)
                return;
            SMonitor.Log($"Checking dialogue for {name}:\n\n{dialogue}");
            var voiceDict = SHelper.GameContent.Load<Dictionary<string, VoiceData>>(dictPath) ?? new Dictionary<string, VoiceData>();
            foreach(var data in voiceDict.Values)
            {
                if(data.speaker == name && (key == data.dialogueKey || data.dialogueString == dialogue))
                {
                    currentDialogue.Value = dialogue;
                    SMonitor.Log($"Dialogue {(key == data.dialogueKey ? "key" : "string")} found, playing voice line");
                    try
                    {
                        voiceSound?.Dispose();
                        string dirPath;
                        string filePath;
                        if (data.filePath.StartsWith("SMAPI/"))
                        {
                            var parts = data.filePath.Split('/', 3);
                            IModInfo info = SHelper.ModRegistry.Get(parts[1]);
                            if (info is not null)
                            {
                                dirPath = (string)AccessTools.Property(info.GetType(), "DirectoryPath").GetValue(info);
                                filePath = parts[2];
                            }
                            else
                            {
                                SMonitor.Log($"Dialogue file {data.filePath} not found", LogLevel.Warn);
                                return;
                            }
                        }
                        else
                        {
                            dirPath = SHelper.DirectoryPath;
                            filePath = data.filePath;
                        }
                        string filePathCombined = Path.Combine(dirPath, filePath);

                        if (File.Exists(filePathCombined))
                        {
                            using (var stream = new FileStream(filePathCombined, FileMode.Open))
                            {
                                voiceSound = SoundEffect.FromStream(stream);
                                voiceSound.Play();
                            }
                        }
                        else
                        {
                            SMonitor.Log($"Dialogue file {data.filePath} not found", LogLevel.Warn);
                        }
                    }
                    catch(Exception e)
                    {
                        SMonitor.Log($"Error playing dialogue:\n\n{e}", LogLevel.Error);
                    }
                    return;
                }
            }
        }


        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.Debug)
                return;
            if (e.Button == SButton.Insert)
            {
                var person = Game1.getCharacterFromName("Emily");
                Game1.warpCharacter(person, Game1.player.currentLocation, Game1.player.getTileLocation() + new Microsoft.Xna.Framework.Vector2(0, -2));
                person.faceDirection(2);
                person.CurrentDialogue.Clear();

                person.setNewDialogue("I am only resolved to act in a manner which will constitute my own happiness without reference to you or to any person so wholly unconnected with me.", true, false);
                var person2 = Game1.getCharacterFromName("Elliott");
                Game1.warpCharacter(person2, Game1.player.currentLocation, Game1.player.getTileLocation() + new Microsoft.Xna.Framework.Vector2(1, -2));
                person2.faceDirection(2);
                person2.CurrentDialogue.Clear();

                person2.setNewDialogue("My feelings will not be repressed. You must allow me to tell you how ardently I admire and love you.", true, false);
                return;

                string relativeTitle = "father";
                string itemName = "French Toast";

                string nameAndTitle = Game1.LoadStringByGender(0, "Strings\\StringsFromCSFiles:NPC.cs.4079", new object[]
                {
                    relativeTitle
                });
                string message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4083", nameAndTitle);
                message += (true ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4135", itemName, Lexicon.getRandomNegativeFoodAdjective(null)) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4138", itemName, Lexicon.getRandomNegativeFoodAdjective(null)));
                try
                {
                    message = message.Substring(0, 1).ToUpper() + message.Substring(1, message.Length - 1);
                }
                catch (Exception)
                {
                }
                //Game1.drawDialogue(person, message);
                person.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1738", person.displayName), true, false);

                return;
            }
            if (e.Button == SButton.F3)
            {
                var person = Game1.getCharacterFromName("Marnie");
                person.sayHiTo(Game1.getCharacterFromName("Lewis"));
                return;
            }
        }
    }
}