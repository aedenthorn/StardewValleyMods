using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CustomFixedDialogue
{
    public class DialoguePatches
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static string prefix = "CustomFixedDialogue";
        private static string suffix = "EndCustomFixedDialogue";
        private static string NPCPrefix = "Strings\\StringsFromCSFiles:NPC.cs.";
        private static string eventPrefix = "Strings\\StringsFromCSFiles:Event.cs.";
        private static string utilityPrefix = "Strings\\StringsFromCSFiles:Utility.cs.";
        private static string extraPrefix = "Data\\ExtraDialogue:";
        private static string charactersPrefix = "Strings\\Characters:";


        private static List<string> NPCexceptions = new List<string>()
        {
            "3954",
            "3955",
            "3981",
            "3987",
            "3969",
            "4018",
            "4020",
            "4021",
        };
        private static List<string> extraExceptions = new List<string>()
        {
            "Farm_RobinWorking_ReadyTomorrow",
            "Farm_RobinWorking1",
            "Farm_RobinWorking2",
            "SeedShop_Abigail_Drawers",
            "Wizard_Hatch",
            "Clint_NoInventorySpace",
            "Clint_StillWorking",
            "Gunther_MuseumComplete",
            "Gunther_NothingToDonate",
            "Gunther_NoArtifactsFound",
            "Sandy_PlayerClubMember",
            "MisterQi_PlayerClubMember",
            "Robin_HouseUpgrade_Accepted",
            "Robin_PamUpgrade_Accepted",
            "Mines_PlayerKilled_Robin",
            "Mines_PlayerKilled_Clint",
            "Mines_PlayerKilled_Maru_Spouse",
            "Mines_PlayerKilled_Maru_NotSpouse",
            "Mines_PlayerKilled_Linus",
            "Morris_PlayerSignedUp",
            "Morris_ComeBackLater",
            "Morris_Greeting",
            "Morris_WeekendGreeting",
            "Morris_FirstGreeting",
            "Morris_FirstGreeting_CommunityCenterComplete",
            "Morris_WeekendGreeting_CommunityCenterComplete",
            "Morris_WeekendGreeting_MembershipAvailable",
            "Morris_FirstGreeting_MembershipAvailable",
            "Morris_WeekendGreeting_SecondPlayer",
            "Morris_FirstGreeting_SecondPlayer",
            "Morris_StillProcessingOrder",
            "Morris_CommunityDevelopmentForm_PlayerMale",
            "Morris_CommunityDevelopmentForm_PlayerFemale",
            "PurchasedItem_Abigail_QualityLow",
            "PurchasedItem_Abigail_QualityHigh",
            "PurchasedItem_Caroline_QualityLow",
            "PurchasedItem_Caroline_QualityHigh",
            "PurchasedItem_Pierre_QualityLow",
            "PurchasedItem_Pierre_QualityHigh",
            "PurchasedItem_Haley",
            "PurchasedItem_Elliott",
            "PurchasedItem_Alex",
            "PurchasedItem_Leah",
            "PurchasedItem_1_QualityHigh_Willy",
            "PurchasedItem_1_QualityLow_Willy",
            "PurchasedItem_2_QualityHigh_Willy",
            "PurchasedItem_2_QualityHigh_Jodi_Willy",
            "PurchasedItem_2_QualityLow_Willy",
            "PurchasedItem_3_QualityLow_Rude_Willy",
            "PurchasedItem_3_QualityHigh_Rude_Willy",
            "PurchasedItem_3_NonRude_Willy",
            "PurchasedItem_4_Willy",
            "PurchasedItem_Pierre_QualityLow_Willy",
            "PurchasedItem_Pierre_QualityHigh_Willy",
            "Town_DumpsterDiveComment_Linus",
            "SkullCavern_100_event",
            "SkullCavern_100_event_honorable",
            "Robin_UpgradeConstruction_Festival",
            "Robin_UpgradeConstruction",
            "Robin_NewConstruction_Festival",
            "Robin_NewConstruction",
            "Robin_Instant",
            "Morris_JojaCDConfirm",
            "Morris_BuyMovieTheater",
            "Morris_TheaterBought",
            "Morris_NoMoreCD",
        };
        private static List<string> charactersAllowed = new List<string>()
        {
            "WipedMemory",
            "Divorced_bouquet",
            "Divorced_gift",
            "Saloon_goodEvent_0",
            "Saloon_goodEvent_1",
            "Saloon_goodEvent_2",
            "Saloon_goodEvent_3",
            "Saloon_goodEvent_4",
            "Saloon_badEvent_0",
            "Saloon_badEvent_1",
            "Saloon_badEvent_2",
            "Saloon_badEvent_3",
            "Saloon_badEvent_4",
            "Saloon_neutralEvent_0",
            "Saloon_neutralEvent_1",
            "Saloon_neutralEvent_2",
            "Saloon_neutralEvent_3",
            "Saloon_neutralEvent_4",
            "MovieInvite_InvitedBySomeoneElse",
            "MovieInvite_AlreadySeen",
            "MovieInvite_Invited",
            "MovieInvite_Invited_Rude",
            "MovieInvite_Invited_Child",
            "MovieInvite_Reject_Child",
            "MovieInvite_Reject",
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

        public static void Dialogue_Prefix(Dialogue __instance, ref string masterDialogue, NPC speaker)
        {
            try
            {
                FixString(speaker, ref masterDialogue);

            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Dialogue_Prefix)}:\n{ex}", LogLevel.Error);
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
        
        public static void NPC_getTermOfSpousalEndearment_Postfix(NPC __instance, ref string __result)
        {
            try
            {
                FixString(__instance, ref __result);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(NPC_getTermOfSpousalEndearment_Postfix)}:\n{ex}", LogLevel.Error);
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
        public static void convertToDwarvish_Prefix(ref string str)
        {
            try
            {
                FixString(Game1.getCharacterFromName("Dwarf"), ref str);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(convertToDwarvish_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void AddWrapperToString(string path, ref string text)
        {
            if (text.Contains("\""))
            {
                Monitor.Log($"event string, not adding wrapper: {text}");
                return;
            }
            if (path.StartsWith(extraPrefix) && !extraExceptions.Contains(path.Substring(extraPrefix.Length)))
            {
                string[] array = text.Split('/');
                for(int i = 0; i < array.Length; i++)
                {
                    array[i] = $"{prefix}{path.Replace(extraPrefix, "ExtraDialogue_")}^{array[i]}^{suffix}{path.Replace(extraPrefix, "ExtraDialogue_")}";
                }
                text = string.Join("/", array);
                Monitor.Log($"edited string: {text}");
            }
            else if (path.StartsWith(charactersPrefix) && charactersAllowed.Contains(path.Substring(charactersPrefix.Length)))
            {
                string[] array = text.Split('/');
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = $"{prefix}{path.Replace(charactersPrefix, "Characters_")}^{array[i]}^{suffix}{path.Replace(charactersPrefix, "Characters_")}";
                }
                text = string.Join("/", array);
                Monitor.Log($"edited string: {text}");
            }
            else if (
                (path.StartsWith(NPCPrefix) && !NPCexceptions.Contains(path.Substring(NPCPrefix.Length))) 
                || (path.StartsWith(eventPrefix) && eventChanges.Contains(path.Substring(eventPrefix.Length))) 
                || (path.StartsWith(utilityPrefix) && utilityChanges.Contains(path.Substring(utilityPrefix.Length)))
                )
            {
                string[] array = text.Split('/');
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = $"{prefix}{path.Replace("Strings\\StringsFromCSFiles:", "")}^{array[i]}^{suffix}{path.Replace("Strings\\StringsFromCSFiles:", "")}";
                }
                text = string.Join("/", array);
                Monitor.Log($"edited string: {text}");
            }
        }

        public static void FixString(NPC speaker, ref string input)
        {

           Monitor.Log($"checking string: {input}");

            Regex pattern1 = new Regex(prefix + @"(?<key>[^\^]+)", RegexOptions.Compiled);

            while (pattern1.IsMatch(input))
            {
                string oldput = input;
                var match = pattern1.Match(input);
                string key = match.Groups["key"].Value;
                Dictionary<string, string> dialogueDic = null;
                try
                {
                    dialogueDic = Helper.Content.Load<Dictionary<string, string>>($"Characters/Dialogue/{speaker.Name}", ContentSource.GameContent);
                }
                catch(Exception ex)
                {
                    Monitor.Log($"Error loading character dictionary for {speaker.Name}:\r\n{ex}");
                    input = input.Replace($"^{suffix}{key}", "").Replace($"{prefix}{key}^", "");
                    Monitor.Log($"reverted input: {input}");
                }


                if (dialogueDic != null && dialogueDic.ContainsKey(key))
                {
                    Regex pattern2 = new Regex(prefix + key + @"\^.*\^" + suffix + key, RegexOptions.Compiled);
                    Monitor.Log($"{speaker.Name} has dialogue for {key}", LogLevel.Debug);
                    input = pattern2.Replace(input, dialogueDic[key]);
                }
                else
                {
                    //Monitor.Log($"edited input: {input}");
                    input = input.Replace($"^{suffix}{key}", "").Replace($"{prefix}{key}^", "");
                    Monitor.Log($"reverted input: {input}");
                }
                Monitor.Log($"edited string: {input}");
                if (input == oldput)
                {
                    Monitor.Log($"Error editing input, aborting.", LogLevel.Error);
                    return;
                }
            }
        }
    }
}