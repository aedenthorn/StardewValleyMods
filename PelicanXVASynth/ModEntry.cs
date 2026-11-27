using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PelicanXVASynth
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        
        public static Harmony harmony;
        public static GameVoices gameVoices = new GameVoices();
        public static readonly string xVaSynthPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "xVASynth", "realTimeTTS");
        public static SoundEffect voiceSound;
        public static Dictionary<string, GameVoice> voiceDict = new Dictionary<string, GameVoice>();

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

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            LoadGameVoices();

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
                configMenu.AddSectionTitle(
                    mod: ModManifest,
                    text: () => "Voices"
                );
                var voiceStrings = new Dictionary<string, string>();
                voiceStrings.Add("","none");
                foreach (var kvp in gameVoices.games)
                {
                    foreach(var v in kvp.Value)
                    {
                        voiceStrings[kvp.Key + ":" + v.id] = $"{v.name} ({kvp.Key})";
                    }
                }
                Monitor.Log($"list of {voiceStrings.Count} game voices");
                foreach (var kvp in Helper.Content.Load<Dictionary<string, string>>("Data\\NPCDispositions", ContentSource.GameContent))
                {
                    configMenu.AddTextOption(
                        mod: ModManifest,
                        name: () => kvp.Key,
                        getValue: () => voiceDict.ContainsKey(kvp.Key) ? voiceDict[kvp.Key].game + ":" + voiceDict[kvp.Key].id : "",
                        setValue: delegate(string value) { 
                            var parts = value.Split(':'); 
                            if (parts.Length != 2) 
                            { 
                                voiceDict.Remove(kvp.Key); 
                                return; 
                            } 
                            voiceDict[kvp.Key] = new GameVoice(parts[0], parts[1]); 
                            SaveGameVoices(); 
                        },
                        allowedValues: voiceStrings.Keys.ToArray(),
                        formatAllowedValue: delegate(string value) { return voiceStrings[value]; } 
                    );
                }
                
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => "Max Seconds Wait ",
                    tooltip: () => "Cancel speech synthesis if it takes longer than this",
                    getValue: () => Config.MaxSecondsWait,
                    setValue: value => Config.MaxSecondsWait = value,
                    min: 1,
                    max: 60
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => "Milliseconds To Prepare",
                    tooltip: () => "Wait this many milliseconds for the engine to generate the voice before synthesizing",
                    getValue: () => Config.MillisecondsPrepare,
                    setValue: value => Config.MillisecondsPrepare = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => "Max Letters To Prepare",
                    tooltip: () => "Only pause to prepare if the number of letters in the string is equal to or smaller than this.",
                    getValue: () => Config.MaxLettersToPrepare,
                    setValue: value => Config.MaxLettersToPrepare = value
                );
            }
        }

        private void SaveGameVoices()
        {
            List<string> output = new List<string>();
            foreach(var kvp in voiceDict)
            {
                output.Add($"{kvp.Key}:{kvp.Value.game}:{kvp.Value.id}");
            }
            Config.NPCGameVoices = string.Join(",", output);
        }

        private void LoadGameVoices()
        {
            string voicesPath = Path.Combine(xVaSynthPath, "xVASynthVoices.json");
            if (!File.Exists(voicesPath))
            {
                SMonitor.Log($"Voices file not found at {voicesPath}");
                return;
            }

            using (StreamReader reader = File.OpenText(voicesPath))
            {
                JsonSerializer serializer = new JsonSerializer();
                gameVoices = (GameVoices)serializer.Deserialize(reader, typeof(GameVoices));
                int count = 0;
                foreach (var kvp in gameVoices.games)
                {
                    count += kvp.Value.Count;
                    Monitor.Log($"Loaded {kvp.Value.Count} voices for {kvp.Key}");
                }
                Monitor.Log($"Loaded {count} voices for {gameVoices.games.Count} games", LogLevel.Debug);
            }
            foreach(var ss in Config.NPCGameVoices.Split(','))
            {
                var ngv = ss.Split(':');
                if (ngv.Length != 3)
                    continue;
                voiceDict[ngv[0]] = new GameVoice(ngv[1], ngv[2]);
            }
        }

        public static void DialogueBox_Ctor_Postfix(Dialogue dialogue)
        {
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
            PlayDialogue(__instance.Name, Text);
        }
        public static string currentDialogue = "";
        public static void PlayDialogue(Dialogue dialogue)
        {
            if (!Config.EnableMod || dialogue.speaker == null || dialogue.dialogues[dialogue.currentDialogueIndex] == currentDialogue)
                return;
            PlayDialogue(dialogue.speaker.Name, dialogue.dialogues[dialogue.currentDialogueIndex]);
        }
        public static void PlayDialogue(string name, string dialogue) {
            if (!voiceDict.ContainsKey(name))
            {
                SMonitor.Log($"No game voice set for {name}");
                return;
            }
            GameVoice voice = voiceDict[name];
            if (!gameVoices.games.ContainsKey(voice.game))
            {
                SMonitor.Log($"Game {voice.game} not found for {name}", LogLevel.Warn);
            }
            if (!gameVoices.games[voice.game].Exists(v => v.id == voice.id))
            {
                SMonitor.Log($"Voice {voice.id} for game {voice.game} not found for {name}", LogLevel.Warn);
            }
            SendToXVASynth(voice, dialogue);
        }

        private static async void SendToXVASynth(GameVoice voice, string dialogue)
        {
            currentDialogue = dialogue;
            SMonitor.Log($"Sending speech {dialogue} for voice {voice.id}, game {voice.game} to xVASynth");
            GameVoiceText text = new GameVoiceText()
            {
                gameId = voice.game,
                voiceId = voice.id,
                vol = 1f,
                text = ""
            };
            if (File.Exists(Path.Combine(xVaSynthPath, "output.wav")))
            {
                File.Delete(Path.Combine(xVaSynthPath, "output.wav"));
            }
            string speechPath = Path.Combine(xVaSynthPath, "xVASynthText.json");
            using (StreamWriter file = File.CreateText(speechPath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, text);
            }
            if (dialogue == null || dialogue.Length == 0)
                return;

            if (dialogue.Length <= Config.MaxLettersToPrepare && Config.MillisecondsPrepare > 0)
            {

                await Task.Delay(Config.MillisecondsPrepare);
            }
            text.text = dialogue;
            using (StreamWriter file = File.CreateText(speechPath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, text);
            }
            ticks = 0;
            CheckForWav();
        }

        public static int ticks = 0;
        public static bool playing = false;
        public static async void CheckForWav()
        {
            if(File.Exists(Path.Combine(xVaSynthPath, "output.wav")))
            {
                SMonitor.Log($"Playing output.wav file");
                FileStream fs = new FileStream(Path.Combine(xVaSynthPath, "output.wav"), FileMode.Open);
                voiceSound = SoundEffect.FromStream(fs);
                fs.Dispose();
                voiceSound.Play();
                File.Delete(Path.Combine(xVaSynthPath, "output.wav"));
                currentDialogue = "";
                return;
            }
            await Task.Delay(100);
            ticks++;
            if (++ticks / 10f > Config.MaxSecondsWait)
            {
                SMonitor.Log($"Timeout waiting for output.wav file");
                currentDialogue = "";
                return;
            }
            CheckForWav();
        }

    }
}