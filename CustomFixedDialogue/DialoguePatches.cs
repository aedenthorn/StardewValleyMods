using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CustomFixedDialogue
{
    internal class DialoguePatches
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static string prefix = "CustomFixedDialogue";
        private static string suffix = "EndCustomFixedDialogue";
        private static string NPCPrefix = "Strings\\StringsFromCSFiles:NPC.cs.";
        private static string eventPrefix = "Strings\\StringsFromCSFiles:Event.cs.";
        private static string utilityPrefix = "Strings\\StringsFromCSFiles:Utility.cs.";
        private static string extraPrefix = "Data\\ExtraDialogue";
        private static List<string> NPCexceptions = new List<string>() 
        { 
            "3981",
            "3987",
            "3969",
        };

        private static List<string> eventChanges = new List<string>() 
        {
            "1497",
            "1498",
            "1499",
            "1500",
            "1501",
            "1503",
            "1504",
            "1632",
            "1633",
            "1634",
            "1635",
            "1736",
            "1738",
            "1801",
        };

        private static List<string> utilityChanges = new List<string>() 
        {
            "5348",
            "5349",
            "5350",
            "5351",
            "5352",
            "5353",
            "5356",
            "5357",
            "5360",
            "5361",
            "5362",
            "5363",
            "5364",
        };

        public static void Initialize(IMonitor monitor, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;
        }
        public static void LocalizedContentManager_LoadString_Postfix(string path, ref string __result)
        {
            try
            {
                AddWrapperToString(path, ref __result);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(LocalizedContentManager_LoadString_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void Dialogue_parseDialogueString_Prefix(Dialogue __instance, ref string masterString)
        {
            try
            {
                FixString(__instance.speaker, ref masterString);

            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Dialogue_parseDialogueString_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void NPC_showTextAboveHead_Prefix(NPC __instance, ref string Text)
        {
            try
            {
                FixString(__instance, ref Text);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(NPC_showTextAboveHead_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }
        public static void NPC_getHi_Postfix(NPC __instance, ref string __result)
        {
            try
            {
                FixString(__instance, ref __result);

            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(NPC_getHi_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void AddWrapperToString(string path, ref string text)
        {
            if (path.StartsWith(extraPrefix))
            {
                text = $"{prefix}{path.Replace("Data\\ExtraDialogue:", "ExtraDialogue_")}^{text}^{suffix}{path.Replace("Data\\ExtraDialogue:", "ExtraDialogue_")}";
            }
            else if ((path.StartsWith(NPCPrefix) && !NPCexceptions.Contains(path.Substring(NPCPrefix.Length))) || (path.StartsWith(eventPrefix) && eventChanges.Contains(path.Substring(eventPrefix.Length))) || (path.StartsWith(utilityPrefix) && utilityChanges.Contains(path.Substring(utilityPrefix.Length))))
            {
                text = $"{prefix}{path.Replace("Strings\\StringsFromCSFiles:", "")}^{text}^{suffix}{path.Replace("Strings\\StringsFromCSFiles:", "")}";
            }
        }

        public static void FixString(NPC speaker, ref string input)
        {

            Regex pattern1 = new Regex(prefix + @"(?<key>[^\^]+)\^(?<word>.*)\^" + suffix + @"(\k<key>)", RegexOptions.Compiled);

            while (pattern1.IsMatch(input))
            {
                var match = pattern1.Match(input);
                Dictionary<string, string> dialogueDic = null;
                try
                {
                    dialogueDic = Helper.Content.Load<Dictionary<string, string>>($"Characters/Dialogue/{speaker.Name}", ContentSource.GameContent);
                }
                catch
                {
                }

                string key = match.Groups["key"].Value;
                string text = match.Groups["word"].Value;

                if (dialogueDic != null && dialogueDic.ContainsKey(key))
                {
                    Regex pattern2 = new Regex(prefix + key + @"\^.*\^" + suffix + key, RegexOptions.Compiled);
                    Monitor.Log($"{speaker.Name} has dialogue for {key}", LogLevel.Debug);
                    input = pattern2.Replace(input, dialogueDic[key]);
                }
                else
                {
                    input = input.Replace($"{prefix}{key}^","").Replace($"^{suffix}{key}","");
                }
            }
        }
    }
}